using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NightEventManager : MonoBehaviour
{
    public DayNightCycle dayCycle;
    public Transform player;
    public AudioManager audio;

    private void Start()
    {
        dayCycle.OnNight += StartNightEvent;
    }

    private void StartNightEvent(object sender, EventArgs e)
    {
        int randVal = Random.Range(1,10);

        if (randVal == 1)
        {
            StartCoroutine(SummonDepthWalkers());
        }
    }

    public IEnumerator SummonDepthWalkers(bool _forced = false)
    {
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
            Vector3 newPos = player.position;
            int isPositive = Random.Range(0, 2);

            if (isPositive == 1)
            {
                newPos.x += Random.Range(50, 250);
            }
            else
            {
                newPos.x += Random.Range(-50, -250);
            }
            isPositive = Random.Range(0, 2);

            if (isPositive == 1)
            {
                newPos.y += Random.Range(50, 250);
            }
            else
            {
                newPos.y += Random.Range(-50, -250);
            }

            int randVal = Random.Range(1, 4);
            audio.Play($"DepthCall{randVal}", transform.position, gameObject);
            Debug.Log($"randval is {randVal}", gameObject);
            RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("DepthWalker") });

            StartCoroutine(SummonDepthWalkers());
        }
    }
}
