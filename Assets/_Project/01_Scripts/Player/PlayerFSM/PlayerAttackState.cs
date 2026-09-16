using UnityEngine;
using Fusion;

public class PlayerAttackState : IPlayerState
{
    public void Enter(PlayerStats player)
    {
        player.Mover?.Stop();
        player.AttackCooldown = TickTimer.None;
    }

    public void Tick(PlayerStats player)
    {
        if (player.Target == null)
        {
            player.ChangeState(PlayerStateType.Idle);
            return;
        }

        float distanceSqr =
            (player.Target.transform.position - player.transform.position)
            .sqrMagnitude;

        if (distanceSqr > player.AttackRange * player.AttackRange)
        {
            player.ChangeState(PlayerStateType.Chase);
            return;
        }

        if (!player.AttackCooldown.ExpiredOrNotRunning(player.Runner))
            return;

        if (player.Target.TryGetComponent(out IDamageable enemy))
            enemy.TakeDamage(player.attackDamage, player.Object);

        player.AttackCooldown = TickTimer.CreateFromSeconds(
            player.Runner,
            player.AttackInterval);
    }
    public void Exit(PlayerStats player)
    {

    }

}
