using UnityEngine;
using Fusion;

public class PlayerMoveState : IPlayerState
{
    private PlayerStats _player;

    public void Enter(PlayerStats player)
    {
        _player = player;

        player.Mover.OnDestinationReached += HandleDestinationReached;
        player.Mover.MoveTo(player.CommandDestination);
    }

    public void Tick(PlayerStats player)
    {
    }

    public void Exit(PlayerStats player)
    {
        player.Mover.OnDestinationReached -= HandleDestinationReached;
        player.Mover.Stop();

        _player = null;
    }

    private void HandleDestinationReached()
    {
        _player?.ChangeState(PlayerStateType.Idle);
    }
}
