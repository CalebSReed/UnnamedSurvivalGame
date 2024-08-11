using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    public static AmbienceManager Instance;

    private AudioSource currentAmbience;

    private void Start()
    {
        Instance = this;

        DayNightCycle.Instance.OnDawn += StartDayAmbience;
        DayNightCycle.Instance.OnNight += StartNightAmbience;
        SoundOptions.Instance.OnAmbienceChanged += OnAmbienceVolumeChanged;
    }

    private void StartDayAmbience(object sender, System.EventArgs e)
    {
        StartCoroutine(FadeInAmbience("DayAmbience"));
    }

    private void StartNightAmbience(object sender, System.EventArgs e)
    {
        StartCoroutine(FadeInAmbience("NightAmbience"));
    }

    private IEnumerator FadeInAmbience(string sound)
    {
        AudioSource tempAmbience = null;
        if (currentAmbience != null)
        {
            tempAmbience = currentAmbience;
        }

        currentAmbience = AudioManager.Instance.Play(sound, transform.position, gameObject, true, false, true);

        float volumeMult = 0f;
        float vol = currentAmbience.volume;

        float tempVolMult = 1f;
        float tempVol = 0f;
        if (tempAmbience != null)
        {
            tempVol = tempAmbience.volume;
        }

        while (volumeMult < 1)
        {
            if (tempAmbience != null)
            {
                tempAmbience.volume = tempVol * tempVolMult;
                tempVolMult -= .01f;
            }
            currentAmbience.volume = vol * volumeMult;
            volumeMult += .01f;
            yield return null;
        }

        if (tempAmbience != null)
        {
            Debug.Log("STOP!");
            tempAmbience.Stop();
        }
    }

    private void OnAmbienceVolumeChanged(object sender, System.EventArgs e)
    {
        AudioManager.Instance.ChangeAmbienceVolume("DayAmbience");
        AudioManager.Instance.ChangeAmbienceVolume("NightAmbience");
    }
}
