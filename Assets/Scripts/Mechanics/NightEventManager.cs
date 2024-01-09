using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NightEventManager : MonoBehaviour
{
    public static NightEventManager Instance { get; set; }

    public DayNightCycle dayCycle;
    public Transform player;
    public AudioManager audio;

    private void Start()
    {
        Instance = this;
        dayCycle.OnNight += StartNightEvent;
    }

    private void StartNightEvent(object sender, EventArgs e)
    {
        if (dayCycle.currentDay <= 5)//let the player have 5 days to prep for the worst outcome
        {
            return;
        }

        int randVal = Random.Range(1,10);

        if (randVal == 1)
        {
            StartCoroutine(SummonDepthWalkers());
        }
    }

    public IEnumerator SummonDepthWalkers(bool _forced = false)
    {
        if (_forced)
        {
            dayCycle.LoadNewTime(1111, 3, 3, 1, 0, 0, 0);//MUAHAHA
            var newPos = CalebUtils.RandomPositionInRadius(player.position, 50, 90);

            int randVal = Random.Range(1, 4);
            audio.Play($"DepthCall{randVal}", transform.position, gameObject);
            Debug.Log($"randval is {randVal}", gameObject);
            yield return new WaitForSeconds(1);
            RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("DepthWalker") });
            yield break;
        }
        int _randTime = Random.Range(120, 240);
        yield return new WaitForSeconds(_randTime);
        if (!dayCycle.isNight && !_forced)
        {
            Debug.LogError("not doin it...");
            yield return null;
        }
        else
        {
            Debug.Log("here i go summonin again!");
            var newPos = CalebUtils.RandomPositionInRadius(player.position, 50, 90);

            int randVal = Random.Range(1, 4);
            audio.Play($"DepthCall{randVal}", transform.position, gameObject);
            Debug.Log($"randval is {randVal}", gameObject);
            yield return new WaitForSeconds(1);
            RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("DepthWalker") });

            StartCoroutine(SummonDepthWalkers());
        }
    }
}
