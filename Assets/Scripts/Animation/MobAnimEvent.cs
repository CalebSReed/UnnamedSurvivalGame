using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MobAnimEvent : MonoBehaviour
{
    [SerializeField] RealMob mob;
    [SerializeField] Rigidbody rb;
    public event EventHandler onAttackEnded;
    public event EventHandler becomeProjectile;
    public event EventHandler unbecomeProjectile;
    bool moving;
    Vector3 dir;
    float speedmult;

    private void Start()
    {
        
    }

    public void OnHitEnemies(int dmgMult = 1)
    {
        mob.HitEnemies(mob.mob.mobSO.combatRadius, dmgMult);
    }

    public void OnDisableMovement()
    {
        mob.mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        moving = false;
    }

    public void GainSuperArmor()
    {
        mob.willStun = false;
    }

    public void BecomeProjectile()
    {
        becomeProjectile?.Invoke(this, EventArgs.Empty);
    }

    public void UnBecomeProjectile()
    {
        unbecomeProjectile?.Invoke(this, EventArgs.Empty);
    }

    public void ResumeChasing()
    {
        Debug.Log("goin back now");

        if (mob.mob.mobSO.aggroType == MobAggroType.AggroType.Aggressive)
        {
            mob.mobMovement.SwitchMovement(mob.mob.mobSO.aggroStrategy);
        }
        else
        {
            mob.mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveAway);
        }
        onAttackEnded?.Invoke(this, EventArgs.Empty);
    }

    public void MoveTowardsTarget(AnimationEvent animEvent)
    {
        dir = mob.mobMovement.target.transform.position - transform.position;
        speedmult = animEvent.floatParameter;
        moving = true;
    }

    public void StopMovingTowardsTarget()
    {
        moving = false;
    }

    private void Update()
    {
        if (moving)
        {
            rb.MovePosition(transform.position + dir.normalized * mob.mob.mobSO.walkSpeed * speedmult * Time.fixedDeltaTime);
        }
    }
}
