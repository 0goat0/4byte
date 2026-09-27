using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetDestination : MonoBehaviour
{
    public static TargetDestination Instance;

    [SerializeField] private List<Transform> destinations = new List<Transform>();
    [SerializeField] private float spreadRadius;
    public int Count {  get { return destinations.Count; } }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public Vector3 GetDestination(int index, int offsetSeed = 0)
    {
        if (destinations.Count == 0)
        {
            Debug.LogWarning("TargetDestination: 등록된 목적지가 없습니다.");
            return transform.position;
        }

        int wrapped = ((index % destinations.Count) + destinations.Count) % destinations.Count;

        Vector3 basePos = destinations[wrapped].position;
        if (spreadRadius <= 0f)
        {
            return basePos;
        }
        // 유닛마다 고유한 시드로 오프셋을 고정 → 매번 값이 바뀌지 않음
        System.Random rng = new System.Random(offsetSeed);
        float angle = (float)(rng.NextDouble() * Mathf.PI * 2);
        float dist = (float)(rng.NextDouble() * spreadRadius);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;

        Vector3 candidate = basePos + offset;

        // NavMesh 위의 유효한 지점으로 스냅
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, spreadRadius + 1f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return basePos;

    }

    public int GetNextIndex(int currentIndex)
    {
        if (destinations.Count == 0) return 0;
        return (currentIndex + 1) % destinations.Count;
    }

}
