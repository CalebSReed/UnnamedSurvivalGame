using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MobNeutralAI : MonoBehaviour//aggressive neutral, attack when attacked
{
    private MobMovementBase mobMovement;

    private CombatArgs combatArgs = new CombatArgs();

    public event EventHandler<CombatArgs> OnAggroed;

    private PlayerMain player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<HealthManager>().OnDamageTaken += InitiateCombat;
    }

    private void InitiateCombat(object sender, DamageArgs e)//will attack anything that isnt predator ig
    {
        if (e.damageSenderTag == "fire" || e.damageSenderTag == "Projectile")
        {
            return;
        }

        var _list = GetComponent<RealMob>().mob.mobSO.predators;
        if (!GetComponent<RealMob>().mob.mobSO.isScouter)//scouters ignore predators on hit, they go crazy
        {
            foreach (string _predator in _list)//if is predator, dont attack back
            {
                if (e.damageSenderTag == _predator)
                {
                    mobMovement.target = e.senderObject;//need this lol. this is predator target so we runaway
                    mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveAway);
                    return;
                }
            }
        }

        combatArgs.combatTarget = e.senderObject;//this is the default 
        mobMovement.target = e.senderObject;//new change hope it dont break everything
        OnAggroed?.Invoke(this, combatArgs);
        if (e.senderObject == player.gameObject)
        {
            if (!isInEnemyList())
            {
                player.enemyList.Add(gameObject);
            }
            MusicManager.Instance.PlayBattleMusic();
        }
        if (GetComponent<RealMob>().mob.mobSO.isScouter)//scouters have their own chase ai i guess idk
        {
            return;
        }

        if (mobMovement.currentMovement != MobMovementBase.MovementOption.DoNothing)
        {
            mobMovement.SwitchMovement(mobMovement.realMob.mob.mobSO.aggroStrategy);
        }
    }

    public bool isInEnemyList()
    {
        foreach (GameObject obj in player.enemyList)
        {
            if (obj == gameObject)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDestroy()
    {
        foreach (GameObject obj in player.enemyList)
        {
            if (obj == gameObject)
            {
                player.enemyList.Remove(obj);
                return;
            }
        }
    }
}
