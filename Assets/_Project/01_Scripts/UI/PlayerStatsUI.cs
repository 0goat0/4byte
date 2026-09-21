using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private PlayerInteractionState playerInteractionState;
    [SerializeField] private TextMeshProUGUI unitInfoText;
    [SerializeField] private Button attackUpButton;
    [SerializeField] private Button defenseUpButton;

    private PlayerStats _trackedUnit;
    private PlayerBuilding _currentBuilding;

    private void Awake()
    {
        attackUpButton?.onClick.AddListener(() => UpgradeAllUnits(true));
        defenseUpButton?.onClick.AddListener(() => UpgradeAllUnits(false));
    }

    private void OnEnable() => playerInteractionState.OnSelectionChanged += HandleSelectionChanged;
    private void OnDisable() => playerInteractionState.OnSelectionChanged -= HandleSelectionChanged;

    private void Update()
    {
        if (_trackedUnit != null) UpdateUnitInfoUI();
        else if (_currentBuilding != null) UpdateBuildingInfoUI();
    }

    private void HandleSelectionChanged()
    {
        _trackedUnit = null;
        _currentBuilding = null;

        if (playerInteractionState?.InfoTarget is not Component target)
        {
            if (unitInfoText != null) unitInfoText.text = "";
            return;
        }

        _trackedUnit = target.GetComponentInParent<PlayerStats>(true);
        _currentBuilding = target.GetComponentInParent<PlayerBuilding>(true);

        if (_trackedUnit != null) UpdateUnitInfoUI();
        else if (_currentBuilding != null) UpdateBuildingInfoUI();
    }

    private void UpdateUnitInfoUI()
    {
        if (_trackedUnit == null || _trackedUnit.Data == null || unitInfoText == null) return;

        var data = _trackedUnit.Data;

        unitInfoText.text = $"<line-height=85%><size=150%><b>{data.PlayerName}</b></size>\n\n" +
                            $"Size: {data.size}<pos=45%>Attack Type: {data.attackType}\n\n" +
                            $"HP: {(int)_trackedUnit.CurrentHp} / {(int)data.hp}<pos=45%>Defense: {_trackedUnit.defense}\n\n" +
                            $"Attack: {_trackedUnit.attackDamage}<pos=45%>AttackSpeed: {_trackedUnit.attackSpeed}\n\n" +
                            $"MoveSpeed: {_trackedUnit.MoveSpeed}</line-height>";
    }

    private void UpdateBuildingInfoUI()
    {
        if (_currentBuilding == null || _currentBuilding.Data == null || unitInfoText == null) return;

        var labData = _currentBuilding.Data;

        unitInfoText.text = $"<line-height=85%><size=150%><b>{labData.buildingName}</b></size>\n\n" +
                            $"HP: {_currentBuilding.CurrentHp} / {labData.hp}<pos=45%>Defense: {labData.defense}</line-height>";
    }

    private void UpgradeAllUnits(bool isAttack)
    {
        if (_currentBuilding == null) return;

        if (_currentBuilding is not EngineeringBay engineeringBay) return;

        foreach (var unit in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
        {
            engineeringBay.UpgradeUnit(unit, isAttack);
        }
    }
}