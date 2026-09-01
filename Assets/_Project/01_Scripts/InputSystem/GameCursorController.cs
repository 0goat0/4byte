using UnityEngine;

public class GameCursorController : MonoBehaviour
{
    [SerializeField] private RTSInputReader inputReader;

    private void Awake()
    {
        inputReader = GetComponent<RTSInputReader>();
    }

    private void OnEnable()
    {
        // ESC 키를 누르면 커서를 게임 창 밖으로 이동할 수 있도록 해제
        inputReader.OnEscapePressed += ReleaseCursor;
        // 화면을 클릭하면 커서를 다시 게임 창 내부에 제한
        inputReader.SelectStarted += HandleScreenClicked;

        // 오브젝트가 활성화될 때 기본적으로 커서를 게임 창 내부에 제한
        ConfineCursor();
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 등록했던 이벤트 해제
        inputReader.OnEscapePressed -= ReleaseCursor;
        inputReader.SelectStarted -= HandleScreenClicked;

        // 게임 시스템이 비활성화되면 커서 제한도 해제
        ReleaseCursor();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // 게임 창이 다시 포커스를 얻으면 커서를 게임 창 내부에 제한
        if (hasFocus)
        {
            ConfineCursor();
        }
        else
        {
            // 게임 창이 포커스를 잃으면 커서 제한을 해제해서 
            // 다른 프로그램이나 창을 자유롭게 사용할 수 있도록 함
            ReleaseCursor();
        }
    }

    private void HandleScreenClicked(Vector2 pointerPosition)
    {
        // 현재 게임 창이 포커스 상태가 아니면 처리하지 않음
        if (!Application.isFocused)
            return;

        // ESC 등으로 커서 제한이 해제된 상태에서 화면을 다시 클릭하면
        // 커서를 게임 창 내부로 다시 제한
        if (Cursor.lockState == CursorLockMode.None)
            ConfineCursor();
    }

    private void ConfineCursor()
    {
        // 커서를 게임 창 내부에서만 움직일 수 있도록 제한
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void ReleaseCursor()
    {
        // 커서 제한을 해제하여 게임 창 밖으로 이동할 수 있도록 함
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
