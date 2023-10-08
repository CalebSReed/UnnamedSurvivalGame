using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BunnyHole : MonoBehaviour
{
    private DayNightCycle dayCycle;

    public int bunnyCount = 1;
    public int bunnyProgress;
    public int bunnyGoal = 4320;

    private void Start()
    {
        dayCycle = GameObject.FindGameObjectWithTag("DayCycle").GetComponent<DayNightCycle>();
        StartCoroutine(BunnyTimer());
        
    }

    private void ReleaseBunny()
    {
        if (bunnyCount > 0 && dayCycle.isDay)
        {
            var bunny = RealMob.SpawnMob(transform.position, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Bunny") });
            bunny.SetHome(GetComponent<RealWorldObject>());
            DayNightCycle.Instance.OnDusk += bunny.GoHome;
            bunnyCount--;
        }
        else if (bunnyCount <= 0)
        {
            StartCoroutine(RepopulateBunnies());
        }
    }

    private IEnumerator RepopulateBunnies()
    {
        yield return new WaitForSeconds(1);
        bunnyProgress++;
        if (bunnyProgress >= bunnyGoal)
        {
            bunnyCount++;
        }
    }

    private void StartBunnyTimer(object sender, EventArgs e)
    {
        StartCoroutine(BunnyTimer());
    }

    private IEnumerator BunnyTimer()
    {
        int _rand = UnityEngine.Random.Range(10, 15);
        yield return new WaitForSeconds(_rand);
        //Debug.LogError("bunny release");
        ReleaseBunny();
    }

    private void OnDisable()
    {
        dayCycle.OnDay -= StartBunnyTimer;
    }

    private void OnEnable()
    {
        dayCycle.OnDay += StartBunnyTimer;
    }
}
