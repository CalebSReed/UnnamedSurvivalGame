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

    public void Start()
    {
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<MobNeutralAI>().OnAggroed += StartCombat;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    public void StartCombat(object sender, CombatArgs e)
    {
        target = e.combatTarget;
        if (attacking)
        {
            return;
        }
        attacking = true;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        StartCoroutine(MeleeAttack());
    }

    private IEnumerator MeleeAttack()
    {
        anim.Play("Attack");
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        yield return new WaitForSeconds(.5f);
        TriggerHitSphere(atkRadius);
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        yield return new WaitForSeconds(.25f);

        if (!realMob.inventory.InventoryHasOpenSlot())
        {
            StartCoroutine(Leave());
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.Special);
        }
        else
        {
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
            attacking = false;
        }
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
        Collider[] _hitEnemies = Physics.OverlapSphere(realMob.sprRenderer.bounds.center, radius);

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
