using UnityEngine;
using Fusion;

public class PlayerDetectState : IPlayerState
{
    private const float DetectDuration = 1f;

    public void Enter(PlayerStats player)
    {
        player.Mover?.Stop();

        player.DetectTimer = TickTimer.CreateFromSeconds(
            player.Runner,
            DetectDuration);
    }

    public void Tick(PlayerStats player)
    {
        if (player.Target == null)
        {
            player.ChangeState(PlayerStateType.Idle);
            return;
        }

        if (!player.DetectTimer.Expired(player.Runner))
            return;

        player.DetectTimer = TickTimer.None;
        player.ChangeState(PlayerStateType.Chase);
    }

    public void Exit(PlayerStats player)
    {
        player.DetectTimer = TickTimer.None;
    }
}
