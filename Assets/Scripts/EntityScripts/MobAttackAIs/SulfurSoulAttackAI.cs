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
        if (attacking)
        {
            return;
        }
        attacking = true;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        StartCoroutine(SwingL());
    }

    void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;

        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    private void EndCombat()
    {
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
    }

    private IEnumerator SwingL()
    {
        yield return new WaitForSeconds(.25f);
        anim.Play("SwipeL");
        yield return new WaitForSeconds(.25f);
        ApplyForce();
        yield return new WaitForSeconds(.25f);
        TriggerHitSphere(atkRadius/2);
        yield return new WaitForSeconds(.5f);
        var rand = Random.Range(0, 3);
        if (rand == 0)
        {
            StartCoroutine(FlingProjectile());
        }
        else
        {
            StartCoroutine(SwingR());
        }
    }

    private IEnumerator SwingR()
    {
        anim.Play("SwipeR");
        yield return new WaitForSeconds(.25f);
        ApplyForce();
        yield return new WaitForSeconds(.25f);
        TriggerHitSphere(atkRadius/2);
        yield return new WaitForSeconds(.5f);
        var rand = Random.Range(0, 3);
        if (rand == 0)
        {
            StartCoroutine(FlingProjectile());
        }
        else
        {
            EndCombat();
        }
    }

    private IEnumerator FlingProjectile()
    {
        if (mobMovement.target == null)
        {
            yield break; ;
        }
        anim.Play("Shoot");
        yield return new WaitForSeconds(.75f);
        ProjectileManager.SpawnProjectile(gameObject, mobMovement.target, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("sulfurproj") }, 50, WosoArray.Instance.SearchWOSOList("sulfurpuddle"));
        yield return new WaitForSeconds(1.25f);
        var rand = Random.Range(0, 3);
        if (rand == 0)
        {
            StartCoroutine(FlingProjectile());
        }
        else
        {
            EndCombat();
        }
    }

    private void ApplyForce()
    {
        if (mobMovement.target == null)
        {
            return;
        }
        Vector3 dir = mobMovement.target.transform.position - transform.position;

        dir.Normalize();
        dir *= 150;
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
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
