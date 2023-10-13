using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOptions : MonoBehaviour
{
    public static SoundOptions Instance { get; set; }
    private void Awake()
    {
        Instance = this;
        Instance.SoundFXVolume = .5f;//set in options later
        Instance.MusicVolume = .5f;
        Instance.AmbienceVolume = .5f;
    }

    public float SoundFXVolume { get; set; }
    public float MusicVolume { get; set; }
    public float AmbienceVolume { get; set; }
}
