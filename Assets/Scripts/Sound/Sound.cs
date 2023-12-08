using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Audio;

[System.Serializable]
public class Sound
{
    public enum SoundType
    {
        SoundEffect,
        Music,
        Ambience,       
    }

    public enum SoundMode
    {
        TwoDimensional,
        ThreeDimensional
    }

    public string name;

    public AudioClip clip;

    public SoundType soundType;

    [System.NonSerialized] public SoundMode soundMode;

    [Range(0f, 1f)]
    private float volume;

    [Range(.1f, 3f)]
    private float pitch;

    public bool loop;
    //public bool overrideTwoDimensional;
}
