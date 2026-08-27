using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;

public class GameBootstrap : NetworkBehaviour, INetworkRunnerCallbacks
{
    [Header("Settings")]
    [SerializeField] private NetworkObject playerPrefab;

    // 플레이어 리스트 관리 (참조용)
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public override void Spawned()
    {
        // 씬 오브젝트로 배치된 경우, Runner에 콜백 등록
        Runner.AddCallbacks(this);
    }

    // [중요] 씬 로딩이 완료된 후 호출됨
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return; // Host만 스폰 권한 가짐

        Debug.Log("Scene Load Done - Spawning Players...");

        // 이미 접속해 있는 모든 플레이어에 대해 캐릭터 생성
        foreach (var player in runner.ActivePlayers)
        {
            SpawnPlayer(runner, player);
        }
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        // 1. 랜덤 위치 결정 (예시)
        Vector3 spawnPos = new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));

        // 2. NetworkRunner.Spawn 호출 (가장 핵심!)
        // inputAuthority: 해당 PlayerRef가 이 객체의 입력을 제어함
        NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);

        _spawnedCharacters.Add(player, networkPlayerObject);
    }

    // 게임 도중 새로 들어온 플레이어 처리
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer) SpawnPlayer(runner, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && _spawnedCharacters.TryGetValue(player, out var networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    #region Unused Callbacks
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {/*
        var data = new NetworkInputData();

        data.movementInput.x = Input.GetAxisRaw("Horizontal");
        data.movementInput.y = Input.GetAxisRaw("Vertical");

        input.Set(data); // 서버로 전송*/
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new System.NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new System.NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new System.NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new System.NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new System.NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new System.NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data)
    {
        throw new System.NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new System.NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new System.NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new System.NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new System.NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new System.NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new System.NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new System.NotImplementedException();
    }
    // ... 나머지 인터페이스 메서드들 (공백으로 유지) ...
    #endregion
}