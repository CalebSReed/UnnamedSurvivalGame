using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierAttackAI : MonoBehaviour, IAttackAI
{
    public Animator anim { get; set; }
    public RealMob realMob { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public MobMovementBase mobMovement { get; set; }

    private Coroutine attackCoroutine;
    private bool isCountering;

    private Coroutine counterCoroutine;

    private Coroutine combo;
    private bool isComboing;

    public void Start()
    {
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        mobMovement = GetComponent<MobMovementBase>();
        //GetComponent<MobNeutralAI>().OnAggroed += Star;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    public void StartCombat(object sender, CombatArgs e)
    {
        target = e.combatTarget;

        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        int rand = Random.Range(0, 2);
        if (rand == 0)
        {
            anim.Play("Attack");
        }
        else if (rand == 1)
        {
            anim.Play("Heavy");
        }
        
    }
}
