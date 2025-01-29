using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour
{
    public DayNightCycle dayCycle;
    public AudioManager audio;
    private AudioSource playingSong;
    bool battleMusicPlaying;
    AudioSource battleSong;
    Coroutine waitToEndBattle;
    Coroutine finalWaitToEnd;
    Coroutine loopCheck;
    public static MusicManager Instance;

    private void Start()
    {
        dayCycle.OnDawn += PlayRandomDaySong;
        dayCycle.OnDusk += PlayRandomDaySong;//lmfao i forgot i put this and started panicking that the saving and loading system for time was broken ;w;
        SoundOptions.Instance.OnMusicChanged += OnMusicVolumeChanged;

        Instance = this;
    }

    private void PlayRandomDaySong(object sender, System.EventArgs e)
    {
        int randVal = Random.Range(1, 5);
        playingSong = audio.Play($"Music{randVal}", transform.position, gameObject);
        if (battleMusicPlaying)
        {
            audio.Pause(playingSong);
        }
    }

    public void PlayBattleMusic()
    {
        if (battleMusicPlaying)
        {
            if (waitToEndBattle != null)
            {
                StopCoroutine(waitToEndBattle);
                waitToEndBattle = null;
            }
            CheckToEndBattleMusic();
            return;
        }
        if (playingSong != null && playingSong.isPlaying)
        {
            battleMusicPlaying = true;
            audio.Play("EnterCombat", transform.position, gameObject, true);
            StartCoroutine(FadeOut("Battle"));
            CheckToEndBattleMusic();
        }
        else
        {
            battleMusicPlaying = true;
            loopCheck = StartCoroutine(WaitToLoopSong(battleSong = audio.Play("Battle", transform.position, gameObject)));
            CheckToEndBattleMusic();
            audio.Play("EnterCombat", transform.position, gameObject, true);
        }
    }

    private void CheckToEndBattleMusic()
    {
        //Debug.Log("Waiting...");
        if (waitToEndBattle != null)
        {
            StopCoroutine(waitToEndBattle);
            waitToEndBattle = null;
        }
        waitToEndBattle = StartCoroutine(WaitToEndBattle());
    }

    private IEnumerator WaitToEndBattle()
    {
        yield return new WaitForSeconds(1);

        if (GameManager.Instance.localPlayerMain.enemyList.Count == 0 || GameManager.Instance.localPlayerMain.enemyList.Count > 0 && GameManager.Instance.localPlayerMain.enemyList[0] == null)
        {
            if (finalWaitToEnd != null)
            {
                StopCoroutine(finalWaitToEnd);
                finalWaitToEnd = null;
            }
                finalWaitToEnd = StartCoroutine(EndBattleMusic());
        }
        else
        {
            CheckToEndBattleMusic();
        }
    }

    private IEnumerator EndBattleMusic()
    {
        //Debug.Log("Ending....");
        yield return new WaitForSeconds(10f);
        if (GameManager.Instance.localPlayerMain.enemyList.Count == 0 || GameManager.Instance.localPlayerMain.enemyList.Count > 0 && GameManager.Instance.localPlayerMain.enemyList[0] == null)
        {
            //Debug.Log("Ended");
            StartCoroutine(FadeOutBattle());
        }
        else
        {
            waitToEndBattle = StartCoroutine(WaitToEndBattle());
        }
        finalWaitToEnd = null;
    }

    private IEnumerator FadeIn()
    {
        audio.UnPause(playingSong);
        Debug.Log($"Playing {playingSong.GetComponent<SoundPrefab>().soundName}");
        while (playingSong.volume < SoundOptions.Instance.MusicVolume)
        {
            playingSong.volume += .01f;
            yield return null;
        }
    }

    public void ForceEndMusic()
    {
        StopAllCoroutines();
        audio.Stop("Music1");
        audio.Stop("Music2");
        audio.Stop("Music3");
        audio.Stop("Music4");
        audio.Stop("Battle");
        audio.Stop("BattleLoop");
        battleMusicPlaying = false;
        /*if (battleMusicPlaying)
        {
            StartCoroutine(FadeOutBattle());
        }*/
    }

    private IEnumerator FadeOut(string newSong)
    {
        while (playingSong.volume > 0)
        {
            playingSong.volume -= .01f;
            yield return null;

        }
        audio.Pause(playingSong);
        loopCheck = StartCoroutine(WaitToLoopSong(battleSong = audio.Play(newSong, transform.position, gameObject)));
    }

    private IEnumerator FadeOutBattle()
    {
        Debug.Log("Ending");
        if (loopCheck != null)
        {
            StopCoroutine(loopCheck);
            loopCheck = null;
        }
        battleMusicPlaying = false;
        while (battleSong != null && battleSong.volume > 0)
        {
            battleSong.volume -= .01f;
            yield return null;
        }
        if (playingSong != null && playingSong.gameObject.activeSelf)
        {
            StartCoroutine(FadeIn());
        }
        battleSong.Stop();
        battleSong = null;
    }

    private IEnumerator WaitToLoopSong(AudioSource song)
    {
        yield return new WaitForSeconds(song.clip.length);
        if (battleMusicPlaying)
        {
            battleSong = audio.Play("BattleLoop", transform.position, gameObject);
        }
        loopCheck = null;
    }

    private void OnMusicVolumeChanged(object sender, EventArgs e)
    {
        audio.ChangeMusicVolume("Music1");
        audio.ChangeMusicVolume("Music2");
        audio.ChangeMusicVolume("Music3");
        audio.ChangeMusicVolume("Music4");
        audio.ChangeMusicVolume("Battle");
        audio.ChangeMusicVolume("BattleLoop");
        Debug.Log("changed volume");
    }
}
