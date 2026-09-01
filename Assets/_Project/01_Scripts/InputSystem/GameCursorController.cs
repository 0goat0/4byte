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
        inputReader.OnEscapePressed += ReleaseCursor;
        inputReader.SelectStarted += HandleScreenClicked;

        ConfineCursor();
    }

    private void OnDisable()
    {
        inputReader.OnEscapePressed -= ReleaseCursor;
        inputReader.SelectStarted -= HandleScreenClicked;

        ReleaseCursor();
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ConfineCursor();
        }
        else
        {
            ReleaseCursor();
        }
    }

    private void HandleScreenClicked(Vector2 pointerPosition)
    {
        if (!Application.isFocused)
            return;

        if (Cursor.lockState == CursorLockMode.None)
            ConfineCursor();
    }

    private void ConfineCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
