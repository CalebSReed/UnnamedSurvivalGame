using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MobNeutralAI : MonoBehaviour//aggressive neutral, attack when attacked
{
    private MobMovementBase mobMovement;

    private CombatArgs combatArgs = new CombatArgs();

    public event EventHandler<CombatArgs> OnAggroed;

    private void Awake()
    {
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<HealthManager>().OnDamageTaken += InitiateCombat;
    }

    private void InitiateCombat(object sender, DamageArgs e)//will attack anything that isnt predator ig
    {
        var _list = GetComponent<RealMob>().mob.mobSO.predators;
        foreach (string _predator in _list)//if is predator, dont attack back
        {
            if (e.damageSenderTag == _predator)
            {
                mobMovement.target = e.senderObject;
                mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveAway);
                return;
            }
        }
        combatArgs.combatTarget = e.senderObject;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        OnAggroed?.Invoke(this, combatArgs);
    }
}
