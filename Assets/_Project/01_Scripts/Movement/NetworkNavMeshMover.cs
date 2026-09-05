using Fusion;
using System;
using UnityEngine;
using UnityEngine.AI;

// 이동 계산과 네트워크 위치 동기화에 필요한 컴포넌트를 보장
[RequireComponent(typeof(NavMeshAgent), typeof(NetworkTransform))]
public class NetworkNavMeshMover : NetworkBehaviour
{
    // 현재 이동 목적지 확인 및 디버깅용
    [SerializeField] private Vector3 _destination;

    private const float ArrivalSpeedSqr = 0.01f;

    private NavMeshAgent _agent;
    private bool _hasActiveDestination;

    // NavMesh 에는 도착 이벤트가 없으므로 직접 판정하여 외부에 전달
    public event Action OnDestinationReached;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public override void Spawned()
    {
        // 이동은 StateAuthority 만 계산하고,
        // 클라이언트는 NetworkTransform으로 결과값만 전달받음
        _agent.enabled = Object.HasStateAuthority;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasReachedDestination())
            return;
        
        // 이벤트에서 새로운 목적지가 설정될 수 있으므로 먼저 현재 이동을 환료 처리
        _hasActiveDestination = false;
        OnDestinationReached?.Invoke();
    }

    private bool HasReachedDestination()
    {
        // 이동 명령이 없거나 아직 경로를 계산 중이면 도착 판정을 하지 않음
        if (!_hasActiveDestination || _agent.pathPending)
            return false;

        bool isWithinStoppingDistance = _agent.remainingDistance <= _agent.stoppingDistance;

        bool isMovingSlowEnough = _agent.velocity.sqrMagnitude <= ArrivalSpeedSqr;

        // 거리만으로는 이동 중인 상태를,
        // 속도만으로는 장애물에 막힌 상태를 도착으로 오인할 수 있어 함께 확인
        return isWithinStoppingDistance && isMovingSlowEnough;
    }

    public void MoveTo(Vector3 destination)
    {
        // 호출하는 상태나 컨트롤러에서 권한 검사를 반복하지 않도록 내부에서 검증
        if (!Object.HasStateAuthority)
            return;

        _destination = destination;

        // 목적지 설정 요청이 받아들여진 경우에만 도착 판정을 시작
        _hasActiveDestination = _agent.SetDestination(_destination);
    }

    public void Stop()
    {
        // 호출하는 상태나 컨트롤러에서 권한 검사를 반복하지 않도록 내부에서 검증
        if (!Object.HasStateAuthority)
            return;

        // 일시 정지가 아니라 현재 이동 명령을 취소하므로 경로를 제거
        _agent.ResetPath();

        _hasActiveDestination = false;
    }
}
