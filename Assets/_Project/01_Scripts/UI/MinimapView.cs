using UnityEngine;

public class MinimapView : MonoBehaviour
{
    public Camera playerCamera;
    public Collider floorCollider;

    private LineRenderer lineRenderer;

    private Vector3[] worldPositions = new Vector3[5];

    public void Start()
    {
        this.lineRenderer = this.GetComponent<LineRenderer>();


        if (this.floorCollider == null)
        {
            GameObject floorObject = GameObject.FindGameObjectWithTag("FloorCollider");
            if (floorObject != null) this.floorCollider = floorObject.GetComponent<Collider>();
        }

        this.lineRenderer.positionCount = 5;
    }

    public void Update()
    {
        if (this.playerCamera == null || this.floorCollider == null || this.lineRenderer == null) return;

        Ray bottomLeftCorner = this.playerCamera.ScreenPointToRay(new Vector3(0f, 0f, 0f));
        Ray bottomRightCorner = this.playerCamera.ScreenPointToRay(new Vector3(Screen.width, 0f, 0f));
        Ray topRightCorner = this.playerCamera.ScreenPointToRay(new Vector3(Screen.width, Screen.height, 0f));
        Ray topLeftCorner = this.playerCamera.ScreenPointToRay(new Vector3(0f, Screen.height, 0f));

        RaycastHit hit;
        // 월드좌표
        if (this.floorCollider.Raycast(bottomLeftCorner, out hit, 1500f)) worldPositions[0] = hit.point;
        if (this.floorCollider.Raycast(bottomRightCorner, out hit, 1500f)) worldPositions[1] = hit.point;
        if (this.floorCollider.Raycast(topRightCorner, out hit, 1500f)) worldPositions[2] = hit.point;
        if (this.floorCollider.Raycast(topLeftCorner, out hit, 1500f)) worldPositions[3] = hit.point;

        worldPositions[4] = worldPositions[0];

        // 선 위치설정
        for (int i = 0; i < worldPositions.Length; i++)
        {
            worldPositions[i].y += 10f;
        }

        this.lineRenderer.SetPositions(worldPositions);
    }
}