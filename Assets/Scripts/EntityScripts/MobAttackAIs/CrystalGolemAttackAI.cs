using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalGolemAttackAI : MonoBehaviour
{

    public MobMovementBase mobMovement { get; set; }
    public Animator anim { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public RealMob realMob { get; set; }

    private bool hasAttacked;

    public void StartCombat(object sender, CombatArgs e)
    {
        target = e.combatTarget;
        if (attacking)
        {
            return;
        }
        attacking = true;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        int _randVal = Random.Range(0, 3);
        hasAttacked = true;
        if (_randVal == 0)
        {
            StartCoroutine(Smash());
        }
        else if (_randVal == 1)
        {
            StartCoroutine(Leap());
        }
        else
        {
            StartCoroutine(Spin());
        }

    }

    void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Special);

        GetComponent<MobNeutralAI>().OnAggroed += StartCombat;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    private IEnumerator Smash()
    {
        anim.Play("Smash");
        yield return new WaitForSeconds(1.1f);
        TriggerHitSphere(atkRadius * 2);
        yield return new WaitForSeconds(.5f);
        TryToDaze();
    }

    private IEnumerator Leap()
    {
        anim.Play("Leap Smash");
        yield return new WaitForSeconds(1f);
        var dir = mobMovement.target.transform.position - transform.position;
        dir.Normalize();
        dir *= 200;
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
        yield return new WaitForSeconds(.3f);
        TriggerHitSphere(atkRadius*.4f);
        yield return new WaitForSeconds(1f);
        TryToLeap();
    }

    private IEnumerator Spin()
    {
        anim.Play("Spin");
        yield return new WaitForSeconds(.9f);
        TriggerHitSphere(atkRadius*1.5f);
        yield return new WaitForSeconds(.35f);
        TriggerHitSphere(atkRadius * 1.5f);
        yield return new WaitForSeconds(.5f);
        TriggerHitSphere(atkRadius * 1.5f);
        yield return new WaitForSeconds(.5f);
        TriggerHitSphere(atkRadius * 1.5f);
        yield return new WaitForSeconds(.25f);
        TryToDaze();
    }

    private IEnumerator Daze()
    {
        anim.Play("Dazed");
        yield return new WaitForSeconds(3);
        EndAttack();
    }

    private void TryToDaze()
    {
        var rand = Random.Range(0, 21);
        if (rand == 20)
        {
            StartCoroutine(Daze());
        }
        else
        {
            EndAttack();
        }
    }

    private void TryToLeap()
    {
        var rand = Random.Range(0, 2);
        if (rand == 0)
        {
            StartCoroutine(Leap());
        }
        else
        {
            TryToDaze();
        }
    }

    private void EndAttack()
    {
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
        hasAttacked = true;
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
                    return true;
                }
            }
            if (CalebUtils.GetParentOfTriggerCollider(_enemy) == mobMovement.target)
            {
                _enemy.GetComponentInParent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
                return true;
            }
        }
        return false;
    }
}
