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
    public PlayerMain player;
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
        int randVal = Random.Range(1, 4);
        playingSong = audio.Play($"Music{randVal}", gameObject, Sound.SoundType.Music, Sound.SoundMode.TwoDimensional);
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
            }
            CheckToEndBattleMusic();
            return;
        }
        if (playingSong != null &&  playingSong.isPlaying)
        {
            StartCoroutine(FadeOut("Battle"));
            CheckToEndBattleMusic();
        }
        else
        {
            loopCheck = StartCoroutine(WaitToLoopSong(battleSong = audio.Play("Battle", gameObject, Sound.SoundType.Music, Sound.SoundMode.TwoDimensional)));
            CheckToEndBattleMusic();
            battleMusicPlaying = true;
        }
    }

    private void CheckToEndBattleMusic()
    {
        //Debug.Log("Waiting...");
        if (waitToEndBattle != null)
        {
            StopCoroutine(WaitToEndBattle());
        }
        waitToEndBattle = StartCoroutine(WaitToEndBattle());
    }

    private IEnumerator WaitToEndBattle()
    {
        yield return new WaitForSeconds(1);

        if (player.enemyList.Count == 0 || player.enemyList.Count > 0 && player.enemyList[0] == null)
        {
            if (finalWaitToEnd != null)
            {
                StopCoroutine(finalWaitToEnd);
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
        if (player.enemyList.Count == 0 || player.enemyList.Count > 0 && player.enemyList[0] == null)
        {
            //Debug.Log("Ended");
            StartCoroutine(FadeOutBattle());
        }
        else
        {
            waitToEndBattle = StartCoroutine(WaitToEndBattle());
        }
        
    }

    private IEnumerator FadeIn()
    {
        audio.UnPause(playingSong);
        while (playingSong.volume < SoundOptions.Instance.MusicVolume)
        {
            playingSong.volume += .01f;
            yield return null;
        }
    }

    public void ForceEndMusic()
    {
        StopAllCoroutines();

        if (battleMusicPlaying)
        {
            StartCoroutine(FadeOutBattle());
        }
    }

    private IEnumerator FadeOut(string newSong)
    {
        while (playingSong.volume > 0)
        {
            playingSong.volume -= .01f;
            yield return null;

        }
        audio.Pause(playingSong);
        loopCheck = StartCoroutine(WaitToLoopSong(battleSong = audio.Play(newSong, gameObject, Sound.SoundType.Music, Sound.SoundMode.TwoDimensional)));
        battleMusicPlaying = true;
    }

    private IEnumerator FadeOutBattle()
    {
        Debug.Log("Ending");
        if (loopCheck != null)
        {
            StopCoroutine(loopCheck);
        }
        battleMusicPlaying = false;
        while (battleSong.volume > 0)
        {
            battleSong.volume -= .01f;
            yield return null;
        }
        if (playingSong != null)
        {
            StartCoroutine(FadeIn());
        }
        battleSong = null;

    }

    private IEnumerator WaitToLoopSong(AudioSource song)
    {
        yield return new WaitForSeconds(song.clip.length);
        battleSong = audio.Play("BattleLoop", gameObject, Sound.SoundType.Music, Sound.SoundMode.TwoDimensional);
    }

    private void OnMusicVolumeChanged(object sender, EventArgs e)
    {
        audio.ChangeMusicVolume("Music1");
        audio.ChangeMusicVolume("Music2");
        audio.ChangeMusicVolume("Music3");
        audio.ChangeMusicVolume("Battle");
        audio.ChangeMusicVolume("BattleLoop");
        Debug.Log("changed volume");
    }
}
