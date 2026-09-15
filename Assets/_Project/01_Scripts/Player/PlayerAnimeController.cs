using UnityEngine;

public class PlayerAnimeController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int IsChasing = Animator.StringToHash("IsChase");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttack");
    private static readonly int SpawnTrigger = Animator.StringToHash("Spawn");
    private static readonly int HitTrigger = Animator.StringToHash("Hit");
    private static readonly int DieTrigger = Animator.StringToHash("Die");


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // FSM 상태(Chase/Attack)에 맞춰 bool만 갱신
    public void SetState(PlayerStateType state)
    {
        bool isMoving =
            state == PlayerStateType.Move ||
            state == PlayerStateType.Chase ||
            state == PlayerStateType.AttackMove;

        animator.SetBool(IsChasing, isMoving);
        animator.SetBool(IsAttacking, state == PlayerStateType.Attack);
    }

    public void PlaySpawn() => animator.SetTrigger(SpawnTrigger);
    public void PlayHit() => animator.SetTrigger(HitTrigger);
    public void PlayDie() => animator.SetTrigger(DieTrigger);
}
