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
    }

    public float SoundFXVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float AmbienceVolume { get; private set; }

    public void OnSoundFXValueChange(float value)
    {
        SoundFXVolume = value;
        OnSoundFXChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnMusicValueChange(float value)
    {
        MusicVolume = value;
        OnMusicChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void OnAmbienceVolumeChange(float value)
    {
        AmbienceVolume = value;
        OnAmbienceChanged?.Invoke(this, System.EventArgs.Empty);
    }
}
