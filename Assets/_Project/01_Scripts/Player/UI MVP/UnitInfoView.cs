using TMPro;
using UnityEngine;

public interface IUnitInfoView
{
    void UpdateUnitInfoUI(string displayName);
    void Hide();
}

public class UnitInfoView : MonoBehaviour, IUnitInfoView
{
    [SerializeField] private TextMeshProUGUI _nameText;

    public void UpdateUnitInfoUI(string displayName)
    {
        _nameText.text = displayName;
    }

    public void Hide()
    {
        _nameText.text = string.Empty;
    }
}
