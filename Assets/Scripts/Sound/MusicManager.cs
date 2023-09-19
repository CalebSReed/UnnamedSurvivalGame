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
        dayCycle.OnDawn += PlayMorningSong;
    }

    private void PlayMorningSong(object sender, System.EventArgs e)
    {
        int randVal = Random.Range(1, 3);
        audio.Play($"Music{randVal}", gameObject, true);
    }
}
