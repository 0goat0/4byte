using UnityEngine;

public class FogRevealer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.RegisterVisionSource(transform);
        }
    }

    private void OnEnable()
    {
        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.RegisterVisionSource(transform);
        }
    }

    private void OnDisable()
    {
        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.UnregisterVisionSource(transform);
        }
    }


}
