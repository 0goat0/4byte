using UnityEngine;

public class UnitSelectionVisual : MonoBehaviour
{
    [SerializeField]
    private Renderer _indicatorRenderer;

    private void Awake()
    {
        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (_indicatorRenderer == null)
            return;

        _indicatorRenderer.enabled = isSelected;
    }
}
