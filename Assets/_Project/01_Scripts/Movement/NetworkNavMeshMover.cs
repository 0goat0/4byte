using Fusion;
using System;
using UnityEngine;
using UnityEngine.AI;

// 컴포넌트에 NavMeshAgent 가 없으면 자동으로 추가
[RequireComponent(typeof(NavMeshAgent), typeof(NetworkTransform))]
public class NetworkNavMeshMover : NetworkBehaviour
{
    // 목적지
    [SerializeField] private Vector3 _destination;

    private const float ArrivalSpeedSqr = 0.01f;

    private NavMeshAgent _agent;
    private bool _hasActiveDestination;

    public event Action OnDestinationReached;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public override void Spawned()
    {
        _agent.enabled = Object.HasStateAuthority;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasReachedDestination())
            return;
        
        _hasActiveDestination = false;
        OnDestinationReached?.Invoke();
    }

    private bool HasReachedDestination()
    {
        if (!_hasActiveDestination || _agent.pathPending)
            return false;

        bool isWithinStoppingDistance = _agent.remainingDistance <= _agent.stoppingDistance;

        bool isMovingSlowEnough = _agent.velocity.sqrMagnitude <= ArrivalSpeedSqr;

        return isWithinStoppingDistance && isMovingSlowEnough;
    }

    public void MoveTo(Vector3 destination)
    {
        if (!Object.HasStateAuthority)
            return;

        _destination = destination;
        _hasActiveDestination = _agent.SetDestination(_destination);
    }

    public void Stop()
    {
        if (!Object.HasStateAuthority)
            return;

        _agent.ResetPath();

        _hasActiveDestination = false;
    }
}
