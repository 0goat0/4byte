using Fusion;
using UnityEngine;
using TMPro;
public struct NetworkInputData : INetworkInput
{
    public Vector3 movementInput; // WASD 이동 입력 값
}

public class PlayerStats : NetworkBehaviour
{
    // [Networked] 변수 값이 바뀔 때마다 모든 클라이언트의 OnPlayerNameChanged를 실행합니다.
    [Networked, OnChangedRender(nameof(OnPlayerNameChanged))]
    public NetworkString<_32> playerName { get; set; }

    [Header("UI Reference")]
    [SerializeField] private TextMeshPro playerNameLabel; // name

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            // 적용
            if (FusionConnection.instance != null && !string.IsNullOrEmpty(FusionConnection.instance._playerName))
            {
                string localName = FusionConnection.instance._playerName;
                UpdateNameUI(localName);

                // 호스트(서버)에게 RPC를 보내 네트워크 동기화 변수(playerName) 세팅 요청
                RPC_SetPlayerName(localName);
            }
        }
        else
        {
            // 갱신
            UpdateNameUI(playerName.ToString());
        }
    }

    // FixedUpdateNetwork에서 서버-클라이언트 간 입력을 동기화 이동
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // 입력 방향을 기준으로 캐릭터 물리 이동 처리 (Runner.DeltaTime 필수 사용)
            Vector3 moveDirection = data.movementInput.normalized;
            transform.Translate(moveDirection * moveSpeed * Runner.DeltaTime, Space.World);
        }
    }

    // 클라이언트가 서버(호스트) 권한으로 네트워크 변수 값을 안전하게 변경하도록 요청하는 RPC
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string nameInput, RpcInfo info = default)
    {
        playerName = nameInput;
    }

    // [OnChangedRender] 콜백 함수 (playerName 값이 변경되면 자동 실행)
    private void OnPlayerNameChanged()
    {
        UpdateNameUI(playerName.ToString());
    }

    // 텍스트 컴포넌트에 이름을 안전하게 반영하는 함수
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



