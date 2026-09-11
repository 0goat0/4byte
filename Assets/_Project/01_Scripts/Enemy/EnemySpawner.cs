using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject enemyPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float minSpawnRadius;   // 건물과 너무 붙지 않도록 최소 거리
    [SerializeField] private float maxSpawnRadius;   // 스폰 가능한 최대 거리
    [SerializeField] private float checkRadius;    // 몬스터 크기에 맞춰 조절 (겹침 검사용)
    [SerializeField] private int enemyNum;
    [Networked] private int RemainingInWave { get; set; }

    [Header("Spawn Timing")]
    [SerializeField] private float waveSpawnInterval; //웨이브 간격
    [SerializeField] private float singleSpawnInterval; //하나씩 소환 간격

    [Header("Obstacle / Overlap Check")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Burst Settings")]
    [SerializeField] private int burstEnemyNum;       // 버스트로 낼 마릿수
    [SerializeField] private float burstSpawnInterval; // 버스트 내 개체 간 간격 (짧게)

    private readonly List<NetworkObject> spawnedEnemies = new List<NetworkObject>();
    [Networked] private TickTimer SpawnTimer { get; set; }
    [Networked] private TickTimer WaveTimer { get; set; }
    [Networked] private TickTimer BurstTimer { get; set; }
    [Networked] private int RemainingInBurst { get; set; }
    public override void Spawned()
    {
        // Runner의 GameObject에서 PooledNetworkObjectProvider 컴포넌트를 찾아옴
        var pooledProvider = Runner.GetComponent<PooledNetworkObjectProvider>();

        if (pooledProvider != null)
        {
            // 미리 enemyNum개 채워두기
            pooledProvider.Prewarm(Runner, enemyPrefab, enemyNum);

            // 현재 풀에 몇 개 남았는지 확인
            //int count = pooledProvider.GetPoolCount(enemyPrefab);
            //Debug.Log($"풀에 남은 개수: {count}");

            //권한이 있는지 확인하고 권한 있는 호스트만 스폰
            //if (HasStateAuthority) 
            //{
            //    StartCoroutine(SpawnRoutine());
            //} 
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }
        //비활성화 / 파괴된 경우 멈춤
        if (!enabled || !gameObject.activeInHierarchy)
        {
            return;
        }
        //if(enemyNum < spawnedEnemies.Count)
        //{
        //    return;
        //}
        if (RemainingInBurst > 0)
        {
            if (!BurstTimer.ExpiredOrNotRunning(Runner)) return;

            TrySpawnEnemyAroundBuilding();
            RemainingInBurst--;
            BurstTimer = TickTimer.CreateFromSeconds(Runner, burstSpawnInterval);
            return;
        }

        if (RemainingInWave > 0)
        {
            if (!SpawnTimer.ExpiredOrNotRunning(Runner))
            {
                return;
            }
            TrySpawnEnemyAroundBuilding();
            RemainingInWave--;
            SpawnTimer = TickTimer.CreateFromSeconds(Runner, singleSpawnInterval);

            if(RemainingInWave == 0)
            {
                RemainingInBurst = burstEnemyNum;
                WaveTimer = TickTimer.CreateFromSeconds(Runner, waveSpawnInterval);
            }
            return;
        }
        if (!WaveTimer.ExpiredOrNotRunning(Runner))
        {
            return;
        } 

        RemainingInWave = enemyNum; // 이번 웨이브에 낼 마릿수
       
    }
    private void SpawnEnemy(Vector3 pos)
    {
        // ★ 이렇게만 호출하면 됨 - Provider를 직접 몰라도 됨
        // Runner가 내부적으로 등록된 ObjectProvider(풀링 로직)를 자동으로 사용함
        NetworkObject enemy = Runner.Spawn(enemyPrefab, pos, Quaternion.identity);

        if (enemy != null)
        {
            spawnedEnemies.Add(enemy);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minSpawnRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);
    }

    
    private void TrySpawnEnemyAroundBuilding()
    {
        Vector3 pos = GetSpawnPosition();

        if (IsPositionFree(pos))
        {
            SpawnEnemy(pos);
        }
        else
        {
            Debug.Log("스폰실패");
        }
    }

    private Vector3 GetSpawnPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;

        return transform.position + offset;
    }

    // 해당 위치에 건물/다른 몬스터 등 장애물이 있는지 검사
    private bool IsPositionFree(Vector3 pos)
    {
        Collider[] hits = Physics.OverlapSphere(pos, checkRadius, obstacleMask);
        return hits.Length == 0;
    }

}