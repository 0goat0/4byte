using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    public void Enter(EnemyAI enemy)
    {
        //Debug.Log("추적시작");
    }

    public void Exit(EnemyAI enemy)
    {
       
    }

    public void Tick(EnemyAI enemy)
    {
        //Debug.Log("추적중");
        if (enemy.Target == null)
        {
            enemy.ChangeState(EnemyStateType.Idle);
            return;
        }
        Vector3 targetPos = enemy.Target.transform.position;
        enemy.agent.SetDestination(targetPos);
    }
}
