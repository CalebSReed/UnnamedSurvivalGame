using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class MobAggroAI : MonoBehaviour//we should decide whether or not if this mob wants to prioritize attacking? or fleeing? maybe fleeing by default? seems pretty balanced to me
{
    private MobMovementBase mobMovement;

    private RealMob realMob;
    public float preyDetectionRadius { get; set; }
    public float abandonRadius { get; set; }
    public float combatRadius { get; set; }
    public List<string> preyList { get; set; }

    public CombatArgs combatArgs = new CombatArgs();

    public event EventHandler<CombatArgs> StartCombat;
    public bool inCombat { get; set; }

    private void Awake()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();

        preyDetectionRadius = realMob.mob.mobSO.preyDetectionRadius;
        abandonRadius = realMob.mob.mobSO.abandonRadius;
        combatRadius = realMob.mob.mobSO.combatRadius;

        preyList = realMob.mob.mobSO.prey;
    }

    private void Update()
    {
        if (mobMovement.currentMovement == MobMovementBase.MovementOption.Chase)
        {
            CheckToInitiateCombat();
            CheckToAbandonPrey();
        }
        else if (mobMovement.currentMovement != MobMovementBase.MovementOption.MoveAway && mobMovement.currentMovement != MobMovementBase.MovementOption.DoNothing)//if we are fleeing, dont bother looking for prey. I think its safe to assume all mobs should think like this
        {
            FindPrey();
        }
    }

    private void CheckToInitiateCombat()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, combatRadius);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.GetComponent<RealMob>() != null)
            {
                var _realMob = _target.GetComponent<RealMob>();
                foreach (string _tag in preyList)
                {
                    if (_realMob.mob.mobSO.mobType == _tag)//if mobType = _tag in prey list or if is player
                    {
                        StartCombat?.Invoke(this, combatArgs);//will run the combat ai of this specific creature
                    }
                }
            }
            else if (_target.GetComponent<PlayerMain>() != null)
            {
                foreach (string _tag in preyList)
                {
                    if (_tag == "Player")//if is player
                    {
                        StartCombat?.Invoke(this, combatArgs);//will run the combat ai of this specific creature
                    }
                }
            }
            else if (_target.GetComponent<RealWorldObject>() != null && _target.GetComponent<RealWorldObject>().obj.woso.isPlayerMade)
            {
                foreach (string _tag in preyList)
                {
                    if (_tag == "PlayerMade")//if is playermade object
                    {
                        StartCombat?.Invoke(this, combatArgs);//will run the combat ai of this specific creature
                    }
                }
            }
        }
    }

    private void FindPrey()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, preyDetectionRadius);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.GetComponent<RealMob>() != null)
            {
                var _realMob = _target.GetComponent<RealMob>();
                foreach (string _tag in preyList)
                {
                    if (_realMob.mob.mobSO.mobType == _tag)//if mobType = _tag in prey list
                    {
                        mobMovement.target = _target.gameObject;
                        combatArgs.combatTarget = _target.gameObject;
                        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
                        return;
                    }
                }
            }
            else if (_target.GetComponent<PlayerMain>() != null)//if is a player
            {
                foreach (string _tag in preyList)
                {
                    if (_target.GetComponent<PlayerMain>() != null && _tag == "Player")
                    {
                        mobMovement.target = _target.gameObject;
                        combatArgs.combatTarget = _target.gameObject;
                        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
                        return;
                    }
                }
            }
            else if (_target.GetComponent<RealWorldObject>() != null && _target.GetComponent<RealWorldObject>().obj.woso.isPlayerMade)//if is a playermade object
            {
                foreach (string _tag in preyList)
                {
                    if (_tag == "PlayerMade")
                    {
                        mobMovement.target = _target.gameObject;
                        combatArgs.combatTarget = _target.gameObject;
                        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
                        return;
                    }
                }
            }
        }
    }

    private void CheckToAbandonPrey()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, abandonRadius);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.GetComponent<RealMob>() != null)
            {
                var _realMob = _target.GetComponent<RealMob>();
                foreach (string _tag in preyList)
                {
                    if (_realMob.mob.mobSO.mobType == _tag)//if mobType = _tag in predator list
                    {

                        //mobMovement.SwitchMovement(MobMovementBase.MovementOption.Aggro);
                        return;
                    }
                }
            }
            else if (_target.GetComponent<PlayerMain>() != null)//if is a real mob or player
            {
                foreach (string _tag in preyList)
                {
                    if (_target.GetComponent<PlayerMain>() != null && _tag == "Player")//if is player
                    {
                            
                        //mobMovement.SwitchMovement(MobMovementBase.MovementOption.Aggro);
                        return;
                    }
                }
            }
        }
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Wait);
    }       
}
