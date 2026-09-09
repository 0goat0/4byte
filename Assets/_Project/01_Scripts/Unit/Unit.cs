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

}
