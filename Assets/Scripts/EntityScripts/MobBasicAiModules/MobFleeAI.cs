using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MobFleeAI : MonoBehaviour
{
    private MobMovementBase mobMovement;

    private RealMob realMob;
    public float escapeRadius { get; set; }
    public float predatorDetectionRadius { get; set; }
    public List<string> predatorList { get; set; }

    private void Awake()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        predatorList = realMob.mob.mobSO.predators;

        escapeRadius = realMob.mob.mobSO.escapeRadius;
        predatorDetectionRadius = realMob.mob.mobSO.predatorDetectionRadius;

        GetComponent<HealthManager>().OnDamageTaken += OnHit;
    }

    private void Update()
    {
        if (mobMovement.currentMovement != MobMovementBase.MovementOption.MoveAway && predatorList.Count > 0 && !mobMovement.ignoreFleeingOverride)
        {
            CheckToFlee();
        }
        else if (!mobMovement.ignoreFleeingOverride)
        {
            CheckToStopFleeing();
        }
    }

    private void OnHit(object sender, DamageArgs e)
    {
        if (realMob.mob.mobSO.isScouter)
        {
            return;
        }
        print("gemme outta here");
        mobMovement.target = e.senderObject;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveAway);
    }

    private void CheckToFlee()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, predatorDetectionRadius);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.GetComponent<RealMob>() != null)
            {
                var _realMob = _target.GetComponent<RealMob>();
                foreach (string _tag in predatorList)
                {
                    if (_realMob.mob.mobSO.mobType == _tag)//if mobType = _tag in predator list
                    {
                        mobMovement.target = _target.gameObject;
                        mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveAway);
                        return;
                    }
                }
            }
            else if (_target.GetComponent<PlayerMain>() != null)//if is a real mob or player
            {
                foreach (string _tag in predatorList)
                {
                    if (_target.GetComponent<PlayerMain>() != null && _tag == "Player")//if is player
                    {
                        mobMovement.target = _target.gameObject;
                        mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveAway);
                        return;
                    }
                }
            }
        }
    }

    private void CheckToStopFleeing()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, escapeRadius);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.GetComponent<RealMob>() != null)
            {
                var _realMob = _target.GetComponent<RealMob>();
                foreach (string _tag in predatorList)
                {
                    if (_realMob.mob.mobSO.mobType == _tag)//if mobType = _tag in predator list
                    {
                        return;
                    }
                }
            }
            else if (_target.GetComponent<PlayerMain>() != null)//if is a real mob or player
            {
                foreach (string _tag in predatorList)
                {
                    if (_target.GetComponent<PlayerMain>() != null && _tag == "Player")//if is player
                    {
                        return;
                    }
                }
            }
        }

        if (Vector3.Distance(transform.position, mobMovement.target.transform.position) < escapeRadius)// OR if the target is closer than our radius!(this is if we entered flee mode from passive neutral script)
        {
            return;//maybe this should be the default?? why should we do everything else when this should work for every scenario
        }
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Wait);
    }
}
