using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        audio.Play("Music1");
    }
}
