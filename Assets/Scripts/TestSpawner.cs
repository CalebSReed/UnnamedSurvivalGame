using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestSpawner : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(SpawnParasites());
    }

    private IEnumerator SpawnParasites()
    {
        RealMob.SpawnMob(transform.position, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });
        yield return new WaitForSeconds(60);
        StartCoroutine(SpawnParasites());
    }

    private void SpawnWave(object sender, EventArgs e)
    {

    }
}
