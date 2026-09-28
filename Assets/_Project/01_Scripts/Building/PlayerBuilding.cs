using Fusion;
using UnityEngine;

public class PlayerBuilding : NetworkBehaviour, ISelectable, IDamageable, IBaseBuilding, IHealthSource, IHealable
{
    [SerializeField] protected BuildingData buildingData;

    [Header("Selection")]
    [SerializeField] private Renderer _selectionIndicatorRenderer;

    public BuildingData Data => buildingData;
    public float CurrentHealth => CurrentHp;
    public float MaxHealth => buildingData != null ? buildingData.hp : 0f;
    public event System.Action<float, float> OnHealthChanged;

    [Networked, OnChangedRender(nameof(OnCurrentHpChanged))] public int CurrentHp { get; set; }
    [Networked] public NetworkBool IsDestroyed { get; set; }

    private WorldHealthBarTarget _healthBarTarget;

    public override void Spawned()
    {
        SetSelected(false);

        if (HasStateAuthority && buildingData != null)
        {
            CurrentHp = buildingData.hp;
            IsDestroyed = false;
        }

        _healthBarTarget = WorldHealthBarTarget.Attach(gameObject, this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _healthBarTarget?.Release();
    }

    private void OnCurrentHpChanged()
    {
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void SetSelected(bool isSelected)
    {
        if (_selectionIndicatorRenderer == null)
            return;

        _selectionIndicatorRenderer.enabled = isSelected;
    }
    // HealZone이 호출: 내가 소유자면 직접, 아니면 소유자에게 요청
    public void Heal(float amount)
    {
        if (HasStateAuthority) TakeDamage(-amount, null);
        else RpcHeal(amount);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcHeal(float amount)
    {
        TakeDamage(-amount, null);
    }
    public void TakeDamage(float damage, NetworkObject attacker)
    {
        if (!HasStateAuthority || IsDestroyed) return;
        if (damage < 0f) // 회복
        {
            CurrentHp = Mathf.CeilToInt(Mathf.Min(CurrentHp - damage, MaxHealth)); // 최대 체력 초과 방지
            return;
        }
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
