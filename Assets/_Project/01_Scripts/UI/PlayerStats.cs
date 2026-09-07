using Fusion;
using UnityEngine;
using TMPro;

public struct NetworkInputData : INetworkInput
{
    public Vector3 movementInput;
}

public class PlayerStats : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnPlayerNameChanged))]
    public NetworkString<_32> playerName { get; set; }

    [SerializeField] private TextMeshPro playerNameLabel;
    [SerializeField] private float moveSpeed = 5f;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            if (FusionConnection.instance != null && !string.IsNullOrEmpty(FusionConnection.instance._playerName))
            {
                string localName = FusionConnection.instance._playerName;
                UpdateNameUI(localName);
                RPC_SetPlayerName(localName);
            }
        }
        else
        {
            UpdateNameUI(string.IsNullOrEmpty(playerName.Value) ? "Connecting..." : playerName.Value);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // GetInput을 통해 틱 단위로 입력을 받아 이동 처리 (NetworkTransform이 동기화 및 보간 담당)
        if (GetInput(out NetworkInputData data))
        {
            Vector3 moveDirection = data.movementInput.normalized;
            transform.position += moveDirection * moveSpeed * Runner.DeltaTime;
        }
    }

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