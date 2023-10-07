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
        dayCycle.OnDawn += BurnToDeath;

        if (dayCycle.isDawn)
        {
            BurnToDeath(this, EventArgs.Empty);
        }
    }

    private void BurnToDeath(object sender, EventArgs e)
    {
        dayCycle.OnDawn -= BurnToDeath;
        gameObject.GetComponent<RealMob>().Die(false);
    }
}
