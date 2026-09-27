using UnityEngine;

public class MinimapView : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float minimapLineHeight = 1.0f; //높이(Y축)

    private LineRenderer lineRenderer;
    private Vector3[] viewportPoints = new Vector3[3];
    private Vector3[] worldPoints = new Vector3[3];

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        //모서리 Viewport 좌표 설정
        viewportPoints[0] = new Vector3(0, 0, 0);
        viewportPoints[1] = new Vector3(1, 0, 0);
        viewportPoints[2] = new Vector3(1, 1, 0);
        viewportPoints[3] = new Vector3(0, 1, 0);
    }

    private void LateUpdate()
    {
        if (mainCamera == null || lineRenderer == null) return;

        UpdateFrustumLines();
    }

    private void UpdateFrustumLines()
    {

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, minimapLineHeight, 0));

        for (int i = 0; i < 4; i++)
        {
            //화면 모서리 Ray 발사
            Ray ray = mainCamera.ViewportPointToRay(viewportPoints[i]);

            if (groundPlane.Raycast(ray, out float enter))
            {
                worldPoints[i] = ray.GetPoint(enter);
            }
            else
            {
                worldPoints[i] = mainCamera.transform.position + ray.direction * 50f;
                worldPoints[i].y = minimapLineHeight;
            }
        }

        worldPoints[3] = worldPoints[0];

        // Line Renderer에 계산된 4개의 좌표 전달
        lineRenderer.SetPositions(worldPoints);
    }
}

