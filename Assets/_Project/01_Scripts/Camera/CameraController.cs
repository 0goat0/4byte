using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private RTSInputReader inputReader;

    [SerializeField] private Transform cameraRig;
    [SerializeField] private Camera mainCamera;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float edgeSize = 20f;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minZ = -50f;
    [SerializeField] private float maxZ = 50f;

    [SerializeField] private float zoomSpeed = 0.01f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    [SerializeField] private Transform target;


    private void Start()
    {
        // 메인 카메라 설정
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        inputReader.ZoomRequested += HandleZoom;
        inputReader.FocusSelectionRequested += HandleFocusSelection;
    }

    private void OnDisable()
    {
        inputReader.ZoomRequested -= HandleZoom;
        inputReader.FocusSelectionRequested -= HandleFocusSelection;
    }

    private void Update()
    {
        HandleCameraMovement();
    }

    private void HandleCameraMovement()
    {
        if (!CanControlCamera())
            return;

        Vector2 pointerPosition = inputReader.PointerPosition;
        Vector2 cameraMoveInput = inputReader.CameraMoveDirection;

        float horizontal = 0f;
        float vertical = 0f;

        if (pointerPosition.x <= edgeSize || cameraMoveInput.x < 0)
            horizontal = -1f;
        else if (pointerPosition.x >= Screen.width - edgeSize || cameraMoveInput.x > 0)
            horizontal = 1f;

        if (pointerPosition.y <= edgeSize || cameraMoveInput.y < 0)
            vertical = -1f;
        else if (pointerPosition.y >= Screen.height - edgeSize || cameraMoveInput.y > 0)
            vertical = 1f;

        Vector3 moveDirection = cameraRig.forward * vertical + cameraRig.right * horizontal;

        moveDirection = moveDirection.normalized;

        Vector3 position = cameraRig.position + moveDirection * moveSpeed * Time.deltaTime;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);

        cameraRig.position = position;
    }

    private void HandleZoom(float zoomValue)
    {
        if (!CanControlCamera())
            return;

        // 휠 입력의 부호만 받아옴 (1 or -1)
        float zoomDirection = Mathf.Sign(zoomValue);
        float nextSize = mainCamera.orthographicSize - zoomSpeed * zoomDirection;

        mainCamera.orthographicSize = Mathf.Clamp(nextSize, minZoom, maxZoom);
    }

    private void HandleFocusSelection()
    {
        if (!CanControlCamera())
            return;

        if (target == null)
            return;

        Vector3 position = target.position;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);

        cameraRig.position = position;
    }

    private bool CanControlCamera()
    {
        return Application.isFocused &&
            Cursor.lockState == CursorLockMode.Confined;
    }
}
