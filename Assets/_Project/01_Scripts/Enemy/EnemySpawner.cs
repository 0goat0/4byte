using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour, IDamageable, IStageSpawner, IHealthSource
{
    [SerializeField] private NetworkObject enemyPrefab;

    [Header("Spawner Health")]
    [SerializeField] private float maxHp = 100f;
    [Networked, OnChangedRender(nameof(OnCurrentHpChanged))] private float CurrentHp { get; set; }
    [Networked] public NetworkBool IsDestroyed { get; set; }
    public float CurrentHealth => CurrentHp;
    public float MaxHealth => maxHp;
    public event System.Action<float, float> OnHealthChanged;

    [Header("On/Off (GameManager가 단계별로 제어)")]
    [SerializeField] private bool startActive = false; // 시작 시 켜진 상태로 시작할지 여부
    [Networked] public NetworkBool IsActive { get; set; }


    [Header("Spawn Area")]
    [SerializeField] private float minSpawnRadius;   // 건물과 너무 붙지 않도록 최소 거리
    [SerializeField] private float maxSpawnRadius;   // 스폰 가능한 최대 거리
    [SerializeField] private float checkRadius;    // 몬스터 크기에 맞춰 조절 (겹침 검사용)
    [SerializeField] private int maxEnemyNum;
    [SerializeField] private int waveEnemyNum;
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
    private WorldHealthBarTarget _healthBarTarget;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            CurrentHp = maxHp;
            IsActive = startActive;
        }

        _healthBarTarget = WorldHealthBarTarget.Attach(gameObject, this);

        // Runner의 GameObject에서 PooledNetworkObjectProvider 컴포넌트를 찾아옴
        var pooledProvider = Runner.GetComponent<PooledNetworkObjectProvider>();

        if (pooledProvider != null)
        {
            // 미리 enemyNum개 채워두기
            pooledProvider.Prewarm(Runner, enemyPrefab, waveEnemyNum);

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

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _healthBarTarget?.Release();
    }

    private void OnCurrentHpChanged()
    {
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }
        if (IsDestroyed)
        {
            return;
        }
        //GameManager에 의해 꺼진 상태면 스폰 로직 자체를 멈춤 (파괴는 아님, 재개 가능)
        if (!IsActive)
        {
            return;
        }
        //비활성화 / 파괴된 경우 멈춤
        if (!enabled || !gameObject.activeInHierarchy)
        {
            return;
        }
        //e = enemy
        spawnedEnemies.RemoveAll(e => e == null || !e.IsValid);
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

            if (RemainingInWave == 0)
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
        if (maxEnemyNum <= spawnedEnemies.Count)
        {
            return;
        }
        RemainingInWave = waveEnemyNum; // 이번 웨이브에 낼 마릿수

    }
    // ─────────────────────────────────────────
    // GameManager가 단계별로 스포너를 켜고 끄기 위한 공개 API
    // ─────────────────────────────────────────

    /// <summary>
    /// State Authority(호스트) 쪽 GameManager가 직접 호출할 때 사용.
    /// 클라이언트에서 호출해야 한다면 아래 RPC_SetActive를 사용할 것.
    /// </summary>
    public void SetSpawnerActive(bool active)
    {
        if (!HasStateAuthority) return;
        if (IsDestroyed) return; // 이미 파괴된 스포너는 다시 켤 수 없음
        if (IsActive == active) return;

        IsActive = active;

        if (active)
        {
            ResumeTimersOnActivate();
        }
    }

    /// <summary>
    /// GameManager가 클라이언트에서도 호출할 수 있도록 하는 RPC.
    /// 실제 처리는 StateAuthority에서만 수행됨.
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetActive(NetworkBool active)
    {
        SetSpawnerActive(active);
    }

    /// <summary>
    /// 꺼져있던 동안 타이머가 이미 만료된 상태로 남아있으면
    /// 다시 켜지자마자 즉시(버스트로) 몬스터가 쏟아지므로,
    /// 재개 시점에 타이머를 다시 세팅해 자연스럽게 이어지도록 함.
    /// </summary>
    private void ResumeTimersOnActivate()
    {
        if (RemainingInBurst > 0)
        {
            BurstTimer = TickTimer.CreateFromSeconds(Runner, burstSpawnInterval);
        }
        else if (RemainingInWave > 0)
        {
            SpawnTimer = TickTimer.CreateFromSeconds(Runner, singleSpawnInterval);
        }
        else
        {
            WaveTimer = TickTimer.CreateFromSeconds(Runner, waveSpawnInterval);
        }
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
    public void TakeDamage(float damage, NetworkObject attacker)
    {
        // 데미지 처리는 StateAuthority에서만
        if (!HasStateAuthority)
        {
            return;
        }
        if (IsDestroyed)
        {
            return;
        }
        if (!IsActive)
        {
            return;
        }

        CurrentHp -= damage;
        if (CurrentHp <= 0f)
        {
            OnSpawnerDestroyed();
        }
    }
    private void OnSpawnerDestroyed()
    {
        IsDestroyed = true;

        // 진행 중이던 스폰 예약 초기화
        RemainingInWave = 0;
        RemainingInBurst = 0;
        SpawnTimer = default;
        WaveTimer = default;
        BurstTimer = default;

        // 오브젝트 자체를 없앰
        Runner.Despawn(Object);
    }
}
