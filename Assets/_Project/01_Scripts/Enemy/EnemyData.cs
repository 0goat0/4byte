using UnityEngine;

public enum EnemySize
{
    Small, //소형
    Medium, //중형
    Large //대형
}
public enum EnemyAttackType
{
    Melee, //근접
    Ranged, //원거리
    AoE //광역
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    //이름
    public string enemyName;
    //사이즈
    public EnemySize size;
    //공격타입
    public EnemyAttackType attackType;
    //체력, 공격력, 방어력, 공격속도, 이동속도
    public float hp, attack, defense, attackSpeed, moveSpeed;
    //드랍되는 골드량
    public int goldDrop;
    //적 프리펩
    public GameObject Prefab;
}
