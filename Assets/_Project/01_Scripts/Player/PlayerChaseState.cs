using UnityEngine;
using Fusion;

public class PlayerChaseState : IPlayerState
{
    public void Enter(PlayerStats player)
    {
        Debug.Log("추적시작");
        player.Animator.SetState(PlayerStateType.Chase);
    }

    public void Tick(PlayerStats player)
    {
        if (player.Target == null)
        {
            player.ChangeState(PlayerStateType.Idle);
            return;
        }

        float distance = Vector3.Distance(player.transform.position, player.Target.transform.position);

        if (distance > player.DetectRange)
        {
            player.ChangeState(PlayerStateType.Idle);
            return;
        }

        if (distance <= player.AttackRange)
        {
            player.ChangeState(PlayerStateType.Attack);
            return;
        }

        if (player.Mover != null)
        {
            player.Mover.MoveTo(player.Target.transform.position);
        }
    }

    public void Exit(PlayerStats player)
    {
        if (player.Mover != null) player.Mover.Stop();
    }
}
