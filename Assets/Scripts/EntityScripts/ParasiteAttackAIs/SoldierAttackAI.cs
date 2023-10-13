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

    private Coroutine counterCoroutine;

    private Coroutine combo;

    public void Start()
    {
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<MobNeutralAI>().OnAggroed += CounterBegin;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    private void CounterBegin(object sender, CombatArgs e)
    {
        if (counterCoroutine != null && combo != null)
        {
            return;
        }

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attacking = true;
        target = e.combatTarget;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        counterCoroutine = StartCoroutine(CounterAttack());
    }
    public void StartCombat(object sender, CombatArgs e)
    {
        if (attacking)
        {
            return;
        }
        target = e.combatTarget;
        attacking = true;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        int rand = Random.Range(0, 2);
        if (rand == 0)
        {
            attackCoroutine = StartCoroutine(MeleeAttack());
        }
        else
        {
            combo = StartCoroutine(TripleCombo());
        }
        
    }

    private IEnumerator CounterAttack()
    {
        anim.Play("Stun");
        yield return new WaitForSeconds(.5f);
        anim.Play("CounterAttack");
        yield return new WaitForSeconds(.25f);
        TriggerHitSphere(atkRadius * 1.5f);
        attacking = false;
        counterCoroutine = null;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
    }

    private IEnumerator TripleCombo()
    {
        int i = 0;
        if (mobMovement.target != null)
        {
            Vector3 dir = mobMovement.target.transform.position - transform.position;
            while (i < 3)
            {
                GetComponent<Rigidbody2D>().AddForce(dir, ForceMode2D.Impulse);
                anim.Play("Attack");
                yield return new WaitForSeconds(.25f);
                TriggerHitSphere(atkRadius / 1.5f);
                yield return new WaitForSeconds(.25f);
                dir += dir * 3;
                i++;
            }
            yield return new WaitForSeconds(.5f);
        }
        attacking = false;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        combo = null;
    }

    private IEnumerator MeleeAttack()
    {       
        anim.Play("Attack");
        yield return new WaitForSeconds(.5f);
        TriggerHitSphere(atkRadius);
        yield return new WaitForSeconds(.25f);
        attacking = false;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
    }

    private bool TriggerHitSphere(float radius)
    {
        if (mobMovement.target == null)
        {
            return false;
        }
        Vector3 _newPos = transform.position;
        _newPos.y += 5;
        Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, radius);

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
        return false;
    }
}
