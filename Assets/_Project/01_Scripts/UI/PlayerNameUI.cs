using Fusion;
using UnityEngine;
using TMPro;

public class PlayerNameUI : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnNameChanged))]
    public NetworkString<_32> PlayerName { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            string localName = FusionConnection.instance._playerName;
            RPC_SetPlayerName(localName);
        }
        OnNameChanged();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string name)
    {
        PlayerName = name;
    }
    private void OnNameChanged()
    {
        if (Object.HasInputAuthority && PlayerName.Length > 0)
        {
            GameObject mainCanvas = GameObject.Find("Main Canvars");

            if (mainCanvas == null)
            {
                mainCanvas = GameObject.Find("Main Canvars");
            }

            if (mainCanvas != null)
            {
                Transform targetTransform = mainCanvas.transform.Find("In Game UI/PlayerTeam/UI Player Name");

                if (targetTransform != null)
                {
                    if (targetTransform.TryGetComponent<TextMeshProUGUI>(out var mainUIText))
                    {
                        mainUIText.text = PlayerName.ToString();
                        Debug.Log($"[PlayerNameUI] 메인 UI 이름 연동 성공: {PlayerName}");
                    }
                }
                else
                {
                    Debug.Log("경로를 찾을 수 없음");
                }
            }
        }
    }
}