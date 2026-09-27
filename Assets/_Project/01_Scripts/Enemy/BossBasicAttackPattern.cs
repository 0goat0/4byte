using UnityEngine;

// 패턴 1: 기본 근접 공격
// 기존 EnemyAttackState의 단일 타격 로직을 패턴 형태로 옮긴 버전.
// 다른 패턴들의 쿨타임이 다 돌고 있을 때 기본으로 나가는 평타 역할.
public class BossBasicAttackPattern : MonoBehaviour, IBossPattern
{
    [Header("발동 조건")]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange; // EnemyAI의 AttackRange와 맞춰서 세팅 권장

    [Header("쿨타임/데미지")]
    [SerializeField] private float cooldown ;
    [SerializeField] private float damageMultiplier;

    [Header("타이밍")]
    [SerializeField] private float animationDuration; // 공격 모션 총 길이
    [SerializeField] private float hitTiming;         // 모션 중 실제 타격 판정 시점

    public string PatternId => "BossBasicAttack";
    public float Cooldown => cooldown;
    public float MinRange => minRange;
    public float MaxRange => maxRange;

    private float elapsed;
    private bool damageApplied;

    public bool CanExecute(EnemyAI enemy) => enemy.Target != null;

    public void Enter(EnemyAI enemy)
    {
        elapsed = 0f;
        damageApplied = false;

        enemy.Mover?.Stop();
        enemy.FaceTargetInstant(enemy.Target.transform.position);
        enemy.Animator.PlayRightSlashAttack();
        Debug.Log("[Boss] 기본 공격");
    }

    public void Tick(EnemyAI enemy)
    {
        elapsed += enemy.Runner.DeltaTime;

        if (!damageApplied && elapsed >= hitTiming)
        {
            damageApplied = true;
            ApplyDamage(enemy);
        }
    }

    public bool IsFinished(EnemyAI enemy) => elapsed >= animationDuration;

    public void Exit(EnemyAI enemy)
    {
    }

    private void ApplyDamage(EnemyAI enemy)
    {
        if (enemy.Target == null)
        {
            return;
        }

        IDamageable damageable = enemy.Target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(enemy.Data.attack * damageMultiplier, enemy.Object);
        }
    }
    public void ShowTelegraph(EnemyAI enemy) { }
    public void HideTelegraph(EnemyAI enemy) { }
}
