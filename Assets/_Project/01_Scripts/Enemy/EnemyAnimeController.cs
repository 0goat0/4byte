using UnityEngine;

public class EnemyAnimeController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int IsChasing = Animator.StringToHash("IsChase");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttack");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int SpawnTrigger = Animator.StringToHash("Spawn");
    private static readonly int HitTrigger = Animator.StringToHash("Hit");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private static readonly int IdleTrigger = Animator.StringToHash("Idle");

    //boss
    private static readonly int RightSlashAttackTrigger = Animator.StringToHash("RightSlashAttack");
    private static readonly int SpinAttackTrigger = Animator.StringToHash("SpinAttack");
    private static readonly int IsSpinAttacking = Animator.StringToHash("IsSpinAttacking"); // Loop 유지용 bool


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // FSM 상태(Chase/Attack)에 맞춰 bool만 갱신
    public void SetState(EnemyStateType state)
    {
        animator.SetBool(IsChasing, state == EnemyStateType.Chase);
        //animator.SetBool(IsAttacking, state == EnemyStateType.Attack);
        if (state == EnemyStateType.Dead)
        {
            PlayDie();
        }
        else if (state == EnemyStateType.Idle)
        {
            PlayIdle();
        }
            
    }

    public void PlaySpawn() => animator.SetTrigger(SpawnTrigger);
    public void PlayIdle() => animator.SetTrigger(IdleTrigger);
    public void PlayHit() => animator.SetTrigger(HitTrigger);
    public void PlayDie() => animator.SetTrigger(DieTrigger);
    public void PlayAttack() => animator.SetTrigger(AttackTrigger);


    //boss
    public void PlayRightSlashAttack() => animator.SetTrigger(RightSlashAttackTrigger);

    public void PlaySpinAttackStart()
    {
        animator.SetTrigger(SpinAttackTrigger);
    }
    public void PlaySpinAttack() => animator.SetBool(IsSpinAttacking, true);
    public void StopSpinAttack() => animator.SetBool(IsSpinAttacking, false);

}
