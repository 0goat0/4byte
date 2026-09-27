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
    private EnemyAI _trackedEnemy;

    private PlayerBuilding _currentBuilding;

    private void Awake()
    {
        attackUpButton?.onClick.AddListener(() => UpgradeAllUnits(true));
        defenseUpButton?.onClick.AddListener(() => UpgradeAllUnits(false));

        SetUpgradeButtonsActive(false);
    }

    private void OnEnable() => playerInteractionState.OnSelectionChanged += HandleSelectionChanged;
    private void OnDisable() => playerInteractionState.OnSelectionChanged -= HandleSelectionChanged;

    private void Update()
    {
        if (_trackedUnit != null)
        {
            if (_trackedUnit.Object != null &&
                _trackedUnit.Object.IsValid)
            {
                UpdateUnitInfoUI();
            }
            else
            {
                ClearUI();
            }

            return;
        }

        if (_trackedEnemy != null)
        {
            if (_trackedEnemy.Object != null &&
                _trackedEnemy.Object.IsValid)
            {
                UpdateEnemyInfoUI();
            }
            else
            {
                ClearUI();
            }

            return;
        }

        if (_currentBuilding != null)
        {
            if (_currentBuilding.Object != null &&
                _currentBuilding.Object.IsValid)
            {
                UpdateBuildingInfoUI();
            }
            else
            {
                ClearUI();
            }
        }
    }

    private void HandleSelectionChanged()
    {
        _trackedUnit = null;
        _trackedEnemy = null;
        _currentBuilding = null;
        if (playerInteractionState == null) return;

        if (playerInteractionState?.InfoTarget is not Component target)
        {
            ClearUI();
            return;
        }

        _trackedUnit = target.GetComponentInParent<PlayerStats>(true);

        if (_trackedUnit != null)
        {
            SetUpgradeButtonsActive(false);
            UpdateUnitInfoUI();
            return;
        }

        _trackedEnemy = target.GetComponentInParent<EnemyAI>(true);

        if (_trackedEnemy != null)
        {
            SetUpgradeButtonsActive(false);
            UpdateEnemyInfoUI();
            return;
        }

        _currentBuilding = target.GetComponentInParent<PlayerBuilding>(true);

        if (_currentBuilding != null)
        {
            SetUpgradeButtonsActive(_currentBuilding is EngineeringBay);

            UpdateBuildingInfoUI();
            return;
        }

        ClearUI();
    }

    private void ClearUI()
    {
        _trackedUnit = null;
        _trackedEnemy = null;
        _currentBuilding = null;

        if (unitInfoText != null)
        {
            unitInfoText.text = string.Empty;
        }

        SetUpgradeButtonsActive(false);
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

    private void UpdateEnemyInfoUI()
    {
        if (_trackedEnemy == null || _trackedEnemy.Data == null || unitInfoText == null)
            return;

        var data = _trackedEnemy.Data;

        unitInfoText.text = $"<line-height=85%><size=150%><b>{data.enemyName}</b></size>\n\n" +
                            $"HP: {(int)_trackedEnemy.CurrentHp} / {(int)data.hp}</line-height>";
    }

    private void UpdateBuildingInfoUI()
    {
        if (_currentBuilding == null || _currentBuilding.Data == null || unitInfoText == null) return;

        var data = _currentBuilding.Data;

        unitInfoText.text = $"<line-height=85%><size=150%><b>{data.buildingName}</b></size>\n\n" +
                            $"HP: {_currentBuilding.CurrentHp} / {data.hp}<pos=45%>Defense: {data.defense}</line-height>";
    }

    private void SetUpgradeButtonsActive(bool isActive)
    {
        if (attackUpButton != null)
        {
            attackUpButton.gameObject.SetActive(isActive);
        }

        if (defenseUpButton != null)
        {
            defenseUpButton.gameObject.SetActive(isActive);
        }
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
