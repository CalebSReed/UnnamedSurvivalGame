using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BunnyHole : MonoBehaviour
{
    public int bunnyCount = 1;
    public int bunnyProgress;
    public int bunnyGoal = DayNightCycle.fullDayTimeLength * 3;
    RealWorldObject obj;

    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();

        obj.onLoaded += OnLoad;
        obj.onSaved += OnSave;
    }

    private void Start()
    {
        StartCoroutine(BunnyTimer());  
    }

    private void ReleaseBunny()
    {
        if (bunnyCount > 0 && DayNightCycle.Instance.isDay)
        {
            var bunny = RealMob.SpawnMob(transform.position, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Bunny") });
            bunny.SetHome(GetComponent<RealWorldObject>());
            DayNightCycle.Instance.OnDusk += bunny.GoHome;
            bunny.gameObject.AddComponent<MobHomeAI>();
            bunnyCount--;
        }
        else if (bunnyCount <= 0)
        {
            StartCoroutine(RepopulateBunnies());
        }
    }

    private IEnumerator RepopulateBunnies()
    {
        bunnyProgress++;
        yield return new WaitForSeconds(1);
        if (bunnyProgress >= bunnyGoal)
        {
            bunnyCount++;
            bunnyProgress = 0;
        }
        else
        {
            StartCoroutine(RepopulateBunnies());
        }
    }

    private void StartBunnyTimer(object sender, EventArgs e)
    {
        StartCoroutine(BunnyTimer());
    }

    private IEnumerator BunnyTimer()
    {
        int _rand = UnityEngine.Random.Range(10, 31);
        yield return new WaitForSeconds(_rand);
        //Debug.LogError("bunny release");
        ReleaseBunny();
    }

    private void OnDisable()
    {
        DayNightCycle.Instance.OnDay -= StartBunnyTimer;
    }

    private void OnEnable()
    {
        DayNightCycle.Instance.OnDay += StartBunnyTimer;
    }

    private void OnSave(object sender, System.EventArgs e)
    {
        obj.saveData.timerProgress = bunnyProgress;
        obj.saveData.currentInhabitants = bunnyCount;
    }

    private void OnLoad(object sender, System.EventArgs e)
    {
        bunnyProgress = obj.saveData.timerProgress;
        bunnyCount = obj.saveData.currentInhabitants;

        if (bunnyProgress > 0)
        {
            StartCoroutine(RepopulateBunnies());
        }
    }
}
