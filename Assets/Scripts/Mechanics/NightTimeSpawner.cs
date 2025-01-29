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
        GameManager.Instance.OnLocalPlayerSpawned += SetPlayer;
    }

    private void SetPlayer(object sender, System.EventArgs e)
    {
        player = GameManager.Instance.localPlayer.transform;
    }

    private void StartSpawningMonsters(object sender, System.EventArgs e)
    {
        if (DayNightCycle.Instance.currentDay == 1 || !GameManager.Instance.isServer)
        {
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
        if (!GameManager.Instance.isServer)
        {
            return;
        }
        monsterCount = 0;
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
        if (monsterCount >= maxMonsters * GameManager.Instance.playerList.Count)
        {
            yield break;
        }
        yield return new WaitForSeconds(15);

        if (!DayNightCycle.Instance.isNight)
        {
            yield break;
        }

        foreach (var player in GameManager.Instance.playerList)
        {
            var newPos = CalebUtils.RandomPositionInRadius(player.transform.position, 50, 400);
            newPos.y = 0;
            var mob = RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("lyncher") });
            monsterCount++;
            if (DayNightCycle.Instance.currentDay >= 5)
            {
                RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("lyncher") });
                monsterCount++;
            }
        }
        StartCoroutine(SpawnMonsters());
    }
}
