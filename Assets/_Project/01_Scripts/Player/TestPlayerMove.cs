using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class TestPlayerMove : NetworkBehaviour
{
    public float moveSpeed = 5f;

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                transform.Translate(Vector3.forward * moveSpeed * Runner.DeltaTime, Space.World);
            }
            if (Keyboard.current.sKey.isPressed)
            {
                transform.Translate(Vector3.back * moveSpeed * Runner.DeltaTime, Space.World);
            }
        }
    }
}