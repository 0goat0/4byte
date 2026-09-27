using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Building/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Building Settings")]
    public string buildingName = " ";

    [Header("Stats")]
    public int hp = 1000;
    public int attackDamage = 0;
    public int defense = 10;
    public int attackSpeed = 0;
    public GameObject Prefab;
}