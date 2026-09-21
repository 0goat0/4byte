using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyConnection : MonoBehaviour, INetworkRunnerCallbacks
{
    public static LobbyConnection Instance;

    [Header("Fusion Prefabs")]
    [SerializeField] private NetworkRunner _runnerPrefab;

    private NetworkRunner _currentRunner;

    [Header("Session List")]
    [SerializeField] private GameObject _roomListPanel;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private Transform _sessionListContent;
    [SerializeField] private GameObject _sessionEntryPrefab;
    private List<SessionInfo> _sessions = new List<SessionInfo>();

    [Header("Scene")]
    [SerializeField] private int gameSceneBuildIndex;
    [SerializeField] private string _mainMenuSceneName = "MainMenuScene";

    public string _playerName = null;

    private bool isConnecting = false;
    private bool isInLobby = false;

    private PooledNetworkObjectProvider _pooledProvider;

    public NetworkRunner CurrentRunner => _currentRunner;

    public event Action<string> OnRoomEntered;
    public event Action OnRoomExited;
    public event Action OnRoomPlayersChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _refreshButton.onClick.AddListener(RefreshSessionListUI);
    }

    private void OnDestroy()
    {
        if (_currentRunner != null)
            _currentRunner.RemoveCallbacks(this);

        if (_refreshButton != null)
        {
            _refreshButton.onClick.RemoveAllListeners();
        }

        if (Instance == this)
            Instance = null;
    }

    // ==================================================================================
    // 메인 로비 관리 메서드
    // ==================================================================================

    /// <summary>
    /// 로비 접속 함수
    /// </summary>
    /// <param name="playerName"></param>
    public async void ConnectedToLobby(string playerName)
    {
        if (isConnecting) return;
        isConnecting = true;

        _roomListPanel.SetActive(true);
        _playerName = playerName;

        CreateRunner();

        Debug.Log("Loading in Lobby");

        var result = await _currentRunner.JoinSessionLobby(SessionLobby.ClientServer);

        if (result.Ok)
        {
            Debug.Log("Connrecting Lobby");
            isInLobby = true;
            isConnecting = false;
            _roomListPanel.SetActive(true); // 로비 진입이 완벽히 끝나면 UI
        }
        else
        {
            Debug.Log("방만들기 실패");
            isConnecting = true;

            await ReleaseRunnerAsync();

            isConnecting = false;
        }
    }

    public void RefreshSessionListUI()
    {
        // 프리팹 오브젝트 삭제
        foreach (Transform child in _sessionListContent)
        {
            Destroy(child.gameObject);
        }

        // 리스트로 UI 생성
        foreach (SessionInfo session in _sessions)
        {
            if (session.IsVisible == false) continue;

            GameObject entry = GameObject.Instantiate(_sessionEntryPrefab, _sessionListContent);
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


    /// <summary>
    /// 호스트 방 생성 함수
    /// </summary>
    public async void CreateSession()
    {
        if (!isInLobby)
        {
            Debug.LogWarning("아직 로비가 아닙니다");
            return;
        }

        if (isConnecting) return;
        isConnecting = true;
        _roomListPanel.SetActive(false);

        int randomInt = UnityEngine.Random.Range(1000, 9999);
        string randomSessionName = "Room-" + randomInt.ToString();

        CreateRunner();

        var sceneManager = _currentRunner.GetComponent<NetworkSceneManagerDefault>();

        var result = await _currentRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = randomSessionName,
            PlayerCount = 3,
            SceneManager = sceneManager,
            ObjectProvider = _pooledProvider
        });

        if (!result.Ok)
        {
            Debug.LogError($"Creation Room false(Host): {result.ShutdownReason}");

            await ReleaseRunnerAsync();

            isConnecting = false;
            ConnectedToLobby(_playerName);
            return;
        }
        else
        {
            OnRoomEntered?.Invoke(randomSessionName);

            isInLobby = false;
            isConnecting = false;
        }
    }

    /// <summary>
    /// 클라이언트 방 접속 함수
    /// </summary>
    /// <param name="sessionName"> 접속 할 방 세션 이름 </param>
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
        _roomListPanel.SetActive(false);

        var sceneManager = _currentRunner.GetComponent<NetworkSceneManagerDefault>();
        // if (sceneManager == null) sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();


        var result = await _currentRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = sceneManager,
            ObjectProvider = _pooledProvider
        });

        if (!result.Ok)
        {
            Debug.Log("방들어가기 실패");

            await ReleaseRunnerAsync();

            isConnecting = false;
            ConnectedToLobby(_playerName);
            return;
        }
        else
        {
            OnRoomEntered?.Invoke(sessionName);

            isInLobby = false;
            isConnecting = false;
        }
    }

    // ==================================================================================
    // 방 내부 로직 (게임 씬 진입 / 나가기)
    // ==================================================================================

    // [Host Only] 게임 씬으로 이동
    public void StartGameScene()
    {
        if (_currentRunner == null || !_currentRunner.IsServer)
            return;

        if (_currentRunner.SessionInfo.IsValid)
        {
            _currentRunner.SessionInfo.IsOpen = false;
            _currentRunner.SessionInfo.IsVisible = false;
        }

        _currentRunner.LoadScene(SceneRef.FromIndex(gameSceneBuildIndex));
    }

    // 방 나가기 (Shutdown)
    public async void ExitRoom()
    {
        if (isConnecting || _currentRunner == null)
            return;

        isConnecting = true;

        await ReleaseRunnerAsync();

        OnRoomExited?.Invoke();

        isConnecting = false;
        ConnectedToLobby(_playerName);
    }

    // ==================================================================================
    // 러너 생성 및 삭제 메서드
    // ==================================================================================

    private void CreateRunner()
    {
        if (_currentRunner != null)
            return;

        _currentRunner = Instantiate(_runnerPrefab);

        DontDestroyOnLoad(_currentRunner.gameObject);

        _pooledProvider = _currentRunner.gameObject.AddComponent<PooledNetworkObjectProvider>();
        _pooledProvider.SetMaxPoolCount(30);

        _currentRunner.ProvideInput = true;
        _currentRunner.AddCallbacks(this);
    }

    private async Task ReleaseRunnerAsync()
    {
        NetworkRunner previousRunner = _currentRunner;
        _currentRunner = null;
        _pooledProvider = null;
        isInLobby = false;

        if (previousRunner == null)
            return;

        previousRunner.RemoveCallbacks(this);

        try
        {
            await previousRunner.Shutdown();
        }
        finally
        {
            if (previousRunner != null)
            {
                Destroy(previousRunner.gameObject);
            }
        }
    }

    // ==================================================================================
    // Fusion Callbacks (INetworkRunnerCallbacks)
    // ==================================================================================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        OnRoomPlayersChanged?.Invoke();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        OnRoomPlayersChanged?.Invoke();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (this == null || runner != _currentRunner)
            return;

        Debug.Log($"OnShutdown: {shutdownReason}");

        _currentRunner = null;
        _pooledProvider = null;
        isInLobby = false;
        isConnecting = false;

        if (runner != null)
            Destroy(runner.gameObject);

        SceneManager.LoadScene(_mainMenuSceneName);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        _sessions = sessionList;
        Debug.Log($"Session List Updated. Count: {sessionList.Count}");
        RefreshSessionListUI();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        if (this == null || runner != _currentRunner)
            return;

        Debug.Log($"호스트 연결 종료 : {reason}");
        ExitRoom();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken){ }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
