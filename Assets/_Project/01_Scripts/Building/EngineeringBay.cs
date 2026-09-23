using UnityEngine;
using Fusion;

public class EngineeringBay : PlayerBuilding
{
    public bool UpgradeUnit(PlayerStats targetUnit, bool isAttack)
    {
        if (targetUnit == null || IsDestroyed) return false;
        //if (targetUnit.Object == null || !targetUnit.Object.IsValid) return false;


        targetUnit.RpcUpgradeStats(isAttack);
        return true;
    }
}