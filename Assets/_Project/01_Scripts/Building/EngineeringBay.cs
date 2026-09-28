using UnityEngine;
using Fusion;

public class EngineeringBay : PlayerBuilding
{
    [Header("Upgrade Settings")]
    [SerializeField] private int upgradeCost = 1; 

    private float _lastUpgradeTime = 0f;

    public void TryUpgradeUnit(PlayerRef requestPlayer, PlayerStats targetUnit, bool isAttack)
    {
        if (targetUnit == null || IsDestroyed) return;

        // RPC 요청
        RpcRequestUpgrade(requestPlayer, targetUnit, isAttack);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcRequestUpgrade(PlayerRef requestPlayer, PlayerStats targetUnit, bool isAttack)
    {
        if (!HasStateAuthority) return;

        if (targetUnit == null || IsDestroyed) return;

        if (Time.time - _lastUpgradeTime < 0.1f) return;

        if (targetUnit.Kills < upgradeCost) return;

        targetUnit.Kills -= upgradeCost;

        if (isAttack)
        {
            targetUnit.AttackLevel++;
            targetUnit.attackDamage += 1f; 
        }
        else
        {
            targetUnit.DefenseLevel++;
            targetUnit.defense += 1f;
        }
    }
}