using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOptions : MonoBehaviour
{
    public static SoundOptions Instance { get; private set; }

    public System.EventHandler OnSoundFXChanged;
    public System.EventHandler OnMusicChanged;
    public System.EventHandler OnAmbienceChanged;

    private void Awake()
    {
        Instance = this;
        if (Application.isEditor)
        {
            SoundFXVolume = .5f;//set in options later
            MusicVolume = .5f;
            AmbienceVolume = .5f;
        }
        else
        {
            SoundFXVolume = 1f;
            MusicVolume = 1f;
            AmbienceVolume = 1f;
        }
        LoadSettings();
    }

    public float SoundFXVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float AmbienceVolume { get; private set; }

    public void OnSoundFXValueChange(float value)
    {
        SoundFXVolume = value;
        SaveSoundFxSettings();
        OnSoundFXChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnMusicValueChange(float value)
    {
        MusicVolume = value;
        SaveMusicSettings();
        OnMusicChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnAmbienceVolumeChange(float value)
    {
        AmbienceVolume = value;
        SaveAmbienceSettings();
        OnAmbienceChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void SaveSoundFxSettings()
    {
        PlayerPrefs.SetFloat("SoundFXVolume", SoundFXVolume);
    }

    private void SaveMusicSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
    }

    private void SaveAmbienceSettings()
    {
        PlayerPrefs.SetFloat("AmbienceVolume", AmbienceVolume);
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("SoundFXVolume"))
        {
            SoundFXVolume = PlayerPrefs.GetFloat("SoundFXVolume");
        }
        
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        }

        if (PlayerPrefs.HasKey("AmbienceVolume"))
        {
            AmbienceVolume = PlayerPrefs.GetFloat("AmbienceVolume");
        }
    }
}
