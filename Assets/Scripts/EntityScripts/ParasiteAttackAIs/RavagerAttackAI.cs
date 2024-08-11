using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RavagerAttackAI : MonoBehaviour, IAttackAI
{
    public Animator anim { get; set; }
    public RealMob realMob { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public MobMovementBase mobMovement { get; set; }

    public void Start()
    {
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<MobNeutralAI>().OnAggroed += CounterSwipe;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    public void StartCombat(object sender, CombatArgs e)
    {
        target = e.combatTarget;

        if (mobMovement.currentMovement == MobMovementBase.MovementOption.DoNothing)
        {
            return;
        }
        attacking = true;

        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        var rand = Random.Range(0, 2);
        if (rand == 0)
        {
            anim.Play("NormalSwipe");
        }
        else
        {
            anim.Play("HeavySwipe");
        }

    }

    public void CounterSwipe(object sender, CombatArgs e)
    {
        target = e.combatTarget;

        if (mobMovement.currentMovement == MobMovementBase.MovementOption.DoNothing)
        {
            return;
        }

        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        anim.Play("HeavySwipe");
    }
}
