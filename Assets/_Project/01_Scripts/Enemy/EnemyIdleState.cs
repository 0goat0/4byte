using Fusion;
using System;
using UnityEngine;

public class EnemyIdleState : IEnemyState
{

    private Action onArrivedHandler;
    public void Enter(EnemyAI enemy)
    {
        enemy.IsAlerted = false;
        if (enemy.Mover != null)
        {
            onArrivedHandler = () => OnArrived(enemy);
            enemy.Mover.OnDestinationReached += onArrivedHandler;
            MoveToCurrentDestination(enemy);
        }
    }

    public void Exit(EnemyAI enemy)
    {
        if (enemy.Mover != null && onArrivedHandler != null)
        {
            enemy.Mover.OnDestinationReached -= onArrivedHandler;
            onArrivedHandler = null;
        }
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
    private void OnArrived(EnemyAI enemy)
    {
        // Detect 등 다른 상태로 이미 넘어간 뒤에는 다음 목적지로 이동시키지 않음
        if (enemy.StateType != EnemyStateType.Idle)
            return;

        enemy.DestinationIndex = TargetDestination.Instance.GetNextIndex(enemy.DestinationIndex);
        MoveToCurrentDestination(enemy);
    }

    private void MoveToCurrentDestination(EnemyAI enemy)
    {
        if (enemy.Mover == null || TargetDestination.Instance == null)
            return;

        // NetworkObject의 고유 ID를 시드로 사용 → 유닛마다 다른 지점, 항상 동일한 값
        int seed = (int)enemy.Object.Id.Raw ^ enemy.DestinationIndex;
        Vector3 destination = TargetDestination.Instance.GetDestination(enemy.DestinationIndex, seed);
        enemy.Mover.MoveTo(destination);
    }

}
