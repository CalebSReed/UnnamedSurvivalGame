using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IsVampire : MonoBehaviour
{
    private HealthManager hp;

    private void Start()
    {
        hp = GetComponent<HealthManager>();

        if (!DayNightCycle.Instance.isNight)
        {
            GetComponent<RealMob>().Die(false, false);
        }
    }

    private void BurnToDeath(object sender, EventArgs e)
    {
        if (!GameManager.Instance.isServer)
        {
            return;
        }
        DayNightCycle.Instance.OnDawn -= BurnToDeath;
        //gameObject.GetComponent<RealMob>().Die(false);
        StartCoroutine(Burn());
    }

    private IEnumerator Burn()
    {
        if (hp == null || gameObject == null)
        {
            yield break;
        }
        if (hp.currentHealth <= 25)
        {
            gameObject.GetComponent<RealMob>().Die(false);
        }
        hp.TakeDamage(25, "fire", gameObject);
        yield return new WaitForSeconds(1);
        StartCoroutine(Burn());
    }

    private void OnDisable()
    {
        DayNightCycle.Instance.OnDawn -= BurnToDeath;
    }

    private void OnEnable()
    {
        if (!DayNightCycle.Instance.isNight)
        {
            GetComponent<RealMob>().Die(false, false);
            return;
        }

        DayNightCycle.Instance.OnDawn += BurnToDeath;

        if (!DayNightCycle.Instance.isNight)
        {
            BurnToDeath(this, EventArgs.Empty);
        }
    }
}
