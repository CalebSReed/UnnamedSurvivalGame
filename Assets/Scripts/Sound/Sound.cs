using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Audio;

[System.Serializable]
public class Sound
{
    public enum SoundType
    {
        Default,
        Music,
        Ambience,       
    }

    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(.1f, 3f)]
    public float pitch;

    [HideInInspector]
    public AudioSource source;

    public bool loop;
}
