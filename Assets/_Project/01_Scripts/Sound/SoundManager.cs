using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    private Dictionary<string, AudioClip> soundDict;
    [SerializeField]private AudioSource sfxPlayer;
    [SerializeField]private AudioSource bgmPlayer;

    [SerializeField] private AudioClip[] audioClips;

    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Init()
    {
        soundDict = new Dictionary<string, AudioClip>();
        bgmPlayer.loop = true; // BGM은 기본적으로 반복 재생

        // Dictionary 초기화
        foreach (var clip in audioClips)
        {
            soundDict[clip.name] = clip;
        }
    }
    public void PlaySFX(string soundName)
    {
        if(soundDict.TryGetValue(soundName,out var clip))
        {
            sfxPlayer.PlayOneShot(clip);
        }
        else
        {
            return;
        }
    }
    public void PlayBGM(string bgmName)
    {
        if (soundDict.TryGetValue(bgmName, out var clip))
        {
            if(bgmPlayer.clip!=clip)
            {
                bgmPlayer.clip = clip;
                bgmPlayer.Play();
            }
        }
        else
        {
            return;
        }
    }
    public void SetSFXVolume(float volume)
    {
        if (sfxPlayer != null)
        {
            sfxPlayer.volume = volume;
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmPlayer != null)
        {
            bgmPlayer.volume = volume;
        }
    }
}
