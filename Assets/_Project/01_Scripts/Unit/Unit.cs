using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;

public class Unit : NetworkBehaviour, ISelectable
{
    [Networked, OnChangedRender(nameof(OnUnitDataChanged))]
    public UnitData UnitData { get; set; }

    [SerializeField] private TextMeshProUGUI playerNameLabel;

    private PlayerStats _playerStats;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    public override void Spawned()
    {
        // 입력 권한을 가진 클라이언트의 이름을 유닛 이름으로 설정
        if (Object.HasInputAuthority)
        {
            if (FusionConnection.instance != null && !string.IsNullOrEmpty(FusionConnection.instance._playerName))
            {
                string localName = FusionConnection.instance._playerName;

                RPC_SetPlayerName(localName);
            }
        }
        else
        {
            UpdateNameUI(string.IsNullOrEmpty(UnitData.Name.Value) ? "Connecting..." : UnitData.Name.Value);
        }

    }

    public void MoveTo(Vector3 destination)
    {
        _playerStats.CommandMove(destination);
    }

    public void AttackTarget(NetworkObject target)
    {
        _playerStats.CommandAttackTarget(target);
    }

    public void AttackMove(Vector3 destination)
    {
        _playerStats.CommandAttackMove(destination);
    }

    // -----------------------
    // 이름 데이터 및 UI 표시
    // -----------------------

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string nameInput)
    {
        UnitData data = UnitData;
        data.Name = nameInput;
        UnitData = data;
    }

    private void OnUnitDataChanged()
    {
        UpdateNameUI(UnitData.Name.ToString());
    }

    private void UpdateNameUI(string nameToDisplay)
    {
        if (string.IsNullOrEmpty(nameToDisplay) || nameToDisplay == "null")
        {
            nameToDisplay = "Connecting...";
        }

        if (playerNameLabel != null)
        {
            playerNameLabel.text = nameToDisplay;
        }
    }

}
