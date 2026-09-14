using UnityEngine;
using Fusion;

public class PlayerDetectState : IPlayerState
{
    public void Enter(PlayerStats player)
    {
        if (player.Mover != null) player.Mover.Stop();
    }

    public void Tick(PlayerStats player)
    {
        if (player.Target == null)
        {
            player.ChangeState(PlayerStateType.Idle);
            return;
        }
        if (player.DetectTimer.Expired(player.Runner))
        {
            player.DetectTimer = TickTimer.None;
            player.ChangeState(PlayerStateType.Chase);
        }
    }

    public void Exit(PlayerStats player)
    {

    }
}
