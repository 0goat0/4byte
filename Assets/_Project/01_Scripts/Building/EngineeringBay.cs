using UnityEngine;
using Fusion;

public class EngineeringBay : PlayerBuilding
{
    public bool UpgradeUnit(PlayerStats targetUnit, bool isAttack)
    {
        if (targetUnit == null || IsDestroyed) return false;

        targetUnit.RpcUpgradeStats(isAttack);
        return true;
    }
}