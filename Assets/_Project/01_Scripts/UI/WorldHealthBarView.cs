using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBarView : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Image _fillImage;

    public void SetAppearance(Sprite sprite, float width, float height)
    {
        _fillImage.sprite = sprite;
        _fillImage.color = Color.white;
        _rectTransform.sizeDelta = new Vector2(width, height);
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        _fillImage.fillAmount = HealthBarMetrics.CalculateFill(currentHealth, maxHealth);
    }

    public void SetPosition(Vector2 localPosition)
    {
        _rectTransform.anchoredPosition = localPosition;
    }

    public void SetVisible(bool isVisible)
    {
        if (gameObject.activeSelf != isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}
