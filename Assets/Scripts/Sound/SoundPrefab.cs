﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPrefab : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    public string soundName;
    public Sound.SoundType soundType;
    public AudioClip clip;
    public bool loops;

    public void StartTimer()
    {
        if (clip != null && !loops)
        {
            StartCoroutine(DisableOnSoundEnd());
        }
    }

    private IEnumerator DisableOnSoundEnd()
    {
        yield return new WaitForSeconds(clip.length+.5f);
        audioManager.DestroySound(gameObject);
    }
}