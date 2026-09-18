using UnityEngine;

public class EngineeringBay : MonoBehaviour
{
    [SerializeField] private BuildingData labData;

    public BuildingData LabData => labData;

    public bool UpgradeUnit(PlayerStats targetUnit)
    {
        if (labData == null || targetUnit == null) return false;

        var fieldInfo = typeof(PlayerStats).GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fieldInfo == null) return false;

        PlayerData unitData = fieldInfo.GetValue(targetUnit) as PlayerData;
        if (unitData == null) return false;

        targetUnit.defense += labData.defense;
        targetUnit.attackDamage += labData.attackDamage;


        Debug.Log("업그레이드");
        return true;
    }
}