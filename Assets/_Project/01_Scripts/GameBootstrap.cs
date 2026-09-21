using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Player Spawn")]
    [SerializeField] private NetworkObject _partyPrefab;
    [SerializeField] private NetworkObject _initialUnitPrefab;
    [SerializeField] private Vector3 _spawnOrigin = new Vector3(8f, 0f, -8f);
    [SerializeField] private float _spawnSpacing = 2f;

    // 플레이어 리스트 관리 (참조용)
    private Dictionary<PlayerRef, List<NetworkParty>> _spawnedParties = new Dictionary<PlayerRef, List<NetworkParty>>();

    private NetworkRunner _runner;

    [SerializeField] private string _mainMenuSceneName = "MainMenuScene";

    private bool _isReturningToMenu;

    private void Awake()
    {
        _runner = NetworkRunner.GetRunnerForScene(gameObject.scene);

        if (_runner == null)
        {
            Debug.LogError("실행 중인 NetworkRunner를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }

        _runner.AddCallbacks(this);
    }

    private void OnDestroy()
    {
        if (_runner != null)
        {
            _runner.RemoveCallbacks(this);
        }
    }

    // 씬 로딩이 완료된 후 호출됨
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return; // Host만 스폰 권한 가짐

        Debug.Log("Scene Load Done - Spawning Players...");

        // 이미 접속해 있는 모든 플레이어에 대해 캐릭터 생성
        foreach (var player in runner.ActivePlayers)
        {
            SpawnPlayerParty(runner, player);
        }
    }

    private void SpawnPlayerParty(NetworkRunner runner, PlayerRef player)
    {
        if (!_spawnedParties.TryGetValue(player, out List<NetworkParty> parties))
        {
            parties = new List<NetworkParty>();
            _spawnedParties.Add(player, parties);
        }

        if (_partyPrefab == null || _initialUnitPrefab == null)
        {
            Debug.LogError("Party spawn prefabs are not assigned.", this);
            return;
        }

        Vector3 spawnPosition =
            _spawnOrigin + Vector3.right * (_spawnSpacing * player.PlayerId);

        NetworkObject partyObject = runner.Spawn(
            _partyPrefab,
            spawnPosition,
            Quaternion.identity,
            player);

        NetworkObject unitObject = runner.Spawn(
            _initialUnitPrefab,
            spawnPosition,
            Quaternion.identity);

        NetworkParty party = partyObject.GetComponent<NetworkParty>();
        PartyMember partyMember = unitObject.GetComponent<PartyMember>();

        if (party == null || partyMember == null ||
            !party.TryAddMember(partyMember))
        {
            runner.Despawn(unitObject);
            runner.Despawn(partyObject);

            Debug.LogError(
                $"Failed to create the initial party for Player {player.PlayerId}.");
            return;
        }

        // runner.SetPlayerObject(player, partyObject);
        _spawnedParties[player].Add(party);

        Debug.Log($"Party Spawned for Player: {player.PlayerId}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        if (runner.IsServer && _spawnedParties.TryGetValue(player, out var networkParties))
        {
            foreach (NetworkParty party in networkParties)
            {
                DespawnParty(runner, party);
            }

            _spawnedParties.Remove(player);
        }

    }

    private void DespawnParty(NetworkRunner runner, NetworkParty party)
    {
        if (party == null)
            return;

        for (int i = party.MemberCount - 1; i >= 0; i--)
        {
            PartyMember member = party.GetMember(i);

            if (member == null)
                continue;

            party.TryRemoveMember(member);

            if (member.Object != null)
            {
                runner.Despawn(member.Object);
            }
        }

        runner.Despawn(party.Object);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) 
    {
        request.Refuse();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
    {
        Debug.Log($"호스트와 연결이 끊어졌습니다 : {reason}");

        LeaveGameAsync(runner);
    }

    private async void LeaveGameAsync(NetworkRunner runner)
    {
        if (_isReturningToMenu)
            return;

        _isReturningToMenu = true;

        if (!runner.IsShutdown)
        {
            await runner.Shutdown();
        }

        SceneManager.LoadScene(_mainMenuSceneName);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    {
        if (_isReturningToMenu)
            return;

        _isReturningToMenu = true;

        SceneManager.LoadScene(_mainMenuSceneName);
    }


    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
