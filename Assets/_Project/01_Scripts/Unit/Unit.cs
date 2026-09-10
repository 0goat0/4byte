using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class Unit : NetworkBehaviour, ISelectable
{
    [SerializeField] private UnitData _unitData;
    public UnitData UnitData => _unitData;

    private NetworkNavMeshMover _mover;

    private void Awake()
    {
        _mover = GetComponent<NetworkNavMeshMover>();
    }

    public override void Spawned()
    {
        if (!Runner.IsServer)
            return;

        Object.AssignInputAuthority(Runner.LocalPlayer);
    }

    public void RequestMove(Vector3 destination)
    {
        RPC_RequestMove(destination);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestMove(Vector3 destination)
    {
        _mover.MoveTo(destination);
    }

    public void RequestAttackTarget(NetworkObject target)
    {
        RPC_RequestAttackTarget(target);
    }

    public void RequestAttackMove(Vector3 destination)
    {
        RPC_RequestAttackMove(destination);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAttackTarget(NetworkObject target)
    {
        // 요청이 전달되는 사이 대상이 디스폰될 수 있습니다.
        if (target == null)
            return;

        Debug.Log($"대상 공격 요청: {target.name}", this);

        // 상태머신 구현 시 목표를 저장하고 Chase 또는 Attack으로 연결합니다.
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAttackMove(Vector3 destination)
    {
        Debug.Log($"공격 이동 요청: {destination}", this);

        // 상태머신 구현 시 목적지를 저장하고 AttackMove로 연결합니다.
    }
}
