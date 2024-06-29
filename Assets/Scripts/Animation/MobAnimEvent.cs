using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAnimEvent : MonoBehaviour
{
    [SerializeField] RealMob mob;
    [SerializeField] Rigidbody rb;
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
