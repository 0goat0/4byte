using UnityEngine;

public class PlayerAnimeController : MonoBehaviour
{
    private Animator animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMove");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttack");
    private static readonly int DieTrigger = Animator.StringToHash("OnDie");


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

        bool isAttacking =
            state == PlayerStateType.Attack;

        animator.SetBool(IsMoving, isMoving);
        animator.SetBool(IsAttacking, state == PlayerStateType.Attack);

        if (state == PlayerStateType.Dead)
        {
            animator.SetTrigger(DieTrigger);
        }
    }
}
