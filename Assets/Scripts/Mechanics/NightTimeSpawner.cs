using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightTimeSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform mobContainer;
    private int monsterCount;
    private int maxMonsters = 50;

    private void Awake()
    {
        DayNightCycle.Instance.OnNight += StartSpawningMonsters;
        DayNightCycle.Instance.OnDawn += DespawnVampires;
    }

    private void StartSpawningMonsters(object sender, System.EventArgs e)
    {
        if (DayNightCycle.Instance.currentDay == 1)
        {
            Debug.Log("Not spawning on day 1");
            return;
        }
        if (DayNightCycle.Instance.currentDay == 2)
        {
            JournalNoteController.Instance.UnlockSpecificEntry("lyncher");
        }
        StartCoroutine(SpawnMonsters());
    }

    private void DespawnVampires(object sender, System.EventArgs e)
    {
        foreach(Transform obj in mobContainer)
        {
            if (obj.GetComponent<IsVampire>() != null && !obj.gameObject.activeSelf)
            {
                obj.GetComponent<RealMob>().Die(false);
                Debug.Log("Killing vampire");
            }
        }
    }

    private IEnumerator SpawnMonsters()//add new monster for blackmoon days. The shadow man? 
    {
        if (monsterCount >= maxMonsters)
        {
            yield break;
        }
        yield return new WaitForSeconds(15);

        if (!DayNightCycle.Instance.isNight)
        {
            yield break;
        }

        var newPos = CalebUtils.RandomPositionInRadius(player.position, 50, 400);
        var mob = RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("lyncher") });
        monsterCount++;
        StartCoroutine(SpawnMonsters());
    }
}
