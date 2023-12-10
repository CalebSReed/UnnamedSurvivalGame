using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightTimeSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform mobContainer;

    private void Awake()
    {
        DayNightCycle.Instance.OnNight += StartSpawningMonsters;
        DayNightCycle.Instance.OnDawn += DespawnVampires;
    }

    private void StartSpawningMonsters(object sender, System.EventArgs e)
    {
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

    private IEnumerator SpawnMonsters()
    {
        yield return new WaitForSeconds(5);

        if (!DayNightCycle.Instance.isNight)
        {
            yield break;
        }

        var newPos = CalebUtils.RandomPositionInRadius(player.position, 50, 400);
        var mob = RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Night Lyncher") });
        StartCoroutine(SpawnMonsters());
    }
}
