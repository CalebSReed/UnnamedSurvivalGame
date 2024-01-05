using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LyncherAttackAI : MonoBehaviour, IAttackAI
{
    public MobMovementBase mobMovement { get; set; }
    public Animator anim { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public RealMob realMob { get; set; }

    private bool enableCollision;

    public void StartCombat(object sender, CombatArgs e)
    {
        target = e.combatTarget;
        if (attacking)
        {
            return;
        }
        attacking = true;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        int _randVal = Random.Range(0, 2);
        if (_randVal == 0)
        {
            StartCoroutine(Melee());
        }
        else
        {
            StartCoroutine(TripleLunge());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;

        GetComponent<MobNeutralAI>().OnAggroed += StartCombat;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    private IEnumerator Melee()
    {
        yield return new WaitForSeconds(.15f);
        anim.Play("Melee");
        yield return new WaitForSeconds(.25f);
        TriggerHitSphere(atkRadius);
        yield return new WaitForSeconds(.5f);
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
    }

    private IEnumerator TripleLunge()
    {
        int i = 0;
        Vector3 dir = mobMovement.target.transform.position - transform.position;
        while (i < 3)
        {
            yield return new WaitForSeconds(.25f);
            anim.Play("Charge");
            yield return new WaitForSeconds(.5f);
            if (mobMovement.target != null)
            {
                dir = mobMovement.target.transform.position - transform.position;
            }
            dir.Normalize();
            dir *= 200 + (i * 50);
            GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
            yield return new WaitForSeconds(.1f);
            TriggerHitSphere(atkRadius/2);
            //Debug.LogError("BITE!");
            yield return new WaitForSeconds(.25f);
            i++;
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (!enableCollision || mobMovement.target == null)
        {
            return;
        }
        if (other.GetComponentInParent<PlayerMain>() != null)
        {
            if (other.GetComponentInParent<PlayerMain>().godMode)
            {
                GetComponent<HealthManager>().TakeDamage(999999, "Player", other.gameObject);
                return;
            }
        }

        if (CalebUtils.GetParentOfTriggerCollider(other) == mobMovement.target)
        {
            other.GetComponentInParent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
        }
    }
}
