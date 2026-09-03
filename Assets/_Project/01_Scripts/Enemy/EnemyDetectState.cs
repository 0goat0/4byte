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
        
    }

    public void Tick(EnemyAI enemy)
    {
       
    }
}
