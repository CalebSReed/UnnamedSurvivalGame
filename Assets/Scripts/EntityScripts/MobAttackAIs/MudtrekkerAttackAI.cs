﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MudtrekkerAttackAI : MonoBehaviour, IAttackAI
{
    public MobMovementBase mobMovement { get; set; }
    public Animator anim { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public RealMob realMob { get; set; }

    private bool enableCollision;

    public void StartCombat(object sender, CombatArgs e)
    {
        target = e.combatTarget;
        int _randVal = Random.Range(0, 2);
        if (_randVal == 0)
        {
            anim.Play("Melee");
        }
        else
        {
            anim.Play("DashingPummel");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Special);
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;

        //GetComponent<MobNeutralAI>().OnAggroed += StartCombat;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
        realMob.hpManager.OnDamageTaken += OnDamageTaken;
    }

    private void OnDamageTaken(object sender, DamageArgs args)
    {
        if (args.dmgType == DamageType.Heavy)
        {
            anim.Play("Stunned");
        }
        //realMob.willStun = !realMob.willStun;
    }

    private bool TriggerHitSphere(float radius)
    {
        if (mobMovement.target == null)
        {
            return false;
        }
        Vector3 _newPos = transform.position;
        _newPos.y += 5;
        Collider[] _hitEnemies = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider _enemy in _hitEnemies)
        {
            if (!_enemy.isTrigger)
            {
                continue;
            }
            else if (_enemy.GetComponentInParent<PlayerMain>() != null)
            {
                if (_enemy.GetComponentInParent<PlayerMain>().godMode)
                {
                    GetComponent<HealthManager>().TakeDamage(999999, "Player", _enemy.gameObject);
                    return true;
                }
            }
            if (CalebUtils.GetParentOfTriggerCollider(_enemy) == mobMovement.target)
            {
                _enemy.GetComponentInParent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enableCollision || mobMovement.target == null)
        {
            return;
        }
        if (other.GetComponentInParent<PlayerMain>() != null)
        {
            if (other.GetComponentInParent<PlayerMain>().godMode)
            {
                GetComponent<HealthManager>().TakeDamage(999999, "Player", other.gameObject);
                return;
            }
        }

        if (CalebUtils.GetParentOfTriggerCollider(other) == mobMovement.target)
        {
            other.GetComponentInParent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
            enableCollision = false;
        }
    }
}
