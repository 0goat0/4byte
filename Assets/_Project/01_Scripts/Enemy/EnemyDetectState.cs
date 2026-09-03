using Fusion;
using UnityEngine;

public class EnemyDetectState : IEnemyState
{
    private const float DetectDuration = 1f;
    public void Enter(EnemyAI enemy)
    {
        Debug.Log("Player 발견!!!");
        enemy.agent.isStopped = true;
        enemy.DetectTimer = TickTimer.CreateFromSeconds(enemy.Runner, DetectDuration);
    }

    public void Exit(EnemyAI enemy)
    {
        enemy.agent.isStopped = false;
    }

    public void Tick(EnemyAI enemy)
    {
        if(enemy.Target == null)
        {
            enemy.ChangeState(EnemyStateType.Idle);
            return;
        }

        if (enemy.DetectTimer.Expired(enemy.Runner))
        {
            enemy.ChangeState(EnemyStateType.Chase);
        }
    }
}
