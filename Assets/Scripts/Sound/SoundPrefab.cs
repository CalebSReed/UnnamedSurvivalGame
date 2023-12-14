using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPrefab : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    public string soundName;
    public Sound.SoundType soundType;
    public AudioClip clip;
    public bool loops;
    public int progress;
    public float goal;

    public void StartTimer()
    {
        if (clip != null && !loops)
        {
            progress = 0;
            goal = clip.length;
            StartCoroutine(DisableOnSoundEnd());
        }
    }

    public IEnumerator DisableOnSoundEnd()
    {
        yield return new WaitForSeconds(1);
        if (progress > goal)
        {
            audioManager.DestroySound(gameObject);
        }
        StartCoroutine(DisableOnSoundEnd());
    }
}
