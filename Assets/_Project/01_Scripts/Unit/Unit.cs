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
}
