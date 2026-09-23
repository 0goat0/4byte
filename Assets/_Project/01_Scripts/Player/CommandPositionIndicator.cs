using System.Collections;
using UnityEngine;

public class CommandPositionIndicator : MonoBehaviour
{
    [SerializeField] private float _visibleDuration = 0.4f;

    [SerializeField] private float _surfaceOffset = 0.03f;

    private Coroutine _hideCoroutine;
    private WaitForSeconds _hideDelay;

    private void Awake()
    {
        _hideDelay = new WaitForSeconds(_visibleDuration);
        gameObject.SetActive(false);
    }

    public void Show(Vector3 position)
    {
        transform.SetPositionAndRotation(position + Vector3.up * _surfaceOffset, 
            Quaternion.identity);

        gameObject.SetActive(true);

        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }

        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return _hideDelay;

        _hideCoroutine = null;
        gameObject.SetActive(false);
    }
}
