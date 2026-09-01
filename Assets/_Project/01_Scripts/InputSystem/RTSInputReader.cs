using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RTSInputReader : MonoBehaviour
{
    // 모든 위치는 커서 위치로 화면 좌표 기준

    // 현재 커서 위치
    public Vector2 PointerPosition { get; private set; }
    // 드래그를 위한 현재 클릭을 유지 중인지 나타내는 bool
    public bool IsSelecting { get; private set; }

    // 좌클릭을 시작했을 때 커서 위치
    public event Action<Vector2> SelectStarted;
    // 좌클릭을 중지했을 때 커서 위치
    public event Action<Vector2> SelectEnded;

    // 우클릭 시 커서 위치
    public event Action<Vector2> MoveCommandRequested;
    // 키보드 A 키 입력 시 이벤트
    public event Action AttackCommandRequested;
    // 키보드 S 키 입력 시 이벤트
    public event Action StopCommandRequested;
    // 키보드 T 키 입력 시 이벤트
    public event Action StimpackRequested;
    // 키보드 H 키 입력 시 이벤트
    public event Action HoldPositionRequested;
    // 키보드 Space 입력 시 이벤트
    public event Action FocusSelectionRequested;
    public event Action OnEscapePressed;

    // 마우스 스크롤 시 이벤트
    public event Action<float> ZoomRequested;

    // 방향키 입력
    public Vector2 CameraMoveDirection { get; private set; }

    public void OnPoint(InputAction.CallbackContext context)
    {
        PointerPosition = context.ReadValue<Vector2>();
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        IsSelecting = context.ReadValueAsButton();

        if (context.started)
        {
            SelectStarted?.Invoke(PointerPosition);
        }

        if (context.canceled)
        {
            SelectEnded?.Invoke(PointerPosition);
        }
    }

    public void OnMoveCommand(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        MoveCommandRequested?.Invoke(PointerPosition);
    }

    public void OnAttackCommand(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        AttackCommandRequested?.Invoke();
    }

    public void OnStopCommand(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        StopCommandRequested?.Invoke();
    }

    public void OnStimpack(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        StimpackRequested?.Invoke();
    }

    public void OnHoldPosition(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        HoldPositionRequested?.Invoke();
    }

    public void OnFocusSelection(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        FocusSelectionRequested?.Invoke();
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        float zoomValue = context.ReadValue<float>();
        ZoomRequested?.Invoke(zoomValue);
    }

    public void OnCameraMove(InputAction.CallbackContext context)
    {
        CameraMoveDirection = context.ReadValue<Vector2>();
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnEscapePressed?.Invoke();
    }

    public void OnApplicationFocus(bool focus)
    {
        if (focus)
            return;

        CameraMoveDirection = Vector2.zero;
        IsSelecting = false;
    }
}