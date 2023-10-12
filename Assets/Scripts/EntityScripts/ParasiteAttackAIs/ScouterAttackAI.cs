using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScouterAttackAI : MonoBehaviour, IAttackAI
{
    public Animator anim;
    public RealMob realMob { get; set; }
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
        if (attacking)
        {
            return;
        }
        mobMovement.target = e.combatTarget;
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
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, 2);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.CompareTag("Player"))
            {
                _target.GetComponent<HealthManager>().TakeDamage(realMob.mob.mobSO.damage, gameObject.tag, gameObject);
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
        GetComponent<Rigidbody2D>().mass = .25f;
        currentlyAttacking = true;
        //Debug.LogError("ATTTTTTTTTTTACKKKKKKKKK!");
        Vector3 dir = mobMovement.target.transform.position - transform.position;
        dir += dir;
        GetComponent<Rigidbody2D>().AddForce(dir, ForceMode2D.Impulse);
        yield return new WaitForSeconds(1.5f);
        GetComponent<Rigidbody2D>().mass = 3;
        currentlyAttacking = false;
        StartCoroutine(Chase());
    }

    private IEnumerator BiteAttack()
    {
        int i = 0;
        Vector3 dir = mobMovement.target.transform.position - transform.position;
        while (i < 3)
        {
            anim.Play("Bite");
            yield return new WaitForSeconds(.25f);
            dir += dir;
            attackLanded = false;
            GetComponent<Rigidbody2D>().mass = .75f;
            currentlyAttacking = true;
            //Debug.LogError("BITE!");
            attackLanded = TriggerHitSphere();
            yield return new WaitForSeconds(.25f);
            GetComponent<Rigidbody2D>().AddForce(dir, ForceMode2D.Impulse);
            i++;
        }
        yield return new WaitForSeconds(.5f);
        GetComponent<Rigidbody2D>().mass = 3;
        attackLanded = false;
        currentlyAttacking = false;
        StartCoroutine(Chase());
    }

    private IEnumerator Chase()
    {
        transform.position = Vector3.MoveTowards(transform.position, mobMovement.target.transform.position, realMob.mob.mobSO.speed * Time.deltaTime);
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, realMob.mob.mobSO.combatRadius);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.CompareTag("Player"))
            {
                print("START THE ATTACK");
                DecideNextAttack();
                yield break;
            }
        }

        Collider2D[] _targetList2 = Physics2D.OverlapCircleAll(realMob.sprRenderer.bounds.center, realMob.mob.mobSO.abandonRadius);

        bool _playerFound = false;
        foreach (Collider2D _target in _targetList2)
        {
            if (_target.CompareTag("Player"))
            {
                _playerFound = true;
            }
        }

        if (!_playerFound)
        {
            print("eh, bored");
            attacking = false;
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.Wait);//if too far, stop attacking
            yield break;
        }

        yield return null;
        StartCoroutine(Chase());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
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
