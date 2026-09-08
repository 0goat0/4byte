using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FusionConnection : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionConnection instance;
    public bool connectOnAwake = false;
    public NetworkRunner runner;
    [SerializeField] NetworkObject playerPrefab;
    public string _playerName = null;

    private bool isConnecting = false;
    private bool isInLobby = false;

    [Header("Session List")]
    public GameObject roomListCanvas;
    private List<SessionInfo> _sessions = new List<SessionInfo>();
    public Button refreshButton;
    public Transform sessionListContent;
    public GameObject sessionEntryPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (connectOnAwake == true)
        {
            CreateSession();
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshSessionListUI);
        }
    }

    // 비동기 대기 완료
    public async void ConnectedToLobby(string playerName)
    {
        if (isConnecting) return;
        isConnecting = true;

        roomListCanvas.SetActive(true);
        _playerName = playerName;

        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        runner.RemoveCallbacks(this);
        runner.AddCallbacks(this);

        Debug.Log("Loading in Lobby");

        var result = await runner.JoinSessionLobby(SessionLobby.ClientServer);

        if (result.Ok)
        {
            Debug.Log("Connrecting Lobby");
            isInLobby = true;
            isConnecting = false;
            roomListCanvas.SetActive(true); // 로비 진입이 완벽히 끝나면 UI
        }
        else
        {
            Debug.Log("방만들기 실패");
            isConnecting = false;
        }

    }
    // 방 개설 호스트
    public async void CreateSession()
    {
        if (!isInLobby)
        {
            Debug.LogWarning("아직 로비가 아닙니다");
            return;
        }

        if (isConnecting) return;
        isConnecting = true;
        roomListCanvas.SetActive(false);

        int randomInt = UnityEngine.Random.Range(1000, 9999);
        string randomSessionName = "Room-" + randomInt.ToString();

        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }
        runner.RemoveCallbacks(this);
        runner.AddCallbacks(this);

        var sceneManager = runner.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = randomSessionName,
            Scene = SceneRef.FromIndex(5),
            PlayerCount = 3,
            SceneManager = sceneManager,
        });

        if (!result.Ok)
        {
            Debug.LogError($"Creation Room false(Host): {result.ShutdownReason}");
            roomListCanvas.SetActive(true);
            isConnecting = false;
        }
        else
        {
            isInLobby = false;
        }
    }

    // 리스트에서 방 선택 Client 접속
    public async void ConnectToClientSession(string sessionName)
    {
        // 로비 실행을 취소 (NameServer error)
        if (!isInLobby)
        {
            Debug.LogWarning("Wait try again");
            return;
        }

        if (isConnecting) return;
        isConnecting = true;
        roomListCanvas.SetActive(false);

        var sceneManager = runner.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();


        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = sceneManager,
        });

        if (!result.Ok)
        {
            Debug.Log("방들어가기 실패");
            roomListCanvas.SetActive(true);
            isConnecting = false;
        }
        else
        {
            // 로비 상태 해제
            isInLobby = false;
        }
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OnConnectedToServer");
        isConnecting = false;
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        _sessions = sessionList;
        Debug.Log($"Session List Updated. Count: {sessionList.Count}");
        RefreshSessionListUI();
    }

    public void RefreshSessionListUI()
    {
        // 프리팹 오브젝트 삭제
        foreach (Transform child in sessionListContent)
        {
            Destroy(child.gameObject);
        }

        // 리스트로 UI 생성
        foreach (SessionInfo session in _sessions)
        {
            if (session.IsVisible == false) continue;

            GameObject entry = GameObject.Instantiate(sessionEntryPrefab, sessionListContent);
            SessionEntryPrefab script = entry.GetComponent<SessionEntryPrefab>();

            script.sessionName.text = session.Name;
            script.playerCount.text = $"{session.PlayerCount}/{session.MaxPlayers}";

            if (session.IsOpen == false || session.PlayerCount >= session.MaxPlayers)
            {
                script.joinButton.interactable = false;
            }
            else
            {
                script.joinButton.interactable = true;

                // 버튼 클릭 시 해당 방에 접속하도록 이벤트 바인딩
                string targetSessionName = session.Name;
                script.joinButton.onClick.RemoveAllListeners();
                script.joinButton.onClick.AddListener(() => ConnectToClientSession(targetSessionName));
            }
        }
    }


    // 호스트 Spawn 권한
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // 호스트 오브젝트 생성
        if (runner.IsServer)
        {
            Debug.Log($"Character Spawn for Player: {player.PlayerId}");

            // 호스트가 일괄 제어
            NetworkObject playerObject = runner.Spawn(playerPrefab, Vector3.one * 2f, Quaternion.identity, player);

            // 플레이어 오브젝트 소유권 설정
            runner.SetPlayerObject(player, playerObject);
        }
        Debug.Log("OnPlayerJoined 완료");
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // 네트워크 객체를 탐색
            if (runner.TryGetPlayerObject(player, out NetworkObject playerObject))
            {
                if (playerObject != null)
                {
                    runner.Despawn(playerObject);
                }
            }
            runner.SetPlayerObject(player, null);
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData inputData = new NetworkInputData();

        // test 입력 감지
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            // 수직 입력
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
        }

        inputData.movementInput = new Vector3(horizontal, 0, vertical);

        // Fusion으로
        input.Set(inputData);
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        isConnecting = false;
        isInLobby = false;
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }


    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

}