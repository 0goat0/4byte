using Fusion;
using UnityEngine;

// 패턴 2: 광역 휘두르기
// 보스 주변 반경에 있는 모든 타겟에게 데미지를 주는 패턴.
// 여러 플레이어가 붙어서 때리는 상황을 견제하는 용도.
// telegraphDuration 동안은 예고(연출)만 하고, 그 시점에 실제 판정을 넣어서
// 플레이어가 회피할 시간을 준다.
public class BossSwingAttackPattern : MonoBehaviour, IBossPattern
{
    [Header("발동 조건")]
    [SerializeField] private float minRange;
    public float MinRange { get { return minRange; } }
    [SerializeField] private float maxRange;
    public float MaxRange { get { return maxRange; } }

    [Header("쿨타임/데미지")]
    [SerializeField] private float cooldown;
    public float Cooldown {  get { return cooldown; } }
    [SerializeField] private float swingRadius;
    [SerializeField] private float damageMultiplier;
    [SerializeField] private LayerMask targetLayerMask; // 비워두면 enemy.TargetLayerMask 사용

    [Header("타이밍")]
    [SerializeField] private float telegraphDuration; // 예고 시간 (이때 이펙트/애니메이션 재생)
    [SerializeField] private float recoveryDuration;  // 타격 후 경직

    public string PatternId => "BossSwingAttack";
   
    private float elapsed;
    private bool damageApplied;

    public bool CanExecute(EnemyAI enemy) 
    {
        return enemy.Target != null;
    } 

    public void Enter(EnemyAI enemy)
    {
        elapsed = 0f;
        damageApplied = false;

        enemy.Mover?.Stop();
        Debug.Log("[Boss] 휘두르기 예고");

        // TODO: 여기서 예고 이펙트/애니메이션 트리거를 재생.
        // 클라이언트 연출까지 동기화하려면 BossPatternController.OnPatternIndexChanged에서 처리.
    }

    public void Tick(EnemyAI enemy)
    {
        elapsed += enemy.Runner.DeltaTime;

        if (!damageApplied && elapsed >= telegraphDuration)
        {
            damageApplied = true;
            ApplySwingDamage(enemy);
        }
    }

    public bool IsFinished(EnemyAI enemy) 
    {
        return elapsed >= telegraphDuration + recoveryDuration;
    } 

    public void Exit(EnemyAI enemy)
    {
    }

    private void ApplySwingDamage(EnemyAI enemy)
    {
        LayerMask mask = targetLayerMask.value != 0 ? targetLayerMask : enemy.TargetLayerMask;
        Collider[] hits = Physics.OverlapSphere(enemy.transform.position, swingRadius, mask);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(enemy.Data.attack * damageMultiplier, enemy.Object);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;

        // 이 패턴이 발동 가능한 최소/최대 거리 (타겟과의 거리 조건)
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(origin, minRange);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(origin, maxRange);

        // 실제 데미지가 들어가는 휘두르기 반경
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, swingRadius);
    }
}
