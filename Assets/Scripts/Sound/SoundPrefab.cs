using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPrefab : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    public string soundName;
    public Sound.SoundType soundType;
    public AudioClip clip;
    public float volMult;
    public bool loops;
    public int progress;
    public float goal;
    public bool follow;
    public GameObject source;
    private Coroutine countdown;

    public void StartTimer()
    {
        if (clip != null && !loops)
        {
            progress = 0;
            goal = clip.length;
            countdown = StartCoroutine(DisableOnSoundEnd());
        }
    }

    public void ResumeTimer()
    {
        if (countdown == null && !loops)
        {
            countdown = StartCoroutine(DisableOnSoundEnd());
        }
    }

    public void PauseTimer()
    {
        StopAllCoroutines();
        countdown = null;
    }

    private void Update()
    {
        if (follow)
        {
            if (source != null)
            {
                transform.position = source.transform.position;
            }
            else
            {
                //mute sound
            }            
        }
    }

    public IEnumerator DisableOnSoundEnd()
    {
        yield return new WaitForSeconds(1);
        progress++;
        if (progress > goal)
        {
            audioManager.DestroySound(gameObject);
            countdown = null;
            follow = false;
            soundName = "";
            yield break;
        }
        StartCoroutine(DisableOnSoundEnd());
    }
}
