using Fusion;
using UnityEngine;

public static class DistanceUtil 
{
    public static float GetDistanceToTarget(Vector3 fromPos, NetworkObject target)
    {
        Collider targetCol = target.GetComponent<Collider>();
        if (targetCol != null)
        {
            Vector3 closest = targetCol.ClosestPoint(fromPos);
            return Vector3.Distance(fromPos, closest);
        }
        return Vector3.Distance(fromPos, target.transform.position);
    }
}
