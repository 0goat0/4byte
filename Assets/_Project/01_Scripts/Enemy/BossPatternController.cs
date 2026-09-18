using System.Collections.Generic;
using Fusion;
using UnityEngine;


// 보스 전용 패턴 인터페이스.
// IEnemyState와 같은 형태로 설계하여 기존 상태머신 스타일과 일관성을 맞춤.
// 보스 종류를 늘릴 때는 이 인터페이스를 구현하는 MonoBehaviour를 새로 만들고
// 보스 프리팹의 BossPatternController에 등록만 해주면 됨.
public interface IBossPattern
{
    // 쿨타임 구분 및 로그용 식별자
    string PatternId { get; }

    // 이 패턴이 끝난 뒤 다시 선택 가능해지기까지의 쿨타임(초)
    float Cooldown { get; }

    // 이 패턴이 발동 가능한 타겟과의 거리 범위
    float MinRange { get; }
    float MaxRange { get; }

    // 거리 조건 외에 추가로 체크하고 싶은 조건(HP 임계값 등)이 있을 때 사용
    bool CanExecute(EnemyAI enemy);

    void Enter(EnemyAI enemy);
    void Tick(EnemyAI enemy);
    bool IsFinished(EnemyAI enemy);
    void Exit(EnemyAI enemy);
}

// 보스 전용 컴포넌트. EnemyAI가 붙어있는 보스 프리팹에 나란히 붙여서 사용.
// 여러 종류의 보스는 이 컴포넌트에 서로 다른 IBossPattern 목록을
// 인스펙터에서 연결하는 것만으로 구성 가능 (근접보스/원거리보스 등).
//
// 사용법:
// 1) 보스 프리팹에 IBossPattern을 구현한 MonoBehaviour들(Basic/Swing/Charge 등)을 같이 붙인다.
// 2) 이 컴포넌트의 Pattern Behaviours 배열에 그 컴포넌트들을 드래그해서 등록한다.
// 3) EnemyAttackState가 이 컴포넌트를 감지해서 자동으로 패턴 기반 로직으로 위임한다.
[RequireComponent(typeof(EnemyAI))]
public class BossPatternController : NetworkBehaviour
{
    [Header("연결된 EnemyAI (비워두면 자동으로 찾음)")]
    [SerializeField] private EnemyAI enemy;

    [Header("이 보스가 사용할 패턴들 (IBossPattern을 구현한 컴포넌트만 등록)")]
    [SerializeField] private MonoBehaviour[] patternBehaviours;

    private readonly List<IBossPattern> patterns = new List<IBossPattern>();
    private readonly Dictionary<string, TickTimer> cooldownMap = new Dictionary<string, TickTimer>();

    private IBossPattern currentPattern;

    // 클라이언트 연출(예고 이펙트 등) 동기화를 위한 현재 패턴 인덱스. -1이면 패턴 없음.
    [Networked, OnChangedRender(nameof(OnPatternIndexChanged))]
    public int CurrentPatternIndex { get; set; }

    public bool HasPatternRunning => currentPattern != null;

    private void Awake()
    {
        if (enemy == null)
        {
            enemy = GetComponent<EnemyAI>();
        }

        patterns.Clear();
        foreach (var behaviour in patternBehaviours)
        {
            if (behaviour is IBossPattern pattern)
            {
                patterns.Add(pattern);
            }
            else if (behaviour != null)
            {
                Debug.LogWarning($"[BossPatternController] {behaviour.name}은(는) IBossPattern을 구현하지 않았습니다.");
            }
        }
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            CurrentPatternIndex = -1;
        }
    }

    // EnemyAttackState.Tick에서 매 틱 호출됨. (호스트에서만 호출되도록 상위에서 보장)
    public void TickPattern(EnemyAI owner)
    {
        if (currentPattern != null)
        {
            currentPattern.Tick(owner);

            if (currentPattern.IsFinished(owner))
            {
                EndCurrentPattern();
            }
            return;
        }

        IBossPattern next = SelectPattern(owner);
        if (next != null)
        {
            StartPattern(owner, next);
        }
        // 발동 가능한 패턴이 없으면 이번 틱은 대기 (다음 틱에 다시 시도)
    }

    private IBossPattern SelectPattern(EnemyAI owner)
    {
        if (owner.Target == null)
        {
            return null;
        }

        float distance = DistanceUtil.GetDistanceToTarget(owner.transform.position, owner.Target);

        List<IBossPattern> candidates = new List<IBossPattern>();
        foreach (var pattern in patterns)
        {
            if (IsOnCooldown(pattern))
            {
                continue;
            }
            if (distance < pattern.MinRange || distance > pattern.MaxRange)
            {
                continue;
            }
            if (!pattern.CanExecute(owner))
            {
                continue;
            }
            candidates.Add(pattern);
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        // 후보 중 랜덤 선택. 필요하면 가중치/우선순위 방식으로 교체 가능.
        return candidates[Random.Range(0, candidates.Count)];
    }

    private bool IsOnCooldown(IBossPattern pattern)
    {
        if (!cooldownMap.TryGetValue(pattern.PatternId, out TickTimer timer))
        {
            return false;
        }
        return !timer.ExpiredOrNotRunning(Runner);
    }

    private void StartPattern(EnemyAI owner, IBossPattern pattern)
    {
        currentPattern = pattern;
        cooldownMap[pattern.PatternId] = TickTimer.CreateFromSeconds(Runner, pattern.Cooldown);

        CurrentPatternIndex = patterns.IndexOf(pattern);

        pattern.Enter(owner);
    }

    private void EndCurrentPattern()
    {
        currentPattern?.Exit(enemy);
        currentPattern = null;
        CurrentPatternIndex = -1;
    }

    // 모든 클라이언트에서 CurrentPatternIndex가 바뀔 때 호출됨.
    // 예고 이펙트/사운드처럼 클라이언트에서도 재생해야 하는 연출이 있으면 여기서 처리.
    private void OnPatternIndexChanged()
    {
        // 예: patterns[CurrentPatternIndex] 에 맞는 이펙트 재생
    }
}