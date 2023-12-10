using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MobHomeAI : MonoBehaviour
{
    private MobMovementBase mobMovement;

    private void Start()
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
        if (DayNightCycle.Instance.isDusk)
        {
            GoHome();
        }
    }
}
