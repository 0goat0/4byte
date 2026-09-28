using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;

public class PlayerStatsUI : NetworkBehaviour
{
    [Header("Connections")]
    [SerializeField] private PlayerInteractionState playerInteractionState;
    [SerializeField] private TextMeshProUGUI unitInfoText;
    [SerializeField] private TextMeshProUGUI totalKillText;
    [SerializeField] private Button attackUpButton;
    [SerializeField] private Button defenseUpButton;

    private PlayerStats _trackedUnit;
    private EnemyAI _trackedEnemy;

    private PlayerBuilding _currentBuilding;

    private void Awake()
    {
        SetUpgradeButtonsActive(false);
    }

    private void OnEnable() => playerInteractionState.OnSelectionChanged += HandleSelectionChanged;
    private void OnDisable() => playerInteractionState.OnSelectionChanged -= HandleSelectionChanged;
    private void Start()
    {
        Invoke(nameof(UpdateTotalKillUI), 0.5f);
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (_trackedUnit != null)
        {
            if (_trackedUnit.Object != null && _trackedUnit.Object.IsValid)
            {
                UpdateUnitInfoUI();
            }
            else
            {
                ClearUI();
            }
            return;
        }

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

        // 1. 선택된 대상이 아군 유닛인지 확인
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
    public void UpdateTotalKillUI()
    {
        if (totalKillText == null) return;

        var runner = FindAnyObjectByType<NetworkRunner>();
        if (runner == null || !runner.IsRunning)
        {
            return;
        }

        // Key: Player ID (int), Value: 총 킬수 (int)
        Dictionary<int, int> playerKillsMap = new Dictionary<int, int>();
        int localPlayerId = runner.LocalPlayer.PlayerId;

        // PlayerStats 유닛 조사
        var allUnits = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

        foreach (var unit in allUnits)
        {
            if (unit != null && unit.Object != null)
            {
                int playerId;

                if (unit.Object.InputAuthority == PlayerRef.None || unit.Object.InputAuthority.PlayerId == -1)
                {
                    playerId = localPlayerId;
                }
                else
                {
                    playerId = unit.Object.InputAuthority.PlayerId;
                }

                if (!playerKillsMap.ContainsKey(playerId))
                {
                    playerKillsMap[playerId] = 0;
                }
                playerKillsMap[playerId] += unit.Kills;
            }
        }

        if (playerKillsMap.Count == 0)
        {
            playerKillsMap[localPlayerId] = 0;
        }

        StringBuilder sb = new StringBuilder();
        int index = 0;

        foreach (var kvp in playerKillsMap)
        {
            if (kvp.Key == localPlayerId)
            {
                sb.Append($"<b>player {kvp.Key} : {kvp.Value} (You)");
            }
            else
            {
                sb.Append($"player {kvp.Key} : {kvp.Value}(Kill)");
            }

            if (index < playerKillsMap.Count + 1)
            {
                sb.Append("\n");
            }
            index++;
        }

        totalKillText.text = sb.ToString();
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
    public void UpgradeAllUnits(bool isAttack)
    {
        if (_currentBuilding == null) return;
        if (_currentBuilding is not EngineeringBay engineeringBay) return;

        PlayerRef myPlayerRef = engineeringBay.Runner.LocalPlayer;
        var allUnits = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

        PlayerStats realKillerUnit = null;

        foreach (var unit in allUnits)
        {
            if (unit != null && unit.Object != null)
            {
                if (unit.Object.InputAuthority == myPlayerRef || unit.Object.InputAuthority == PlayerRef.None || unit.Object.InputAuthority.PlayerId == -1)
                {
                    if (unit.Kills > 0)
                    {
                        realKillerUnit = unit;
                        break;
                    }
                }
            }
        }

        if (realKillerUnit != null)
        {
            engineeringBay.TryUpgradeUnit(myPlayerRef, realKillerUnit, isAttack);
        }
    }
}
