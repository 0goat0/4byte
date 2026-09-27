using Fusion;
using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private const float AttackExitBuffer = 1.5f;

    // 보스 프리팹에만 존재. 일반 몬스터는 null이라 기존 로직 그대로 동작함.
    private BossPatternController patternController;
    public void Enter(EnemyAI enemy)
    {
        enemy.Mover?.Stop();
        Debug.Log("공격시작");
        //enemy.Animator.SetState(EnemyStateType.Attack);

        patternController = enemy.GetComponent<BossPatternController>();
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
        enemy.FaceTarget(targetPos);
        float distance = DistanceUtil.GetDistanceToTarget(enemy.transform.position, enemy.Target);

        // 보스가 패턴(돌진 등)을 실행 중일 때는 사거리 체크로 인해
        // 중간에 추격 상태로 끊기지 않도록 건너뜀
        bool isRunningPattern = patternController != null && patternController.HasPatternRunning;

        //타겟이 공격 범위 밖으로 나가면 다시 추격
        if (!isRunningPattern && distance > enemy.AttackRange + AttackExitBuffer)
        {
            enemy.ChangeState(EnemyStateType.Chase);
            return;
        }

        // 보스(BossPatternController가 붙은 경우) - 패턴 기반 로직으로 위임
        if (patternController != null)
        {
            patternController.TickPattern(enemy);
            return;
        }

        //일반 적 기본 단일 공격
        //쿨타임이 끝났으면 공격 실행
        if (enemy.AttackCooldown.ExpiredOrNotRunning(enemy.Runner))
        {
            Debug.Log("공격중");
            enemy.AttackCooldown = TickTimer.CreateFromSeconds(enemy.Runner, enemy.AttackInterval);
            enemy.Animator.PlayAttack();
            IDamageable damageable = enemy.Target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(enemy.Data.attack, enemy.Object);
            }
        }
    }
}
