using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryAttackAI : MonoBehaviour, IAttackAI
{
    public Animator anim { get; set; }
    public RealMob realMob { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public MobMovementBase mobMovement { get; set; }

    private int attackCount;

    private bool isFleeing;
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

    private void Update()
    {
        if (isFleeing && Vector3.Distance(realMob.player.transform.position, transform.position) > 400)
        {
            realMob.Die(false, false);
        }
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
        if (!realMob.inventory.InventoryHasOpenSlot())
        {
            isFleeing = true;
            mobMovement.target = realMob.player.gameObject;
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveAway);
            return;
        }
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
        return;
        /*if (attackCount > )
        {
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
            attacking = false;
            return;
        }
        attackCount++;*/
    }

    private IEnumerator Leave()
    {
        yield return new WaitForSeconds(5f);
        realMob.Die(false, false);
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
                    break;
                }
            }

            if (CalebUtils.GetParentOfTriggerCollider(_enemy) == mobMovement.target)
            {
                if (mobMovement.target.GetComponent<RealWorldObject>() != null && mobMovement.target.GetComponent<RealWorldObject>().woso.isContainer)
                {
                    _enemy.GetComponentInParent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage * 10, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
                    break;
                }
                else if (mobMovement.target.GetComponent<RealItem>() != null)
                {
                    realMob.inventory.AddItem(mobMovement.target.GetComponent<RealItem>().item, transform.position, false);
                    mobMovement.target.GetComponent<RealItem>().DestroySelf();
                }
                else
                {
                    _enemy.GetComponentInParent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
                    break;
                }
            }
        }
        return false;
    }
}
