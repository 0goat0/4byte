using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private RTSInputReader inputReader;

    [Header("Camera")]
    [SerializeField] private Transform cameraRig;
    [SerializeField] private Camera mainCamera;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float edgeSize = 20f;

    [Header("Movement Bounds")]
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minZ = -50f;
    [SerializeField] private float maxZ = 50f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 0.01f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Focus")]
    [SerializeField] private Transform target;


    private void Start()
    {
        // 메인 카메라 설정
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        // 카메라 입력 이벤트 등록
        inputReader.ZoomRequested += HandleZoom;
        inputReader.FocusSelectionRequested += HandleFocusSelection;
    }

    private void OnDisable()
    {
        // 카메라 입력 이벤트 해제
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

        // 화면 가장자리 및 이동 입력 확인
        if (pointerPosition.x <= edgeSize || cameraMoveInput.x < 0)
            horizontal = -1f;
        else if (pointerPosition.x >= Screen.width - edgeSize || cameraMoveInput.x > 0)
            horizontal = 1f;

        if (pointerPosition.y <= edgeSize || cameraMoveInput.y < 0)
            vertical = -1f;
        else if (pointerPosition.y >= Screen.height - edgeSize || cameraMoveInput.y > 0)
            vertical = 1f;

        // CameraRig 기준으로 이동 방향 계산
        Vector3 moveDirection = cameraRig.forward * vertical + cameraRig.right * horizontal;

        moveDirection = moveDirection.normalized;

        Vector3 position = cameraRig.position + moveDirection * moveSpeed * Time.deltaTime;

        // 카메라 이동 범위 제한
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);

        cameraRig.position = position;
    }

    private void HandleZoom(float zoomValue)
    {
        if (!CanControlCamera())
            return;

        // Orthographic 카메라 줌 처리
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

        // 선택된 대상 위치로 카메라 이동
        Vector3 position = target.position;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);

        cameraRig.position = position;
    }

    private bool CanControlCamera()
    {
        // 게임 창이 활성화되고 커서가 제한된 상태에서만 카메라 조작 허용
        return Application.isFocused &&
            Cursor.lockState == CursorLockMode.Confined;
    }
}
