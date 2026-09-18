using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Building/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Building Settings")]
    public string buildingName = "Engineering Bay";

    [Header("Stats")]
    public int hp = 100;
    public int attackDamage = 0;
    public int defense = 10;
    public int attackSpeed = 5;
    public GameObject Prefab;
}