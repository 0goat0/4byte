using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private const float AttackEnterBuffer = 1f;
    public void Enter(EnemyAI enemy)
    {
        //Debug.Log("추적시작");
        enemy.Animator.SetState(EnemyStateType.Chase);
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

        //타겟과의 거리를 계속 계산
        //float distance = Vector3.Distance(enemy.transform.position, targetPos);
        float distance = DistanceUtil.GetDistanceToTarget(enemy.transform.position, enemy.Target);
        //설정한 공격 사거리 안에 들어오면 공격상태 진입
        if (distance <= enemy.AttackRange)
        {
            enemy.ChangeState(EnemyStateType.Attack);
            return;
        }
        if(distance - AttackEnterBuffer > enemy.DetectRange)
        {
            enemy.ChangeState(EnemyStateType.Idle);
            enemy.Target = null;
            return;
        }

        //enemy.agent.SetDestination(targetPos);
        enemy.Mover.MoveTo(targetPos);
    }
}
