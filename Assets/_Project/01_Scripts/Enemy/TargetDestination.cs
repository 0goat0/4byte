using UnityEngine;

public class TargetDestination : MonoBehaviour
{
    public static TargetDestination Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
