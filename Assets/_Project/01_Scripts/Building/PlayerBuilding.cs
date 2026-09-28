using Fusion;
using UnityEngine;

public class PlayerBuilding : NetworkBehaviour, ISelectable, IDamageable, IBaseBuilding
{
    [SerializeField] protected BuildingData buildingData;

    [Header("Selection")]
    [SerializeField] private Renderer _selectionIndicatorRenderer;

    public BuildingData Data => buildingData;

    [Networked] public int CurrentHp { get; set; }
    [Networked] public NetworkBool IsDestroyed { get; set; }

    public override void Spawned()
    {
        SetSelected(false);

        if (HasStateAuthority && buildingData != null)
        {
            CurrentHp = buildingData.hp;
            IsDestroyed = false;
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (_selectionIndicatorRenderer == null)
            return;

        _selectionIndicatorRenderer.enabled = isSelected;
    }

    public void TakeDamage(float damage, NetworkObject attacker)
    {
        if (!HasStateAuthority || IsDestroyed) return;

        int finalDamage = Mathf.Max((int)damage - buildingData.defense, 1);
        CurrentHp = Mathf.Clamp(CurrentHp - finalDamage, 0, buildingData.hp);

        if (CurrentHp <= 0)
        {
            DestroyBuilding();
        }
    }

    private void DestroyBuilding()
    {
        IsDestroyed = true;
        Runner.Despawn(Object);
    }
}
