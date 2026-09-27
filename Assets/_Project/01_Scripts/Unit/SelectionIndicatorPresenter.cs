using UnityEngine;

public class SelectionIndicatorPresenter : MonoBehaviour
{
    [SerializeField]
    private PlayerInteractionState _interactionState;

    private UnitSelectionVisual _currentVisual;

    private void OnEnable()
    {
        _interactionState.OnSelectionChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        _interactionState.OnSelectionChanged -= Refresh;
        ClearCurrentVisual();
    }

    private void Refresh()
    {
        UnitSelectionVisual nextVisual = FindSelectedVisual();

        if (_currentVisual == nextVisual)
            return;

        ClearCurrentVisual();

        _currentVisual = nextVisual;

        if (_currentVisual != null)
        {
            _currentVisual.SetSelected(true);
        }
    }

    private UnitSelectionVisual FindSelectedVisual()
    {
        if (_interactionState.InfoTarget is not Unit unit)
        {
            return null;
        }

        unit.TryGetComponent(out UnitSelectionVisual selectionVisual);

        return selectionVisual;
    }

    private void ClearCurrentVisual()
    {
        if (_currentVisual != null)
        {
            _currentVisual.SetSelected(false);
        }

        _currentVisual = null;
    }
}
