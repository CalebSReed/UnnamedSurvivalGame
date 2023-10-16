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
        GetComponent<MobNeutralAI>().OnAggroed += CounterBegin;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    private void CounterBegin(object sender, CombatArgs e)
    {
        target = e.combatTarget;
        if (isCountering || isComboing)
        {
            Debug.Log("Ignoring");
            return;
        }

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attacking = true;
        isCountering = true;
        StopAllCoroutines();
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        counterCoroutine = StartCoroutine(CounterAttack());
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
        anim.StopPlayback();
        anim.Play("Stun");
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        yield return new WaitForSeconds(.5f);
        anim.Play("CounterAttack");
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        yield return new WaitForSeconds(.25f);
        TriggerHitSphere(atkRadius * 1.5f);
        counterCoroutine = null;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
        isCountering = false;
    }

    private IEnumerator TripleCombo()
    {
        anim.StopPlayback();
        int i = 0;
        if (mobMovement.target != null)
        {
            Vector3 dir = mobMovement.target.transform.position - transform.position;
            while (i < 3)
            {
                GetComponent<Rigidbody2D>().AddForce(dir, ForceMode2D.Impulse);
                anim.Play("Attack");
                mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
                yield return new WaitForSeconds(.5f);
                TriggerHitSphere(atkRadius / 1.5f);
                mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
                yield return new WaitForSeconds(.5f);
                dir += dir * 3;
                i++;
            }
            yield return new WaitForSeconds(1f);
        }
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
        combo = null;
    }

    private IEnumerator MeleeAttack()
    {       
        anim.Play("Attack");
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        yield return new WaitForSeconds(.5f);
        TriggerHitSphere(atkRadius);
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        yield return new WaitForSeconds(.25f);
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
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

            if (_enemy.gameObject == mobMovement.target)
            {
                _enemy.GetComponent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
                break;
            }
        }
        return false;
    }
}
