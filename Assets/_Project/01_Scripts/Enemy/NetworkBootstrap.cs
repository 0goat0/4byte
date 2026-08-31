using Fusion;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private NetworkObject enemyPrefab; // 예: 풀링할 프리팹

    private NetworkRunner runner;
    private PooledNetworkObjectProvider pooledProvider;

    async void Start()
    {
        runner = Instantiate(runnerPrefab);

        // Provider 컴포넌트를 runner 오브젝트에 추가
        pooledProvider = runner.gameObject.AddComponent<PooledNetworkObjectProvider>();
        pooledProvider.SetMaxPoolCount(30);

        //핵심: runner가 이 Provider를 쓰도록 지정
        runner.ProvideInput = true;
        var startArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared, // 또는 Host, Client 등
            SessionName = "TestRoom",
            ObjectProvider = pooledProvider // 이 부분이 핵심!
        };

        await runner.StartGame(startArgs);

        // 게임이 시작된 뒤에 미리 생성해두고 싶다면 Prewarm 호출
        pooledProvider.Prewarm(runner, enemyPrefab, 10);
    }
}