using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IsVampire : MonoBehaviour
{
    private DayNightCycle dayCycle;

    private void Start()
    {
        dayCycle = GameObject.FindGameObjectWithTag("DayCycle").GetComponent<DayNightCycle>();
        dayCycle.OnDay += BurnToDeath;
    }

    private void BurnToDeath(object sender, EventArgs e)
    {
        gameObject.GetComponent<RealMob>().Die();
    }
}
