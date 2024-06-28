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
    private Coroutine attackTimer;

    private bool moving;
    Vector3 dir;
    Rigidbody rb;
    public bool waitingToAttack;

    public void StartCombat(object sender, CombatArgs e)
    {
        target = e.combatTarget;
        if (attacking)
        {
            return;
        }
        //attacking = true;
        //mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        Attack();
    }

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;

        GetComponent<MobAggroAI>().StartCombat += StartCombat;
        GetComponent<MobAggroAI>().onAggro += StartAttackTimer;
        GetComponent<MobAggroAI>().onDeaggro += EndTimer;
        realMob.hpManager.OnDamageTaken += OnDamageTaken;
    }

    private void StartAttackTimer(object sender, System.EventArgs e)
    {
        attackTimer = StartCoroutine(WaitToAttack());
    }       

    private void EndTimer(object sender, System.EventArgs e)
    {
        if (attackTimer != null)
        {
            StopCoroutine(attackTimer);
        }
    }

    private IEnumerator WaitToAttack()
    {
        Debug.Log("waiting to attack!");
        float num = Random.Range(5f, 12f);
        yield return new WaitForSeconds(num);

        if (mobMovement.currentMovement == MobMovementBase.MovementOption.DoNothing)
        {
            attackTimer = StartCoroutine(WaitToAttack());
            yield break;
        }

        if (waitingToAttack)
        {
            yield break;
        }

        if (realMob.mobMovement.target != null)
        {
            if (Vector3.Distance(transform.position, mobMovement.target.transform.position) < mobMovement.surroundDistance + 5)//leniancy so u cant easily reset timer
            {
                var targetHealth = mobMovement.target.GetComponent<HealthManager>();
                if (targetHealth.currentHealth < targetHealth.maxHealth / 4)
                {
                    mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
                    //Debug.LogError("WOO!");
                }
                else
                {
                    anim.Play("HeavyAttack");
                    waitingToAttack = true;
                    StartCoroutine(ResumeAttacking(this));
                    CheckOthers();
                    //check every enemy in player enemylist if same type tell them to wait until 1 second after this attack finishes, then attack, and so on...
                    //realMob.audio.Play("lyncherLynch", transform.position, gameObject, false, true);
                }
            }
            attackTimer = StartCoroutine(WaitToAttack());
        }
    }

    private void CheckOthers()
    {
        Collider[] _hitEnemies = Physics.OverlapSphere(transform.position, 50);
        List<LyncherAttackAI> mobList = new List<LyncherAttackAI>();

        foreach (var enemy in _hitEnemies)
        {
            var enemyMob = enemy.GetComponent<RealMob>();
            if (enemyMob != null && enemyMob.mob.mobSO.mobType == realMob.mob.mobSO.mobType && enemyMob.mobMovement.target == mobMovement.target && enemyMob != this.realMob)
            {
                var lyncher = enemy.GetComponent<LyncherAttackAI>();
                lyncher.waitingToAttack = true;
                lyncher.StopCoroutine(attackTimer);
                mobList.Add(lyncher);
            }
        }
        StartCoroutine(QueueAttacks(mobList));
    }

    private IEnumerator QueueAttacks(List<LyncherAttackAI> mobList)
    {
        foreach (var lyncher in mobList)
        {
            yield return new WaitForSeconds(1);
            lyncher.anim.Play("HeavyAttack");
            lyncher.StartCoroutine(ResumeAttacking(lyncher));
        }
    }

    private IEnumerator ResumeAttacking(LyncherAttackAI lyncher)
    {
        yield return new WaitForSeconds(2.5f);
        lyncher.waitingToAttack = false;
    }

    private void Attack()
    {
        if (waitingToAttack)
        {
            return;
        }
        int _randVal = Random.Range(0, 2);
        if (_randVal == 0)
        {
            anim.Play("Melee");
        }
        else
        {
            anim.Play("HeavyAttack");
            //realMob.audio.Play("lyncherLynch", transform.position, gameObject, false, true);
            //StartCoroutine(TripleLunge());
        }
    }

    private void OnDamageTaken(object sender, DamageArgs args)
    {
        if (args.dmgType == DamageType.Heavy)
        {
            anim.Play("Stunned");    
        }
        //realMob.willStun = !realMob.willStun;
    }

    private void Update()
    {
        if (moving)
        {
            rb.MovePosition(transform.position + dir.normalized * realMob.mob.mobSO.walkSpeed * 2 * Time.fixedDeltaTime);
        }
    }
}
