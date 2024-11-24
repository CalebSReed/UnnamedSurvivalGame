using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParasiticHeartAttackAI : MonoBehaviour, IAttackAI
{
    public MobMovementBase mobMovement { get; set; }
    public Animator anim { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public RealMob realMob { get; set; }

    private bool appendageCooldown;

    private bool reinforcementCooldown;

    private bool puddleCooldown;

    private bool readyToSummon;

    private Coroutine summonCoroutine;

    void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;

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

        if (GetComponent<HealthManager>().currentHealth < GetComponent<HealthManager>().maxHealth / 2)
        {
            realMob.sprRenderer.color = Color.red;
        }

        if (summonCoroutine == null)
        {
            summonCoroutine = StartCoroutine(WaitToSummonMore());
        }

        DecideNextAttack();
    }

    private void TryToSummon()
    {
        if (!appendageCooldown)
        {
            StartCoroutine(SummonAppendage());
            EndCombat();
            StartCoroutine(WaitToSummonMore());
            readyToSummon = false;
            return;
        }

        if (!puddleCooldown)
        {
            StartCoroutine(GrowPuddle());
            EndCombat();
            StartCoroutine(WaitToSummonMore());
            readyToSummon = false;
            return;
        }

        if (!reinforcementCooldown)
        {
            StartCoroutine(SummonReinforcements());
            EndCombat();
            StartCoroutine(WaitToSummonMore());
            readyToSummon = false;
            return;
        }
    }

    private void DecideNextAttack()
    {
        if (readyToSummon)
        {
            if (!appendageCooldown)
            {
                StartCoroutine(SummonAppendage());
                EndCombat();
                StartCoroutine(WaitToSummonMore());
                readyToSummon = false;
                return;
            }

            if (!puddleCooldown)
            {
                StartCoroutine(GrowPuddle());
                EndCombat();
                StartCoroutine(WaitToSummonMore());
                readyToSummon = false;
                return;
            }

            if (!reinforcementCooldown)
            {
                StartCoroutine(SummonReinforcements());
                EndCombat();
                StartCoroutine(WaitToSummonMore());
                readyToSummon = false;
                return;
            }
        }

        int _randVal = Random.Range(0, 4);
        if (_randVal == 0)
        {
            StartCoroutine(Pulsate());
        }
        else if (_randVal == 1)
        {
            if (!appendageCooldown)
            {
                StartCoroutine(SummonAppendage());
            }
            else
            {
                DecideNextAttack();
            }
        }
        else if (_randVal == 2)
        {
            if (!puddleCooldown)
            {
                StartCoroutine(GrowPuddle());
            }
            else
            {
                DecideNextAttack();
            }
        }
        else
        {
            if (!reinforcementCooldown)
            {
                StartCoroutine(SummonReinforcements());
            }
            else
            {
                DecideNextAttack();
            }
        }
    }

    private void EndCombat()
    {
        if (mobMovement.target != null && Vector3.Distance(transform.position, mobMovement.target.transform.position) < 32)
        {
            //DecideNextAttack();
        }

        if (GetComponent<HealthManager>().currentHealth < GetComponent<HealthManager>().maxHealth / 2)
        {
            realMob.sprRenderer.color = Color.red;
        }

        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
    }

    private IEnumerator GrowPuddle()
    {
        var pos = CalebUtils.RandomPositionInRadius(transform.position, 10, 30);
        var obj = Instantiate(MobAssets.Instance.summonEffect, pos, Quaternion.identity);
        anim.Play("GrowPuddle");
        yield return new WaitForSeconds(1);
        Destroy(obj);
        RealWorldObject.SpawnWorldObject(pos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("parasiticpuddle") });
        yield return new WaitForSeconds(1f);
        StartCoroutine(PuddleCooldown());
        EndCombat();
    }

    private IEnumerator Pulsate()
    {
        anim.Play("Pulsate");
        yield return new WaitForSeconds(.5f);
        TriggerHitSphere(atkRadius);
        yield return new WaitForSeconds(1f);
        EndCombat();
    }

    private IEnumerator SummonAppendage()
    {
        var pos = CalebUtils.RandomPositionInRadius(transform.position, 15, 35);
        var obj = Instantiate(MobAssets.Instance.summonEffect, pos, Quaternion.identity);
        anim.Play("SummonAppendage");
        yield return new WaitForSeconds(1.25f);
        Destroy(obj);
        RealMob.SpawnMob(pos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("parasiticappendage") });
        yield return new WaitForSeconds(1f);
        StartCoroutine(AppendageCooldown());
        EndCombat();
    }

    private IEnumerator SummonReinforcements()
    {
        var pos = CalebUtils.RandomPositionInRadius(transform.position, 15, 45);
        var pos2 = CalebUtils.RandomPositionInRadius(transform.position, 15, 45);
        var obj = Instantiate(MobAssets.Instance.summonEffect, pos, Quaternion.identity);
        var obj2 = Instantiate(MobAssets.Instance.summonEffect, pos2, Quaternion.identity);
        anim.Play("SummonReinforcements");
        yield return new WaitForSeconds(1);
        Destroy(obj);
        Destroy(obj2);
        RealMob.SpawnMob(pos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        RealMob.SpawnMob(pos2, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        yield return new WaitForSeconds(1f);
        StartCoroutine(ReinforcementCooldown());
        EndCombat();
    }

    private IEnumerator AppendageCooldown()
    {
        appendageCooldown = true;
        if (GetComponent<HealthManager>().currentHealth < GetComponent<HealthManager>().maxHealth / 2)
        {
            yield return new WaitForSeconds(5);
        }
        else
        {
            yield return new WaitForSeconds(10);
        }
        appendageCooldown = false;
    }

    private IEnumerator ReinforcementCooldown()
    {
        reinforcementCooldown = true;
        if (GetComponent<HealthManager>().currentHealth < GetComponent<HealthManager>().maxHealth / 2)
        {
            yield return new WaitForSeconds(15);
        }
        else
        {
            yield return new WaitForSeconds(30);
        }
        reinforcementCooldown = false;
    }

    private IEnumerator PuddleCooldown()
    {
        puddleCooldown = true;
        if (GetComponent<HealthManager>().currentHealth < GetComponent<HealthManager>().maxHealth / 2)
        {
            yield return new WaitForSeconds(5);
        }
        else
        {
            yield return new WaitForSeconds(15);
        }
        puddleCooldown = false;
    }

    private IEnumerator WaitToSummonMore()
    {
        yield return new WaitForSeconds(30);
        if (mobMovement.target != null && Vector3.Distance(transform.position, mobMovement.target.transform.position) < 48)
        {
            readyToSummon = true;
            TryToSummon();
        }
        summonCoroutine = null;
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
