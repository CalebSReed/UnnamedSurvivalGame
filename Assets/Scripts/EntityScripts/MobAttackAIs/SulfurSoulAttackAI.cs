using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SulfurSoulAttackAI : MonoBehaviour, IAttackAI
{
    public MobMovementBase mobMovement { get; set; }
    public Animator anim { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public RealMob realMob { get; set; }

    public void StartCombat(object sender, CombatArgs e)
    {
        target = e.combatTarget;

        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        anim.Play("SwipeL");
    }

    void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;

        GetComponent<MobAggroAI>().StartCombat += StartCombat;
        realMob.animEvent.checkAttackConditions += CheckCombatConditions;
    }

    private void CheckCombatConditions(object sender, AttackEventArgs e)
    {
        if (e.checkType == "left")
        {
            var rand = Random.Range(0, 3);
            if (rand == 0)
            {
                anim.Play("Shoot");
            }
            else
            {
                anim.Play("SwipeR");
            }
        }
        else if (e.checkType == "right")
        {
            var rand = Random.Range(0, 3);
            if (rand == 0)
            {
                anim.Rebind();
                anim.Play("Shoot", 0, 0);
            }
            else
            {
                EndCombat();
            }
        }
        else if (e.checkType == "projectile")
        {
            var rand = Random.Range(0, 3);
            if (rand == 0)
            {
                anim.Rebind();
                anim.Play("Shoot", 0, 0);
            }
            else
            {
                EndCombat();
            }
        }

    }

    private void EndCombat()
    {
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
    }
}
