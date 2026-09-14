using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkParty : NetworkBehaviour
{
    public const int MaxMemberCount = 3;

    [Networked, Capacity(MaxMemberCount)]
    private NetworkArray<PartyMember> Members => default;

    [Networked]
    public int MemberCount { get; private set; }

    public PartyMember GetMember(int index)
    {
        if (index < 0 || index >= MaxMemberCount)
            return null;

        return Members.Get(index);
    }

    public bool ContainsMember(PartyMember member)
    {
        return FindMemberIndex(member) >= 0;
    }

    public bool TryAddMember(PartyMember member)
    {
        if (!Object.HasStateAuthority)
            return false;

        if (member == null || !member.Object.HasStateAuthority || MemberCount >= MaxMemberCount)
            return false;

        if (ContainsMember(member))
            return false;

        Members.Set(MemberCount, member);
        MemberCount++;

        member.AssignParty(this);
        return true;
    }

    public bool TryRemoveMember(PartyMember member)
    {
        if (!Object.HasStateAuthority || member == null)
            return false;

        int memberIndex = FindMemberIndex(member);

        if (memberIndex < 0)
            return false;

        int lastMemberIndex = MemberCount - 1;

        for (int i = memberIndex; i < lastMemberIndex; i++)
        {
            Members.Set(i, Members.Get(i + 1));
        }

        Members.Set(lastMemberIndex, null);
        MemberCount = lastMemberIndex;

        member.LeaveParty(this);
        return true;
    }

    private int FindMemberIndex(PartyMember member)
    {
        if (member == null)
            return -1;

        for (int i = 0; i < MemberCount; i++)
        {
            if (Members.Get(i) == member)
                return i;
        }

        return -1;
    }

    public void RequestMove(Vector3 destination)
    {
        RPC_RequestMove(destination);
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
    private void RPC_RequestMove(Vector3 destination)
    {
        for (int i = 0; i < MemberCount; i++)
        {
            PartyMember member = Members.Get(i);

            if (member == null || member.Unit == null)
                continue;

            member.Unit.MoveTo(destination);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAttackTarget(NetworkObject target)
    {
        if (target == null)
            return;

        for (int i = 0; i < MemberCount; i++)
        {
            PartyMember member = Members.Get(i);

            if (member == null || member.Unit == null)
                continue;

            member.Unit.AttackTarget(target);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAttackMove(Vector3 destination)
    {
        for (int i = 0; i < MemberCount; i++)
        {
            PartyMember member = Members.Get(i);

            if (member == null || member.Unit == null)
                continue;

            member.Unit.AttackMove(destination);
        }
    }

}
