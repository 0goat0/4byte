using Fusion;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject roomPanel;

    [Header("Room UI")]
    [SerializeField] private TMP_Text roomNumberText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text roomStatusText;
    [SerializeField] private TMP_Text playerListText;

    private void Start()
    {
        LobbyConnection.Instance.OnRoomEntered += ShowRoom;
        LobbyConnection.Instance.OnRoomExited += CloseRoom;
        LobbyConnection.Instance.OnRoomPlayersChanged += RefreshPlayerList;

        startButton.onClick.AddListener(OnClickStartGame);
        exitButton.onClick.AddListener(OnClickExitRoom);
    }

    private void OnDestroy()
    {
        if (LobbyConnection.Instance != null)
        {
            LobbyConnection.Instance.OnRoomEntered -= ShowRoom;
            LobbyConnection.Instance.OnRoomExited -= CloseRoom;
            LobbyConnection.Instance.OnRoomPlayersChanged -= RefreshPlayerList;
        }

        startButton.onClick.RemoveListener(OnClickStartGame);
        exitButton.onClick.RemoveListener(OnClickExitRoom);
    }

    private void ShowRoom(string session)
    {
        if (roomPanel) roomPanel.SetActive(true);
        if (roomNumberText) roomNumberText.text = session;

        startButton.interactable = LobbyConnection.Instance.CurrentRunner.IsServer;

        RefreshPlayerList();
    }

    private void CloseRoom()
    {
        if (roomPanel) roomPanel.SetActive(false);
    }

    private void RefreshPlayerList()
    {
        NetworkRunner currentRunner = LobbyConnection.Instance.CurrentRunner;

        if (currentRunner == null || !currentRunner.IsRunning)
        {
            playerListText.text = "";
            return;
        }

        var sb = new StringBuilder();
        int count = 0;
        foreach (var p in currentRunner.ActivePlayers)
        {
            count++;
            sb.AppendLine($"Player {p.PlayerId} {(p == currentRunner.LocalPlayer ? "(You)" : "")}");
        }
        playerListText.text = sb.ToString();
        roomStatusText.text = $"Players: {count}";
    }

    private void OnClickStartGame()
    {
        LobbyConnection.Instance.StartGameScene();
    }

    private void OnClickExitRoom()
    {
        LobbyConnection.Instance.ExitRoom();
    }
}
