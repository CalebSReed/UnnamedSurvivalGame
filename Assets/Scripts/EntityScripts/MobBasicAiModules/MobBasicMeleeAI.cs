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
        GetComponent<MobNeutralAI>().OnAggroed += StartCombat;
        anim = realMob.mobAnim;
    }

    public void StartCombat(object sender, CombatArgs e)
    {
        if (attacking)//do nothing if attacking already
        {
            return;
        }
        attacking = true;
        target = e.combatTarget;
        GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()//make it so only one entity is attacked UNLESS AOE ATTACK, so make script to find nearest target
    {
        anim.Play("Attack");
        yield return new WaitForSeconds(.5f);//windup

        Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, atkRadius);    

        foreach (Collider2D _enemy in _hitEnemies)
        {
            if (_enemy.GetComponent<PlayerMain>() != null)
            {
                if (_enemy.GetComponent<PlayerMain>().godMode)
                {
                    GetComponent<HealthManager>().TakeDamage(999999, "Player", _enemy.gameObject);
                    break;
                }
            }

            if (_enemy.gameObject == target)
            {
                _enemy.GetComponent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
                break;
            }
        }
        yield return new WaitForSeconds(.5f);//attack lag
        CheckAttack();
    }

    private void CheckAttack()
    {
        Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, atkRadius);
        foreach (Collider2D _enemy in _hitEnemies)
        {
            if (_enemy.gameObject == target)
            {
                StartCoroutine(Attack());
                return;
            }
        }
        attacking = false;
        GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.Chase);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(realMob.sprRenderer.bounds.center, atkRadius);
    }

}
