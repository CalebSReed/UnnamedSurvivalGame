using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScouterAttackAI : MonoBehaviour, IAttackAI
{
    public RealMob realMob { get; set; }
    public Animator anim { get; set; }
    public MobMovementBase mobMovement { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }

    private bool currentlyAttacking;

    private bool attackLanded;

    void Start()
    {
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<MobNeutralAI>().OnAggroed += StartCombat;
    }

    public void StartCombat(object sender, CombatArgs e)
    {
        mobMovement.target = e.combatTarget;
        if (attacking)
        {
            return;
        }
        attacking = true;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        mobMovement.ignoreFleeingOverride = true;
        StartCoroutine(BackOff());
    }

    private void DecideNextAttack()
    {
        int _randVal = Random.Range(0, 2);
        if (_randVal == 0)
        {
            StartCoroutine(LaunchAttack());
        }
        else
        {
            StartCoroutine(BiteAttack());
        }
    }

    private bool TriggerHitSphere()
    {
        if (mobMovement.target == null)
        {
            return false;
        }
        Collider[] _targetList = Physics.OverlapSphere(transform.position, 2);

        foreach (Collider _target in _targetList)
        {
            if (!_target.isTrigger)
            {
                continue;
            }
            if (CalebUtils.GetParentOfTriggerCollider(_target).CompareTag("Player"))
            {
                _target.GetComponentInParent<HealthManager>().TakeDamage(realMob.mob.mobSO.damage, gameObject.tag, gameObject);
                return true;
            }
        }
        return false;
    }

    private IEnumerator BackOff()
    {
        int i = 0;
        while (i < 60)
        {
            transform.position = CalebUtils.MoveAway(transform.position, mobMovement.target.transform.position, realMob.mob.mobSO.speed * Time.deltaTime);
            yield return null;
            i++;
        }
        StartCoroutine(LaunchAttack());
    }

    private IEnumerator LaunchAttack()
    {
        anim.Play("Launch");
        yield return new WaitForSeconds(1);
        GetComponent<Rigidbody>().mass = .25f;
        currentlyAttacking = true;
        //Debug.LogError("ATTTTTTTTTTTACKKKKKKKKK!");
        Vector3 dir = mobMovement.target.transform.position - transform.position;
        dir.Normalize();
        dir *= 15;//multiply by desired magnitude EASY!!
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
        yield return new WaitForSeconds(1.5f);
        GetComponent<Rigidbody>().mass = 3;
        currentlyAttacking = false;
        StartCoroutine(Chase());
    }

    private IEnumerator BiteAttack()
    {
        int i = 0;
        if (mobMovement.target != null)
        {
            Vector3 dir = mobMovement.target.transform.position - transform.position;
            while (i < 3)
            {
                anim.Play("Bite");
                yield return new WaitForSeconds(.25f);
                dir.Normalize();
                dir *= 10 + (i * 2);
                attackLanded = false;
                GetComponent<Rigidbody>().mass = .75f;
                currentlyAttacking = true;
                //Debug.LogError("BITE!");
                attackLanded = TriggerHitSphere();
                yield return new WaitForSeconds(.25f);
                GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
                i++;
            }
            yield return new WaitForSeconds(.5f);
            GetComponent<Rigidbody>().mass = 3;
        }
        attackLanded = false;
        currentlyAttacking = false;
        StartCoroutine(Chase());
    }

    private IEnumerator Chase()
    {
        transform.position = Vector3.MoveTowards(transform.position, mobMovement.target.transform.position, realMob.mob.mobSO.speed * Time.deltaTime);
        Collider[] _targetList = Physics.OverlapSphere(transform.position, realMob.mob.mobSO.combatRadius);

        foreach (Collider _target in _targetList)
        {
            if (!_target.isTrigger)
            {
                continue;
            }
            else if (CalebUtils.GetParentOfTriggerCollider(_target).CompareTag("Player"))
            {
                print("START THE ATTACK");
                DecideNextAttack();
                yield break;
            }
        }

        Collider[] _targetList2 = Physics.OverlapSphere(transform.position, realMob.mob.mobSO.abandonRadius);

        bool _playerFound = false;
        foreach (Collider _target in _targetList2)
        {
            if (!_target.isTrigger)
            {
                continue;
            }
            else if (CalebUtils.GetParentOfTriggerCollider(_target).CompareTag("Player"))
            {
                _playerFound = true;
            }
        }

        if (!_playerFound)
        {
            print("eh, bored");
            attacking = false;
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.Wait);//if too far, stop attacking
            if (GetComponent<MobNeutralAI>().isInEnemyList())
            {
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>().enemyList.Remove(gameObject);
            }
            yield break;
        }

        yield return null;
        StartCoroutine(Chase());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<RealWorldObject>() != null && collision.collider.GetComponent<RealWorldObject>().obj.woso.isParasiteMade)
        {
            return;
        }
        if (collision.collider.GetComponent<RealWorldObject>() != null && !collision.collider.GetComponent<RealWorldObject>().obj.woso.isPlayerMade && !collision.collider.GetComponent<RealWorldObject>().obj.woso.isParasiteMade)
        {
            return;
        }

        if (collision.collider.GetComponent<HealthManager>() != null && currentlyAttacking && !attackLanded)//if damageable, hit it. So basically hit ANYTHING
        {
            if (collision.collider.GetComponent<RealMob>() != null)
            {
                if (collision.collider.GetComponent<RealMob>().mob.mobSO.isParasite)//if is parasite, dont hurt it lol
                {
                    return;
                }
            }
            collision.gameObject.GetComponent<HealthManager>().TakeDamage(realMob.mob.mobSO.damage, gameObject.tag, gameObject);//be diff on attack type?
            currentlyAttacking = false;
        }
    }
}
