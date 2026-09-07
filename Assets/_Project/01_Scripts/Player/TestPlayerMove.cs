using UnityEngine;
using Fusion;


public class TestPlayerMove : NetworkBehaviour
{
    public float moveSpeed = 5f;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Vector3 moveDirection = data.movementInput.normalized;

            // NetworkTransform이 위치를 부드럽게 보간
            transform.position += moveDirection * moveSpeed * Runner.DeltaTime;
        }
    }
}