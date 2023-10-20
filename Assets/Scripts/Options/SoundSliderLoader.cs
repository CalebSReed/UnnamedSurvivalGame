using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSliderLoader : MonoBehaviour
{
    [SerializeField] private Slider soundFXSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider ambienceSlider;
    void Start()
    {
        LoadSoundSettings();
    }

    private void LoadSoundSettings()
    {
        soundFXSlider.value = SoundOptions.Instance.SoundFXVolume;
        musicSlider.value = SoundOptions.Instance.MusicVolume;
        ambienceSlider.value = SoundOptions.Instance.AmbienceVolume;
    }
}
