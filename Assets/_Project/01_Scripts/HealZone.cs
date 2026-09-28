using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

// 회복 가능한 대상(플레이어, 건물 등)이 구현. Heal() 안에서 소유자에게 회복을 요청한다.
public interface IHealable
{
    void Heal(float amount);
}

// 빈 오브젝트에 BoxCollider + NetworkObject + 이 컴포넌트를 붙여서 사용.
// 호스트(이 오브젝트의 StateAuthority)가 박스 범위 안의 모든 플레이어를 찾아
// healInterval 초마다 TakeDamage(-healAmount)로 회복시킨다.
//
// Shared 모드에서는 PlayerStats.TakeDamage가 "그 플레이어의 StateAuthority"에서만 동작하므로
// 호스트가 소유하지 않은 플레이어는 PlayerStats.RpcHeal(RPC)로 소유자에게 회복을 요청한다.
[RequireComponent(typeof(BoxCollider))]
public class HealZone : NetworkBehaviour
{
    [Header("대상 (플레이어 + 건물 레이어를 모두 포함)")]
    [FormerlySerializedAs("playerLayerMask")]
    [SerializeField] private LayerMask targetLayerMask;

    [Header("회복")]
    [SerializeField] private float healAmount = 1f;    // 1회 회복량
    [SerializeField] private float healInterval = 1f;  // 회복 간격(초)

    private BoxCollider box;
    private readonly Collider[] hitBuffer = new Collider[32];
    private readonly HashSet<IHealable> healedThisTick = new HashSet<IHealable>();

    [Networked] private TickTimer HealTimer { get; set; }

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true; // 플레이어가 통과할 수 있도록
    }

    public override void FixedUpdateNetwork()
    {
        // 호스트만 판정
        if (!HasStateAuthority)
        {
            return;
        }
        if (!HealTimer.ExpiredOrNotRunning(Runner))
        {
            return;
        }

        int count = OverlapBox();

        healedThisTick.Clear();
        for (int i = 0; i < count; i++)
        {
            IHealable target = hitBuffer[i].GetComponentInParent<IHealable>();
            if (target == null)
            {
                continue;
            }
            // 콜라이더가 여러 개여도 대상당 1회만
            if (!healedThisTick.Add(target))
            {
                continue;
            }

            target.Heal(healAmount);
        }

        // 누군가 회복됐을 때만 쿨타임 시작 → 범위에 들어오면 바로 첫 회복
        if (healedThisTick.Count > 0)
        {
            HealTimer = TickTimer.CreateFromSeconds(Runner, healInterval);
        }
    }

    private int OverlapBox()
    {
        Transform t = box.transform;
        Vector3 scale = t.lossyScale;

        Vector3 center = t.TransformPoint(box.center);
        Vector3 halfExtents = Vector3.Scale(
            box.size * 0.5f,
            new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));

        return Physics.OverlapBoxNonAlloc(
            center, halfExtents, hitBuffer, t.rotation,
            targetLayerMask, QueryTriggerInteraction.Collide);
    }

    private void OnDrawGizmos()
    {
        BoxCollider col = box != null ? box : GetComponent<BoxCollider>();
        if (col == null)
        {
            return;
        }
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.5f);
        Gizmos.matrix = col.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(col.center, col.size);
    }
}