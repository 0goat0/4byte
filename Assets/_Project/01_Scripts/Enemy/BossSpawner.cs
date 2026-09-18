using Fusion;
using UnityEngine;

// 건물(오브젝트)에 붙이는 컴포넌트.
// 이 건물이 파괴되면(HP 0) 지정된 보스 프리팹을 1마리 스폰
// 보스 프리팹은 기존 EnemyAI/EnemyData/상태머신을 그대로 재사용해도 되고,
// 필요하면 보스 전용 EnemyData(체력/공격력만 크게)만 새로 만들면 됌.
public class BossSpawner : NetworkBehaviour, IDamageable
{
    [Header("Boss Prefab")]
    [SerializeField] private NetworkObject bossPrefab;

    // 지정하지 않으면 이 건물의 위치에서 스폰
    [SerializeField] private Transform spawnPoint;

    [Header("Building Health")]
    [SerializeField] private float maxHp;
    [Networked] private float CurrentHp { get; set; }

    // 건물이 파괴되었는지 여부 (다른 클라이언트/시스템에서 참조 가능하도록 공개)
    [Networked] public NetworkBool IsDestroyed { get; set; }

    public override void Spawned()
    {
        // 호스트만 초기 체력 설정
        if (HasStateAuthority)
        {
            CurrentHp = maxHp;
            IsDestroyed = false;
        }

        var pooledProvider = Runner.GetComponent<PooledNetworkObjectProvider>();

        if (pooledProvider != null)
        {
            //미리 풀에 넣어두기
            pooledProvider.Prewarm(Runner, bossPrefab, 1);
        }

    }

    public void TakeDamage(float damage, NetworkObject attacker)
    {
        // 데미지 판정은 StateAuthority(호스트)에서만
        if (!HasStateAuthority)
        {
            return;
        }
        // 이미 파괴된 건물이면 무시
        if (IsDestroyed)
        {
            return;
        }

        CurrentHp -= damage;
        if (CurrentHp <= 0f)
        {
            CurrentHp = 0f;
            OnBuildingDestroyed();
        }
    }

    private void OnBuildingDestroyed()
    {
        if (IsDestroyed)
        {
            return;
        }
        IsDestroyed = true;

        Debug.Log("[BossSpawner] 건물 파괴 - 보스 스폰");

        // 여기서 파괴 이펙트/사운드/카메라 흔들림 등 연출 로직을 추가하면 됨

        SpawnBoss();

        // 건물 자체는 네트워크에서 제거
        Runner.Despawn(Object);
    }

    private void SpawnBoss()
    {
        if (!bossPrefab)
        {
            Debug.LogWarning("[BossSpawner] bossPrefab이 지정되지 않았습니다.");
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;

        Runner.Spawn(bossPrefab, pos, Quaternion.identity);
    }
}