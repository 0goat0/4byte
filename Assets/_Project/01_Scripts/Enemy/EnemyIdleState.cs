using Fusion;
using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    public void Enter(EnemyAI enemy)
    {
        if(enemy.agent != null)
        {
            enemy.agent.isStopped = false;
            enemy.agent.SetDestination(TargetDestination.Instance.transform.position);
        }
    }

    public void Exit(EnemyAI enemy)
    {
        
    }

    public void Tick(EnemyAI enemy)
    {
        NetworkObject detectObj = DetectTarget(enemy);
        //Debug.Log("감지중");
        if (detectObj != null)
        {
            enemy.Target = detectObj;
            enemy.ChangeState(EnemyStateType.Detect);
        }
    }

    //제일 가까운 레이어에 걸리는 네트워크 오브젝트를 찾아냄.
    private NetworkObject DetectTarget(EnemyAI enemy)
    {
        Collider[] hits = Physics.OverlapSphere(enemy.transform.position, enemy.DetectRange, enemy.TargetLayerMask);

        NetworkObject returnObj = null;

        float minDistance = float.MaxValue;
        foreach(var hit in hits)
        {
            NetworkObject netObj = hit.GetComponent<NetworkObject>();
            if(netObj == null)
            {
                continue;
            }

            float distance = Vector3.Distance(enemy.transform.position, hit.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                returnObj = netObj;
            }
        }
        return returnObj;
    }
}
