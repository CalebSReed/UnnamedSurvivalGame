using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour
{
    public DayNightCycle dayCycle;
    public AudioManager audio;

    private void Start()
    {
        dayCycle.OnDawn += PlayRandomDaySong;
        dayCycle.OnDusk += PlayRandomDaySong;//lmfao i forgot i put this and started panicking that the saving and loading system for time was broken ;w;
        SoundOptions.Instance.OnMusicChanged += OnMusicVolumeChanged;
    }

    private void PlayRandomDaySong(object sender, System.EventArgs e)
    {
        int randVal = Random.Range(1, 4);
        audio.Play($"Music{randVal}", gameObject, Sound.SoundType.Music, Sound.SoundMode.TwoDimensional);
    }

    private void OnMusicVolumeChanged(object sender, EventArgs e)
    {
        audio.ChangeMusicVolume("Music1");
        audio.ChangeMusicVolume("Music2");
        audio.ChangeMusicVolume("Music3");
        Debug.Log("changed volume");
    }
}
