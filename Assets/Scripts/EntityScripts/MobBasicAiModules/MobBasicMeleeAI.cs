using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobBasicMeleeAI : MonoBehaviour, IAttackAI
{
    public RealMob realMob { get; set; }
    public Animator anim { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public MobMovementBase mobMovement { get; set; }

    private void Start()
    {
        realMob = GetComponent<RealMob>();
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
        anim = realMob.mobAnim;
        mobMovement = GetComponent<MobMovementBase>();
    }

    public void StartCombat(object sender, CombatArgs e)
    {
        target = mobMovement.target;

        anim.Play("Attack");
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(realMob.sprRenderer.bounds.center, atkRadius);
    }

}
