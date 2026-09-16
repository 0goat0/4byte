using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private const float AttackEnterBuffer = 1f;
    private const float RepathDistanceSqr = 0.25f;

    private Vector3 _lastTargetPosition;

    public void Enter(EnemyAI enemy)
    {
        //Debug.Log("추적시작");
        enemy.Animator.SetState(EnemyStateType.Chase);

        _lastTargetPosition = enemy.Target.transform.position;
        enemy.Mover.MoveTo(_lastTargetPosition);
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

        // 타겟이 기존 위치에서 일정거리 벗어날 경우 목적지 재설정
        if ((targetPos - _lastTargetPosition).sqrMagnitude < RepathDistanceSqr)
            return;

        _lastTargetPosition = targetPos;
        enemy.Mover.UpdateDestination(targetPos);
    }
}
