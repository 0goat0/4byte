using UnityEngine;

public class BGMStarter : MonoBehaviour
{
    [SerializeField] private string bgmName;

    private void Start()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayBGM(bgmName);
        }
        else
        {
            return;
        }
    }
}