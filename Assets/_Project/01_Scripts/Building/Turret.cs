using Fusion;
using UnityEngine;

public class Turret : PlayerBuilding
{
    [Header("Fusion Turret References")]
    [SerializeField] private Transform turretHead;
    [SerializeField] private Transform firePointL;
    [SerializeField] private Transform firePointR;
    [SerializeField] private ParticleSystem shootEffectL;
    [SerializeField] private ParticleSystem shootEffectR;

    [Header("Turret Settings")]
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private LayerMask enemyLayer;

    // 네트워크 동기화 변수들
    [Networked] private NetworkObject TargetObject { get; set; }
    [Networked] private TickTimer ShootTimer { get; set; }
    [Networked] private int ShootCount { get; set; }
    [Networked] private bool IsLeftTurn { get; set; }

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        base.Spawned();

        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
        {
            TargetObject = null;
            ShootCount = 0;
            IsLeftTurn = true;

            ShootTimer = TickTimer.None;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (TargetObject != null && (TargetObject.gameObject == null || Vector3.Distance(transform.position, TargetObject.transform.position) > attackRange))
        {
            if (HasStateAuthority)
            {
                TargetObject = null;
                ShootTimer = TickTimer.None; //타겟 타이머 초기화
            }
        }

        if (HasStateAuthority)
        {
            if (TargetObject == null)
            {
                FindNearestTarget();

                if (TargetObject != null)
                {
                    float fireInterval = buildingData.attackSpeed > 0 ? 1f / buildingData.attackSpeed : 1f;
                    ShootTimer = TickTimer.CreateFromSeconds(Runner, fireInterval);
                }
            }

            if (TargetObject != null && ShootTimer.Expired(Runner))
            {
                if (TargetObject.TryGetComponent<PlayerBuilding>(out var pb) && pb.IsDestroyed)
                {
                    TargetObject = null;
                    ShootTimer = TickTimer.None;
                    return;
                }

                ServerDirectAttack();

                // 다음 공격 타이머 설정
                float fireInterval = buildingData.attackSpeed > 0 ? 1f / buildingData.attackSpeed : 1f;
                ShootTimer = TickTimer.CreateFromSeconds(Runner, fireInterval);
            }
        }
    }

    // 프레임마다 호출
    public override void Render()
    {
        // 회전
        if (TargetObject != null && turretHead != null)
        {
            Vector3 targetPositionSameHeight = new Vector3(TargetObject.transform.position.x, turretHead.position.y, TargetObject.transform.position.z);
            Vector3 dir = targetPositionSameHeight - turretHead.position;

            if (dir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                turretHead.rotation = Quaternion.Slerp(turretHead.rotation, lookRotation, Runner.DeltaTime * rotationSpeed);
            }
        }
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(ShootCount))
            {
                PlayShootVisual();
            }
        }
    }

    // 적 탐색
    private void FindNearestTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        float closestDistance = Mathf.Infinity;
        NetworkObject nearestEnemy = null;

        foreach (var col in hitColliders)
        {
            if (col.TryGetComponent<NetworkObject>(out var no))
            {
                if (col.TryGetComponent<PlayerBuilding>(out var pb) && pb.IsDestroyed) continue;

                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestEnemy = no;
                }
            }
        }

        if (nearestEnemy != null)
        {
            TargetObject = nearestEnemy;
        }
    }

    private void ServerDirectAttack()
    {
        if (TargetObject == null) return;

        if (TargetObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(buildingData.attackDamage, Object);
        }

        ShootCount++;
    }

    private void PlayShootVisual()
    {
        Debug.Log($"터렛 공격");
        ParticleSystem currentEffect = IsLeftTurn ? shootEffectL : shootEffectR;

        // 이펙트 재생
        if (currentEffect != null)
        {
            currentEffect.Play();
        }
        IsLeftTurn = !IsLeftTurn;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}