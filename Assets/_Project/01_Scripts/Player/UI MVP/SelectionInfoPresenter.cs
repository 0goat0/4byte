using UnityEngine;

public class SelectionInfoPresenter : MonoBehaviour
{
    [SerializeField] private PlayerInteractionState _state;
    [SerializeField] private UnitInfoView _unitView;

    private void OnEnable()
    {
        _state.OnSelectionChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        _state.OnSelectionChanged -= Refresh;
    }

    private void Refresh()
    {
        if (_state.InfoTarget is Unit unit)
        {
            _unitView.UpdateUnitInfoUI(unit.UnitData.Name.ToString());
            return;
        }

        _unitView.Hide();
    }
}
