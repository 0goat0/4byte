using Fusion;
using UnityEngine;
using WebSocketSharp;

public class PlayerBuilding : NetworkBehaviour, ISelectable
{
    [SerializeField] protected BuildingData buildingData;
    public BuildingData Data => buildingData;

    [Networked] public int CurrentHp { get; set; }
    [Networked] public bool IsDestroyed { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority && buildingData != null)
        {
            CurrentHp = buildingData.hp;
            IsDestroyed = false;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!HasStateAuthority || IsDestroyed) return;

        int finalDamage = Mathf.Max((int)damage - buildingData.defense, 1);
        CurrentHp = Mathf.Clamp(CurrentHp - finalDamage, 0, buildingData.hp);

        if (CurrentHp <= 0) DestroyBuilding();
    }

    private void DestroyBuilding()
    {
        IsDestroyed = true;
        Runner.Despawn(Object);
    }




}
