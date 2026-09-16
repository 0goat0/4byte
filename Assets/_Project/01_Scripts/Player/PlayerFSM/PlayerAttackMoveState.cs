using Fusion;

public class PlayerAttackMoveState : IPlayerState
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
        if (!player.TryFindTarget(out NetworkObject target))
            return;

        player.Target = target;
        player.ChangeState(PlayerStateType.Chase);
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
