using UnityEngine;
using UnityEngine.EventSystems;

public class MiniMapManager : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public Camera minimapCamera;
    public Transform mainCameraTarget;

    public void OnPointerClick(PointerEventData eventData)
    {
        MoveMainCamera(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveMainCamera(eventData);
    }

    private void MoveMainCamera(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        Rect rect = (transform as RectTransform).rect;

        float xPercent = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float yPercent = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        Ray ray = minimapCamera.ViewportPointToRay(new Vector3(xPercent, yPercent, 0));

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPosition =hit.point;
            targetPosition.y = mainCameraTarget.position.y;
            mainCameraTarget.position = targetPosition;
        }
    }
}
