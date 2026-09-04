using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

// NetworkObjectProviderDefault를 상속받아
// "오브젝트 풀링" 기능을 추가한 클래스입니다.
// 풀링이란: 오브젝트를 매번 생성/파괴하지 않고,
// 다 쓴 오브젝트를 재활용(비활성화 후 보관)해서 성능을 아끼는 기법입니다.
public class PooledNetworkObjectProvider : NetworkObjectProviderDefault
{
    // 프리팹 하나당 최대 몇 개까지 풀(보관함)에 쌓아둘지 정하는 값
    private int maxPoolCount = 20;

    // 프리팹(원본) 별로 "재사용 가능한 오브젝트 목록(Queue)"을 저장하는 사전(Dictionary)
    // key: 프리팹, value: 그 프리팹으로 만들어진 오브젝트들을 담은 큐(대기줄)
    private readonly Dictionary<NetworkObject, Queue<NetworkObject>> pool = new();

    // 최대 풀 개수를 외부에서 변경할 수 있게 해주는 함수
    public void SetMaxPoolCount(int count)
    {
        // 음수가 들어오지 않도록 0 이상으로 보정
        maxPoolCount = Mathf.Max(0, count);
    }

    // 미리 오브젝트를 만들어서 풀에 채워놓는 함수 (게임 시작 시 미리 준비해두면
    // 실제 플레이 중에 갑자기 생성하느라 렉 걸리는 걸 방지할 수 있음)
    // runner: 네트워크를 관리하는 주체, prefab: 만들 원본, count: 몇 개 미리 만들지
    public void Prewarm(NetworkRunner runner, NetworkObject prefab, int count)
    {
        // 프리팹이 없으면(null) 경고 출력하고 함수 종료
        if (!prefab)
        {
            Debug.LogWarning("[Pool] Prewarm 요청에 null 프리팹이 전달되었습니다.");
            return;
        }

        // 이 프리팹에 대한 큐가 아직 없으면 새로 만들어서 등록
        if (!pool.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<NetworkObject>();
            pool[prefab] = queue;
        }

        // 이미 큐에 몇 개 있는지 확인하고, 목표 개수(count)까지 몇 개 더 만들어야 하는지 계산
        int needToCreate = Mathf.Max(0, count - queue.Count);
        if (needToCreate == 0)
        {
            // 이미 충분히 있으면 더 만들 필요 없음
            return;
        }

        // 부족한 개수만큼 실제로 오브젝트를 생성
        for (int i = 0; i < needToCreate; i++)
        {
            // 부모 클래스(NetworkObjectProviderDefault)의 기본 생성 기능을 이용해 실제로 인스턴스 생성
            var inst = base.InstantiatePrefab(runner, prefab);

            // 생성된 오브젝트를 네트워크 러너 전용 씬으로 이동 (씬이 바뀌어도 유지되도록)
            runner.MoveToRunnerScene(inst.gameObject);

            // 아직 실제로 쓰이는 게 아니므로 비활성화(꺼둠) 상태로 대기
            inst.gameObject.SetActive(false);

            // 대기 목록(큐)에 추가
            queue.Enqueue(inst);
        }
    }

    // Fusion이 네트워크 오브젝트가 필요할 때 자동으로 호출하는 함수 (오버라이드)
    // "새로 만들지, 풀에서 재사용할지"를 여기서 결정합니다.
    public override NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject instance)
    {
        instance = null; // 일단 결과값을 비워둠

        // 씬 매니저가 바쁜 상태(씬 로딩 중 등)라면 지금은 만들지 말고 나중에 다시 시도하라고 알림
        if (DelayIfSceneManagerIsBusy && runner.SceneManager.IsBusy)
        {
            return NetworkObjectAcquireResult.Retry;
        }

        NetworkObject prefab;
        try
        {
            // context.PrefabId로 실제 프리팹 원본을 불러옴
            prefab = runner.Prefabs.Load(context.PrefabId, context.IsSynchronous);
        }
        catch (Exception ex)
        {
            // 로드 중 에러가 나면 로그 남기고 실패 처리
            Debug.LogError($"[Pool] Prefab load 실패: {ex}");
            return NetworkObjectAcquireResult.Failed;
        }

        // 프리팹을 못 불러온 경우
        if (!prefab)
        {
            // 동기 방식이면 아예 실패, 비동기면 나중에 다시 시도
            return context.IsSynchronous ? NetworkObjectAcquireResult.Failed : NetworkObjectAcquireResult.Retry;
        }

        // 이 프리팹에 대한 풀(큐)이 존재하는지 확인
        if (pool.TryGetValue(prefab, out var queue))
        {
            // 큐에 재사용할 오브젝트가 남아있고, 아직 instance를 못 구했다면 반복
            while (queue.Count > 0 && !instance)
            {
                // 큐에서 하나 꺼냄 (먼저 넣은 게 먼저 나옴 - FIFO)
                var pooledObj = queue.Dequeue();

                // 혹시 이미 파괴되어 null인 오브젝트라면 건너뜀
                if (!pooledObj)
                {
                    continue;
                }
                try
                {
                    // 재사용할 오브젝트를 다시 켜서(활성화) 사용 준비
                    pooledObj.gameObject.SetActive(true);
                    instance = pooledObj;
                }
                catch (MissingReferenceException)
                {
                    // Unity 오브젝트가 파괴되었는데 참조만 남아있는 경우 예외 처리 (파괴된 오브젝트는 그냥 스킵)
                }
            }
        }

        // 풀에서 재사용할 오브젝트를 못 구했다면 (풀이 비었거나 없으면)
        if (!instance)
        {
            // 새로 생성
            instance = base.InstantiatePrefab(runner, prefab);
        }

        // DontDestroyOnLoad 여부에 따라 오브젝트 위치(씬)를 다르게 처리
        if (!context.DontDestroyOnLoad)
        {
            // 씬이 바뀌어도 파괴되지 않도록 설정
            runner.MakeDontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            // 네트워크 러너 전용 씬으로 이동
            runner.MoveToRunnerScene(instance.gameObject);
        }

        // 성공적으로 오브젝트를 확보했다고 결과 반환
        return NetworkObjectAcquireResult.Success;
    }

    // Fusion이 네트워크 오브젝트를 더 이상 쓰지 않을 때(반납할 때) 자동으로 호출하는 함수 (오버라이드)
    // 여기서 "진짜로 파괴할지, 풀에 다시 담아둘지"를 결정합니다.
    public override void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
    {
        // 파괴되는 중이 아니고, 프리팹 기반 오브젝트인 경우에만 풀링 시도
        if (!context.IsBeingDestroyed && context.TypeId.IsPrefab)
        {
            // 원본 프리팹 ID를 가져와서
            var prefabId = context.TypeId.AsPrefabId;
            // 실제 프리팹 원본을 로드 (동기 방식으로)
            var prefab = runner.Prefabs.Load(prefabId, true);

            // 프리팹과 반납할 오브젝트가 둘 다 유효하다면
            if (prefab && context.Object)
            {
                // 이 프리팹에 대한 큐가 없으면 새로 만들어줌
                if (!pool.TryGetValue(prefab, out var queue))
                    pool[prefab] = queue = new Queue<NetworkObject>();

                // 최대 풀 개수 제한이 없거나(0 이하), 아직 여유가 있으면 풀에 담기
                if (maxPoolCount <= 0 || queue.Count < maxPoolCount)
                {
                    var go = context.Object.gameObject;
                    if (go)
                    {
                        // 화면에서 꺼서(비활성화) 안 보이게 하고
                        go.SetActive(false);
                        // 큐에 다시 담아 재사용 대기 상태로 전환
                        queue.Enqueue(context.Object);
                        // 여기서 함수 종료 (파괴하지 않고 재사용 대기시켰으므로)
                        return;
                    }
                }
            }
        }

        // 위 조건에 해당하지 않으면(풀에 담을 수 없는 상황이면) 부모 클래스의 기본 동작대로 진짜 파괴
        base.ReleaseInstance(runner, context);
    }

    // 특정 프리팹의 풀에 현재 몇 개가 대기 중인지 확인하는 함수
    public int GetPoolCount(NetworkObject prefab) => pool.TryGetValue(prefab, out var q) ? q.Count : 0;

    // 특정 프리팹의 풀을 통째로 비우는 함수 (주의: 큐 안의 오브젝트 자체를 파괴하지는 않고
    // 목록에서만 제거하므로, 실제 오브젝트 정리가 필요하면 별도 처리가 필요할 수 있음)
    public void ClearPool(NetworkObject prefab)
    {
        if (pool.TryGetValue(prefab, out var q))
            q.Clear();
    }
}