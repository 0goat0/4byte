using Fusion;
using System;
using UnityEngine;

[Serializable]
public struct UnitData : INetworkStruct
{    
    public NetworkString<_32> Name;

}
