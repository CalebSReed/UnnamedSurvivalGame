using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerAttackAI : MonoBehaviour, IAttackAI
{
    public Animator anim { get; set; }
    public RealMob realMob { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public MobMovementBase mobMovement { get; set; }

    private int attackCount = 0;

    public void Start()
    {
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<MobNeutralAI>().OnAggroed += CounterSlam;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
        realMob.animEvent.checkAttackConditions += CheckAttacks;
    }

    public void StartCombat(object sender, CombatArgs e)
    {
        attackCount = 0;
        target = e.combatTarget;

        if (mobMovement.currentMovement == MobMovementBase.MovementOption.DoNothing)
        {
            return;
        }
        attacking = true;

        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        anim.Play("UpperCut");
    }

    public void CounterSlam(object sender, CombatArgs e)
    {
        attackCount = 0;
        target = e.combatTarget;

        if (mobMovement.currentMovement == MobMovementBase.MovementOption.DoNothing)
        {
            return;
        }
        attacking = true;

        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        anim.Play("Slam");
    }
    
    private void CheckAttacks(object sender, AttackEventArgs e)
    {
        if (attackCount > 3)
        {
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
            attacking = false;
            return;
        }
        attackCount++;
        var rand = Random.Range(0, 4);

        if (rand == 0)
        {
            anim.Play("Swipe");
        }
        else if (rand == 1)
        {
            anim.Play("UpperCut");
        }
        else
        {
            anim.Play("Slam");
        }
    }
}
