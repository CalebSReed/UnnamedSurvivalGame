using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyHole : MonoBehaviour
{
    private DayNightCycle dayCycle;

    private int bunnyCount = 1;

    private void Start()
    {
        dayCycle = GameObject.FindGameObjectWithTag("DayCycle").GetComponent<DayNightCycle>();
        BunnyTimer();
    }

    private void ReleaseBunny()
    {
        if (bunnyCount > 0 && dayCycle.isDay)
        {
            var bunny = RealMob.SpawnMob(transform.position, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Bunny") });
            bunny.SetHome(GetComponent<RealWorldObject>());
            bunnyCount--;
        }
        else if (!dayCycle.isDay)
        {
            StopAllCoroutines();
        }
    }

    private IEnumerator BunnyTimer()
    {
        int _rand = Random.Range(60, 180);
        yield return new WaitForSeconds(_rand);
        ReleaseBunny();
    }
}
