using UnityEngine;
using Fusion;

public class PlayerChaseState : IPlayerState
{
    private const float RepathDistanceSqr = 0.25f;

    private Vector3 _lastTargetPosition;

    public void Enter(PlayerStats player)
    {
        if (player.Target == null)
        {
            player.ChangeState(PlayerStateType.Idle);
            return;
        }

        _lastTargetPosition = player.Target.transform.position;
        player.Mover.MoveTo(_lastTargetPosition);
    }

    public void Tick(PlayerStats player)
    {
        if (player.Target == null || !player.Target.gameObject.activeInHierarchy)
        {
            player.ChangeState(PlayerStateType.Idle);
            return;
        }

        Vector3 targetPosition = player.Target.transform.position;
        float distanceSqr =
            (targetPosition - player.transform.position).sqrMagnitude;

        if (distanceSqr > player.DetectRange * player.DetectRange)
        {
            player.ChangeState(PlayerStateType.Idle);
            return;
        }

        if (distanceSqr <= player.AttackRange * player.AttackRange)
        {
            player.ChangeState(PlayerStateType.Attack);
            return;
        }

        if ((targetPosition - _lastTargetPosition).sqrMagnitude <
            RepathDistanceSqr)
            return;

        _lastTargetPosition = targetPosition;
        player.Mover.UpdateDestination(targetPosition);
    }

    public void Exit(PlayerStats player)
    {
        player.Mover?.Stop();
    }
}
