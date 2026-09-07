using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NavMeshTest : NetworkBehaviour
{
    [SerializeField] private List<Transform> _patrolPoints = new();

    private NetworkNavMeshMover _mover;
    private int _currentPointIndex;

    private void Awake()
    {
        _mover = GetComponent<NetworkNavMeshMover>();
    }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority || _patrolPoints.Count == 0)
            return;

        _mover.OnDestinationReached += MoveToNextPoint;
        MoveToCurrentPoint();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _mover.OnDestinationReached -= MoveToNextPoint;
    }

    private void MoveToCurrentPoint()
    {
        Transform currentPoint = _patrolPoints[_currentPointIndex];

        if (currentPoint == null)
            return;

        _mover.MoveTo(currentPoint.position);
    }

    private void MoveToNextPoint()
    {
        _currentPointIndex =
            (_currentPointIndex + 1) % _patrolPoints.Count;

        MoveToCurrentPoint();
    }
}