using Fusion;
using UnityEngine;

// 데미지를 받을 수 있는 모든 대상(플레이어, 건물, 몬스터 등)이 구현하는 인터페이스
// EnemyAttackState에서는 타겟이 이 인터페이스를 구현하고 있는지만 확인하면 되므로
// 타겟이 플레이어인지 건물인지 몰라도 공격 로직을 재사용할 수 있음.
public interface IDamageable
{
    void TakeDamage(float damage, NetworkObject attacker);
}