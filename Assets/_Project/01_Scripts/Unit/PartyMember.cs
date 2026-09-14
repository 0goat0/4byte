using UnityEngine;
using Fusion;

public class PartyMember : NetworkBehaviour
{
    public Unit Unit { get; private set; }
    [Networked]
    public NetworkParty Party { get; private set; }

    public bool IsAssigned => Party != null;

    private void Awake()
    {
        Unit = GetComponent<Unit>();
    }

    public void AssignParty(NetworkParty party)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        Party = party;
    }

    public void LeaveParty(NetworkParty party)
    {
        if (!Object.HasStateAuthority || Party != party)
            return;

        Party = null;
    }
}
