using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WorldHealthBarTarget : MonoBehaviour
{
    private static readonly HashSet<WorldHealthBarTarget> _registeredTargets = new();

    private IHealthSource _healthSource;
    private ISelectable _selectable;
    private Vector3 _localAnchorPosition;
    private bool _isRegistered;

    public static IEnumerable<WorldHealthBarTarget> RegisteredTargets => _registeredTargets;
    public static event Action<WorldHealthBarTarget> OnRegistered;
    public static event Action<WorldHealthBarTarget> OnUnregistered;

    public event Action<float, float> OnHealthChanged;

    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public float TargetSize { get; private set; }
    public Vector3 WorldPosition => transform.TransformPoint(_localAnchorPosition);

    public static WorldHealthBarTarget Attach(GameObject owner, IHealthSource healthSource)
    {
        if (!owner.TryGetComponent(out WorldHealthBarTarget target))
        {
            target = owner.AddComponent<WorldHealthBarTarget>();
        }

        target.Initialize(healthSource);
        return target;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry()
    {
        _registeredTargets.Clear();
        OnRegistered = null;
        OnUnregistered = null;
    }

    public void Initialize(IHealthSource healthSource)
    {
        Release();
        _healthSource = healthSource;

        if (_healthSource == null)
        {
            return;
        }

        CalculateLayout();
        _selectable = GetComponent<ISelectable>();
        CurrentHealth = _healthSource.CurrentHealth;
        MaxHealth = _healthSource.MaxHealth;
        _healthSource.OnHealthChanged += HandleHealthChanged;
        WorldHealthBarManager.EnsureExists();
        Register();
    }

    public void Release()
    {
        Unregister();
        _selectable = null;

        if (_healthSource == null)
        {
            return;
        }

        _healthSource.OnHealthChanged -= HandleHealthChanged;
        _healthSource = null;
    }

    public bool MatchesSelection(ISelectable selectable)
    {
        return selectable != null && ReferenceEquals(_selectable, selectable);
    }

    private void OnEnable()
    {
        Register();
    }

    private void OnDisable()
    {
        Unregister();
    }

    private void OnDestroy()
    {
        Release();
    }

    private void Register()
    {
        if (_isRegistered || _healthSource == null || !isActiveAndEnabled)
        {
            return;
        }

        _isRegistered = true;
        _registeredTargets.Add(this);
        OnRegistered?.Invoke(this);
    }

    private void Unregister()
    {
        if (!_isRegistered)
        {
            return;
        }

        _isRegistered = false;
        _registeredTargets.Remove(this);
        OnUnregistered?.Invoke(this);
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void CalculateLayout()
    {
        Collider targetCollider = FindTargetCollider();

        if (targetCollider != null)
        {
            TargetSize = CalculateColliderWidth(targetCollider);
            Vector3 colliderAnchor = CalculateColliderAnchor(targetCollider);
            _localAnchorPosition = transform.InverseTransformPoint(targetCollider.transform.TransformPoint(colliderAnchor));
            return;
        }

        Renderer targetRenderer = FindTargetRenderer();

        if (targetRenderer != null)
        {
            Bounds localBounds = targetRenderer.localBounds;
            Vector3 rendererAnchor = localBounds.center + Vector3.up * localBounds.extents.y;
            Vector3 scale = targetRenderer.transform.lossyScale;
            TargetSize = Mathf.Max(Mathf.Abs(localBounds.size.x * scale.x), Mathf.Abs(localBounds.size.z * scale.z));
            _localAnchorPosition = transform.InverseTransformPoint(targetRenderer.transform.TransformPoint(rendererAnchor));
            return;
        }

        TargetSize = 1f;
        _localAnchorPosition = Vector3.zero;
    }

    private Collider FindTargetCollider()
    {
        Collider targetCollider = GetComponent<Collider>();

        if (targetCollider != null && targetCollider.enabled && !targetCollider.isTrigger)
        {
            return targetCollider;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].enabled && !colliders[i].isTrigger)
            {
                return colliders[i];
            }
        }

        return null;
    }

    private Renderer FindTargetRenderer()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].enabled)
            {
                return renderers[i];
            }
        }

        return null;
    }

    private static Vector3 CalculateColliderAnchor(Collider collider)
    {
        if (collider is BoxCollider boxCollider)
        {
            return boxCollider.center + Vector3.up * boxCollider.size.y * 0.5f;
        }

        if (collider is SphereCollider sphereCollider)
        {
            return sphereCollider.center + Vector3.up * sphereCollider.radius;
        }

        if (collider is CapsuleCollider capsuleCollider)
        {
            float topOffset = capsuleCollider.direction == 1 ? capsuleCollider.height * 0.5f : capsuleCollider.radius;
            return capsuleCollider.center + Vector3.up * topOffset;
        }

        if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            Bounds localBounds = meshCollider.sharedMesh.bounds;
            return localBounds.center + Vector3.up * localBounds.extents.y;
        }

        return Vector3.zero;
    }

    private static float CalculateColliderWidth(Collider collider)
    {
        Vector3 scale = collider.transform.lossyScale;

        if (collider is BoxCollider boxCollider)
        {
            return Mathf.Max(Mathf.Abs(boxCollider.size.x * scale.x), Mathf.Abs(boxCollider.size.z * scale.z));
        }

        if (collider is SphereCollider sphereCollider)
        {
            float diameter = sphereCollider.radius * 2f;
            return diameter * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        }

        if (collider is CapsuleCollider capsuleCollider)
        {
            float diameter = capsuleCollider.radius * 2f;
            float xSize = capsuleCollider.direction == 0 ? capsuleCollider.height : diameter;
            float zSize = capsuleCollider.direction == 2 ? capsuleCollider.height : diameter;
            return Mathf.Max(Mathf.Abs(xSize * scale.x), Mathf.Abs(zSize * scale.z));
        }

        if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            Vector3 size = meshCollider.sharedMesh.bounds.size;
            return Mathf.Max(Mathf.Abs(size.x * scale.x), Mathf.Abs(size.z * scale.z));
        }

        return Mathf.Max(collider.bounds.size.x, collider.bounds.size.z);
    }
}
