using Fusion;
using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    public void Enter(EnemyAI enemy)
    {
        //공격 상태 진입
        if(enemy.Mover != null)
        {
            enemy.Mover.Stop();
        }
        enemy.Mover.Stop();
        Debug.Log("공격시작");
        enemy.AttackCooldown = TickTimer.None;
        enemy.Animator.SetState(EnemyStateType.Attack);
    }

    public void Exit(EnemyAI enemy)
    {
        
    }

    public void Tick(EnemyAI enemy)
    {
        if (enemy.Target == null)
        {
            enemy.ChangeState(EnemyStateType.Idle);
            return;
        }
        //공격시작
        Vector3 targetPos = enemy.Target.transform.position;
        float distance = Vector3.Distance(enemy.transform.position, targetPos);

        //타겟이 공격 범위 밖으로 나가면 다시 추격
        if (distance > enemy.AttackRange)
        {
            enemy.ChangeState(EnemyStateType.Chase);
            return;
        }
        //쿨타임이 끝났으면 공격 실행
        if (enemy.AttackCooldown.ExpiredOrNotRunning(enemy.Runner))
        {
            Debug.Log("공격중");
            enemy.AttackCooldown = TickTimer.CreateFromSeconds(enemy.Runner, enemy.AttackInterval);
        }
    }
}
