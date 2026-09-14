using UnityEngine;
using Fusion;

public enum PlayerSize
{
    Small, //소형
    Medium, //중형
    Large //대형
}
public enum PlayerAttackType
{
    Melee, //근접
    Ranged, //원거리
    AoE //광역
}
[CreateAssetMenu(fileName = "PlayerData", menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    //이름
    public string PlayerName;
    //사이즈
    public PlayerSize size;
    //공격타입
    public PlayerAttackType attackType;
    //체력, 공격력, 방어력, 공격속도, 이동속도
    public float hp, attack, defense, attackSpeed, moveSpeed;
    //적 프리펩
    public GameObject Prefab;
}
