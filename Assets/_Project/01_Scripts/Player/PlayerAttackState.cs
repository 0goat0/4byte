using UnityEngine;
using Fusion;

public class PlayerAttackState : IPlayerState
{
    public void Enter(PlayerStats player)
    {
        player.AttackCooldown = TickTimer.None;
        Debug.Log($"{player.playerName.Value} 공격 상태 진입");
    }
    public void Tick(PlayerStats player)
    {
        // 1. 타겟이 없으면 대기로 복귀
        if (player.Target == null)
        {
            // player.ChangeState(PlayerStateType.Idle); 
            return;
        }

        // 2. 거리 계산 및 사거리 체크
        float distance = Vector3.Distance(player.transform.position, player.Target.transform.position);
        if (distance > player.AttackRange)
        {
            // player.ChangeState(PlayerStateType.Chase); 
            return;
        }

        // 쿨타임이 끝났을 때의 공격
        if (player.AttackCooldown.ExpiredOrNotRunning(player.Runner))
        {
            Debug.Log($"{player.playerName.Value}가 {player.Target.name}을 공격 중!");

            // 데미지 처리
            if (player.Target.TryGetComponent<PlayerStats>(out var enemy))
            {
                // player.TakeDamage(enemy.attackDamage);
            }

            // 쿨타임 타이머 재설정
            player.AttackCooldown = TickTimer.CreateFromSeconds(player.Runner, player.AttackInterval);
        }
    }
    public void Exit(PlayerStats player)
    {

    }

}