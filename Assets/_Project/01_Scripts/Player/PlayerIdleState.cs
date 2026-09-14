using UnityEngine;
using Fusion;

public class PlayerIdleState : IPlayerState
{
    private readonly Collider[] _detectResults =new Collider[10];
    private Camera _mainCamera;

    public void Enter(PlayerStats player)
    {
        _mainCamera = Camera.main;
        player.Target = null;

    }
    public void Tick(PlayerStats player)
    {
        //주변 탐색
        int count = Physics.OverlapSphereNonAlloc(player.transform.position, player.DetectRange, _detectResults);
        for (int i = 0; i < count; i++)
        {
            var hit = _detectResults[i];
            if (hit.gameObject != player.gameObject && hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent<NetworkObject>(out var networkObj))
                {
                    player.Target = networkObj;
                }

                player.ChangeState(PlayerStateType.Detect); // 타겟을 찾으면 자동 사냥 모드(Detect)로 전이
                return;
            }
        }
        if (player.Object.HasInputAuthority && Input.GetMouseButtonDown(0))
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            if (_mainCamera != null)
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);


                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    Debug.Log("collider check");
                    // 3. GetComponent로 받아둔 mover를 활용하여 목적지로 이동시킵니다.
                    if (player.Mover != null)
                    {
                        Debug.Log("navmeshagent check");
                        player.Mover.MoveTo(hitInfo.point);
                    }
                }
            }
        }
    }
    public void Exit(PlayerStats player)
    {

    }
}
