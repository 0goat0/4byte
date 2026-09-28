using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldHealthBarManager : MonoBehaviour
{
    private const string ResourcePath = "WorldHealthBars";

    [SerializeField] private WorldHealthBarSettings _settings;
    [SerializeField] private WorldHealthBarView _healthBarPrefab;
    [SerializeField] private RectTransform _healthBarRoot;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Camera _worldCamera;

    private class TargetBinding
    {
        public WorldHealthBarTarget Target;
        public WorldHealthBarView View;
        public Action<float, float> OnHealthChanged;
    }

    private static WorldHealthBarManager _instance;

    private readonly Dictionary<WorldHealthBarTarget, TargetBinding> _bindings = new();
    private readonly List<TargetBinding> _activeBindings = new();
    private readonly Stack<WorldHealthBarView> _pooledViews = new();
    private PlayerInteractionState _interactionState;
    private bool _isInitialized;

    public static void EnsureExists()
    {
        if (_instance != null || !Application.isPlaying || Application.isBatchMode)
        {
            return;
        }

        WorldHealthBarManager prefab = Resources.Load<WorldHealthBarManager>(ResourcePath);

        if (prefab == null)
        {
            Debug.LogError($"Resources/{ResourcePath} 체력바 프리팹을 찾을 수 없습니다.");
            return;
        }

        Instantiate(prefab.gameObject);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        _instance = null;
    }

    private void Awake()
    {
        if (Application.isBatchMode)
        {
            enabled = false;
            return;
        }

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (_settings == null || _healthBarPrefab == null || _healthBarRoot == null || _canvas == null)
        {
            Debug.LogError("HP 바 설정, 프리팹, 루트, Canvas를 연결해 주세요.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < _settings.InitialPoolSize; i++)
        {
            _pooledViews.Push(CreateView());
        }

        _isInitialized = true;
    }

    private void OnEnable()
    {
        if (!_isInitialized)
        {
            return;
        }

        WorldHealthBarTarget.OnRegistered += Register;
        WorldHealthBarTarget.OnUnregistered += Unregister;
        BindInteractionState();

        foreach (WorldHealthBarTarget target in WorldHealthBarTarget.RegisteredTargets)
        {
            Register(target);
        }
    }

    private void OnDisable()
    {
        WorldHealthBarTarget.OnRegistered -= Register;
        WorldHealthBarTarget.OnUnregistered -= Unregister;
        UnbindInteractionState();

        foreach (TargetBinding binding in _bindings.Values)
        {
            binding.Target.OnHealthChanged -= binding.OnHealthChanged;
            ReturnView(binding);
        }

        _bindings.Clear();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Register(WorldHealthBarTarget target)
    {
        if (target == null || _bindings.ContainsKey(target) || !_settings.CanDisplay(target.gameObject.layer))
        {
            return;
        }

        TargetBinding binding = new TargetBinding { Target = target };
        binding.OnHealthChanged = (currentHealth, maxHealth) => Refresh(binding, currentHealth, maxHealth);
        target.OnHealthChanged += binding.OnHealthChanged;
        _bindings.Add(target, binding);
        Refresh(binding, target.CurrentHealth, target.MaxHealth);
    }

    private void Unregister(WorldHealthBarTarget target)
    {
        if (!_bindings.TryGetValue(target, out TargetBinding binding))
        {
            return;
        }

        target.OnHealthChanged -= binding.OnHealthChanged;
        ReturnView(binding);
        _bindings.Remove(target);
    }

    private void Refresh(TargetBinding binding, float currentHealth, float maxHealth)
    {
        bool isSelected = _interactionState != null && binding.Target.MatchesSelection(_interactionState.InfoTarget);

        if (!HealthBarMetrics.ShouldShow(currentHealth, maxHealth, isSelected))
        {
            ReturnView(binding);
            return;
        }

        if (binding.View == null)
        {
            binding.View = _pooledViews.Count > 0 ? _pooledViews.Pop() : CreateView();
            _activeBindings.Add(binding);
        }

        bool isEnemy = _settings.IsEnemy(binding.Target.gameObject.layer);
        Sprite sprite = isEnemy ? _settings.EnemySprite : _settings.AllySprite;
        float width = _settings.CalculateWidth(binding.Target.TargetSize, maxHealth);
        binding.View.SetAppearance(sprite, width, _settings.Height);
        binding.View.SetHealth(currentHealth, maxHealth);
    }

    private void BindInteractionState()
    {
        UnbindInteractionState();
        _interactionState = FindFirstObjectByType<PlayerInteractionState>();

        if (_interactionState == null)
        {
            return;
        }

        _interactionState.OnSelectionChanged += RefreshSelection;
        RefreshSelection();
    }

    private void UnbindInteractionState()
    {
        if (_interactionState != null)
        {
            _interactionState.OnSelectionChanged -= RefreshSelection;
        }

        _interactionState = null;
    }

    private void RefreshSelection()
    {
        foreach (TargetBinding binding in _bindings.Values)
        {
            Refresh(binding, binding.Target.CurrentHealth, binding.Target.MaxHealth);
        }
    }

    private void ReturnView(TargetBinding binding)
    {
        if (binding.View == null)
        {
            return;
        }

        binding.View.SetVisible(false);
        _pooledViews.Push(binding.View);
        binding.View = null;
        _activeBindings.Remove(binding);
    }

    private WorldHealthBarView CreateView()
    {
        WorldHealthBarView view = Instantiate(_healthBarPrefab, _healthBarRoot);
        view.SetVisible(false);
        return view;
    }

    private void LateUpdate()
    {
        if (_activeBindings.Count == 0)
        {
            return;
        }

        if (_worldCamera == null || !_worldCamera.isActiveAndEnabled)
        {
            _worldCamera = Camera.main;
        }

        Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        for (int i = 0; i < _activeBindings.Count; i++)
        {
            TargetBinding binding = _activeBindings[i];

            if (_worldCamera == null || binding.Target == null)
            {
                binding.View.SetVisible(false);
                continue;
            }

            Vector3 screenPosition = _worldCamera.WorldToScreenPoint(binding.Target.WorldPosition);
            bool isVisible = screenPosition.z > 0f && _worldCamera.pixelRect.Contains(screenPosition);

            if (isVisible && RectTransformUtility.ScreenPointToLocalPointInRectangle(_healthBarRoot, screenPosition, uiCamera, out Vector2 localPosition))
            {
                binding.View.SetPosition(localPosition);
                binding.View.SetVisible(true);
            }
            else
            {
                binding.View.SetVisible(false);
            }
        }
    }
}
