using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MobHomeAI : MonoBehaviour
{
    private MobMovementBase mobMovement;

    private void Awake()
    {
        mobMovement = GetComponent<MobMovementBase>();
    }

    public void GoHome()
    {
        mobMovement.goHome = true;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveTowards);
    }

    private void OnEnable()
    {
        if (DayNightCycle.Instance.isDusk || DayNightCycle.Instance.isNight)
        {
            GoHome();
        }
    }
}
