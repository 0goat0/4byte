using UnityEngine;
using Fusion;

public class PlayerIdleState : IPlayerState
{
    public void Enter(PlayerStats player)
    {
        player.Target = null;
        player.Mover?.Stop();
    }
    public void Tick(PlayerStats player)
    {
        if (!player.TryFindTarget(out NetworkObject target))
            return;

        player.Target = target;
        player.ChangeState(PlayerStateType.Detect);
    }
    public void Exit(PlayerStats player)
    {

    }
}
