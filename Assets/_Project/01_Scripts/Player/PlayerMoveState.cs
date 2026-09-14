using UnityEngine;
using Fusion;

public class PlayerMoveState : IPlayerState
{
    public void Enter(PlayerStats player)
    {

    }

    public void Tick(PlayerStats player)
    {
        Vector3 currentPos = player.transform.position;
        Vector3 targetPos = player.Target.transform.position;

        float speed = player.MoveSpeed;
        player.transform.position = Vector3.MoveTowards(currentPos, targetPos, speed * player.Runner.DeltaTime);

        if (Vector3.Distance(player.transform.position, targetPos) < 0.1f)
        {
            player.ChangeState(PlayerStateType.Idle);
        }
    }

    public void Exit(PlayerStats player)
    {

    }
}