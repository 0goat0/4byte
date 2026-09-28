using UnityEngine;
using UnityEngine.UI;

public class SliderBridge : MonoBehaviour
{
    [SerializeField] private bool isBGM;

    private void Start()
    {
        Slider slider = GetComponent<Slider>();
        if (slider == null) return;

        slider.onValueChanged.RemoveAllListeners();

        slider.onValueChanged.AddListener((value) =>
        {
            if (SoundManager.instance != null)
            {
                if (isBGM)
                    SoundManager.instance.SetBGMVolume(value);
                else
                    SoundManager.instance.SetSFXVolume(value);
            }
        });
    }
}