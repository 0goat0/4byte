using UnityEngine;

[CreateAssetMenu(fileName = "WorldHealthBarSettings", menuName = "UI/World Health Bar Settings")]
public class WorldHealthBarSettings : ScriptableObject
{
    [Header("Appearance")]
    [SerializeField] private Sprite _allySprite;
    [SerializeField] private Sprite _enemySprite;
    [SerializeField] private LayerMask _allyLayers;
    [SerializeField] private LayerMask _enemyLayers;

    [Header("Size (Canvas Units)")]
    [SerializeField, Min(0f)] private float _widthPerWorldUnit = 18f;
    [SerializeField, Min(0f)] private float _healthWidthScale = 0.4f;
    [SerializeField, Min(1f)] private float _minWidth = 35f;
    [SerializeField, Min(1f)] private float _maxWidth = 140f;
    [SerializeField, Min(1f)] private float _height = 6f;

    [Header("Pool")]
    [SerializeField, Min(0)] private int _initialPoolSize = 20;

    public Sprite AllySprite => _allySprite;
    public Sprite EnemySprite => _enemySprite;
    public float Height => _height;
    public int InitialPoolSize => _initialPoolSize;

    public bool IsEnemy(int layer)
    {
        return (_enemyLayers.value & (1 << layer)) != 0;
    }

    public bool CanDisplay(int layer)
    {
        return ((_allyLayers.value | _enemyLayers.value) & (1 << layer)) != 0;
    }

    public float CalculateWidth(float targetSize, float maxHealth)
    {
        return HealthBarMetrics.CalculateWidth(targetSize, maxHealth, _widthPerWorldUnit, _healthWidthScale, _minWidth, _maxWidth);
    }

    private void OnValidate()
    {
        _minWidth = Mathf.Max(1f, _minWidth);
        _maxWidth = Mathf.Max(_minWidth, _maxWidth);
        _height = Mathf.Max(1f, _height);
        _widthPerWorldUnit = Mathf.Max(0f, _widthPerWorldUnit);
        _healthWidthScale = Mathf.Max(0f, _healthWidthScale);
        _initialPoolSize = Mathf.Max(0, _initialPoolSize);
    }
}
