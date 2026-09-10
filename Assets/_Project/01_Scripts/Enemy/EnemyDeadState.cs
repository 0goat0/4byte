using Fusion;
using UnityEngine;

public class EnemyDeadState : IEnemyState
{
    public void Enter(EnemyAI enemy)
    {
        Debug.Log("사망");

        if(enemy.Mover != null)
        {
            enemy.Mover.Stop();
        }
        //타겟을 비워줌.
        enemy.Target = null;


        if (enemy.HasStateAuthority)
        {
            enemy.DeathTimer = TickTimer.CreateFromSeconds(enemy.Runner, enemy.DespawnDelay);
        }

        // 사망 애니메이션 재생, 사운드 재생, 골드 드랍(enemy.Data.goldDrop) 처리 등을
        // 여기서 호출해주면 됨 (연출/보상 로직)
    }

    public void Exit(EnemyAI enemy)
    {
        //이 상태는 보통 Despawn으로 끝나서 Exit이 호출되지 않지만,
        //혹시 모를 재사용에 대비해 콜라이더는 EnemyAI.Spawned()에서 다시 켜줌
    }

    public void Tick(EnemyAI enemy)
    {
        //호스트만 실제로 Despawn 판단
        if (enemy.DeathTimer.Expired(enemy.Runner))
        {
            enemy.DeathTimer = TickTimer.None;
            enemy.Runner.Despawn(enemy.Object);
        }
    }
}
