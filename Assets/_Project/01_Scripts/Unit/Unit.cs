using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Unit : NetworkBehaviour, ISelectable
{
    [SerializeField] private UnitData _unitData;
    public UnitData UnitData => _unitData;

    [Networked, OnChangedRender(nameof(OnPlayerNameChanged))]
    public NetworkString<_32> playerName { get; set; }

    [SerializeField] private TextMeshProUGUI playerNameLabel;

    private NetworkNavMeshMover _mover;

    private void Awake()
    {
        _mover = GetComponent<NetworkNavMeshMover>();
    }

    public override void Spawned()
    {
        // 입력 권한을 가진 클라이언트의 이름을 유닛 이름으로 설정
        if (Object.HasInputAuthority)
        {
            if (FusionConnection.instance != null && !string.IsNullOrEmpty(FusionConnection.instance._playerName))
            {
                string localName = FusionConnection.instance._playerName;
                UpdateNameUI(localName);
                RPC_SetPlayerName(localName);

                _unitData = null;
                _unitData = new UnitData(localName);
            }
        }
        else
        {
            UpdateNameUI(string.IsNullOrEmpty(playerName.Value) ? "Connecting..." : playerName.Value);
        }
    }

    public void RequestMove(Vector3 destination)
    {
        RPC_RequestMove(destination);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestMove(Vector3 destination)
    {
        _mover.MoveTo(destination);
    }

    public void RequestAttackTarget(NetworkObject target)
    {
        RPC_RequestAttackTarget(target);
    }

    public void RequestAttackMove(Vector3 destination)
    {
        RPC_RequestAttackMove(destination);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAttackTarget(NetworkObject target)
    {
        // 요청이 전달되는 사이 대상이 디스폰될 수 있습니다.
        if (target == null)
            return;

        Debug.Log($"대상 공격 요청: {target.name}", this);

        // 상태머신 구현 시 목표를 저장하고 Chase 또는 Attack으로 연결합니다.
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAttackMove(Vector3 destination)
    {
        Debug.Log($"공격 이동 요청: {destination}", this);

        // 상태머신 구현 시 목적지를 저장하고 AttackMove로 연결합니다.
    }

    // -----------------------
    // 이름 데이터 및 UI 표시
    // -----------------------

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string nameInput, RpcInfo info = default)
    {
        playerName = nameInput;
    }

    private void OnPlayerNameChanged()
    {
        UpdateNameUI(playerName.Value);
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
