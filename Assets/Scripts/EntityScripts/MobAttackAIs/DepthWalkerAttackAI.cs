using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthWalkerAttackAI : MonoBehaviour, IAttackAI
{
    public MobMovementBase mobMovement { get; set; }
    public Animator anim { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public RealMob realMob { get; set; }

    private bool enableCollision;

    private bool summoned;
    public void StartCombat(object sender, CombatArgs e)
    {
        target = e.combatTarget;
        if (attacking)
        {
            return;
        }
        attacking = true;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);

        int _randVal = Random.Range(0, 11);
        if (_randVal == 10 && !summoned)
        {
            StartCoroutine(Summon());
            return;
        }
        _randVal = Random.Range(0, 3);
        if (_randVal == 0)
        {
            StartCoroutine(BigAttack());
        }
        else if (_randVal == 1)
        {
            StartCoroutine(FastAttack());
        }
        else
        {
            StartCoroutine(Leap());
        }

    }

    void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;

        GetComponent<MobNeutralAI>().OnAggroed += StartCombat;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    private IEnumerator FastAttack()
    {
        anim.Play("FastAttack");
        yield return new WaitForSeconds(.5f);
        TriggerHitSphere(atkRadius * 1.5f);
        yield return new WaitForSeconds(1f / 3f);
        TriggerHitSphere(atkRadius * 1.5f);
        yield return new WaitForSeconds(1f / 3f);
        TriggerHitSphere(atkRadius * 1.5f);
        yield return new WaitForSeconds(1f / 3f);
        TryToSurprise();
    }

    private IEnumerator BigAttack()
    {
        anim.Play("Attack");
        yield return new WaitForSeconds(1.3f);
        TriggerHitSphere(atkRadius * 2);
        yield return new WaitForSeconds(.7f);
        TryToSurprise();
    }

    private IEnumerator Leap()
    {
        anim.Play("Leap");
        yield return new WaitForSeconds(.5f);
        var dir = mobMovement.target.transform.position - transform.position;
        dir.Normalize();
        dir *= 75;
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
        yield return new WaitForSeconds(.25f);
        TriggerHitSphere(atkRadius);
        yield return new WaitForSeconds(.5f);
        TryToSurprise();
    }

    private IEnumerator Summon()
    {
        anim.Play("Summon");
        yield return new WaitForSeconds(1.1f);
        StartCoroutine(SpawnPillars());
        yield return new WaitForSeconds(2.25f);
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
    }

    private IEnumerator Surprise()
    {
        anim.Play("Surprise");
        yield return new WaitForSeconds(.2f);
        var dir = mobMovement.target.transform.position - transform.position;
        dir.Normalize();
        dir *= 100;
        GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
        enableCollision = true;
        yield return new WaitForSeconds(.8f);
        enableCollision = false;
        yield return new WaitForSeconds(.5f);
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        attacking = false;
    }

    private void TryToSurprise()
    {
        var rand = Random.Range(0, 5);
        if (rand == 0)
        {
            StartCoroutine(Surprise());
        }
        else
        {
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
            attacking = false;
        }
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

    private IEnumerator SpawnPillars()
    {
        var dir = transform.position - mobMovement.target.transform.position;
        dir *= 25;
        var target = mobMovement.target.GetComponent<Rigidbody>();
        var prevDrag = target.drag;
        target.drag = 1;
        Debug.Log(target.gameObject);
        target.AddForce(dir, ForceMode.Impulse);//make sure they are imprisoned with you >:D

        var pos = transform.position;

        summoned = true;
        int i = 40;
        int j = 0;
        while (i > 0)
        {
            RealWorldObject.SpawnWorldObject(new Vector3(pos.x + j, pos.y, pos.z + i), new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Depth Pillar") });
            i-= 2;
            j+= 2;
            yield return null;
        }
        while (i > -40)
        {
            RealWorldObject.SpawnWorldObject(new Vector3(pos.x + j, pos.y, pos.z + i), new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Depth Pillar") });
            i -= 2;
            j -= 2;
            yield return null;
        }
        target.drag = prevDrag;
        while (i < 0)
        {
            RealWorldObject.SpawnWorldObject(new Vector3(pos.x + j, pos.y, pos.z + i), new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Depth Pillar") });
            i += 2;
            j -= 2;
            yield return null;
        }
        while (i < 40)
        {
            RealWorldObject.SpawnWorldObject(new Vector3(pos.x + j, pos.y, pos.z + i), new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Depth Pillar") });
            i += 2;
            j += 2;
            yield return null;
        }
        StartCoroutine(WaitToSummon());
    }

    private IEnumerator WaitToSummon()
    {
        yield return new WaitForSeconds(60);
        summoned = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enableCollision || mobMovement.target == null && !other.isTrigger)
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

        Debug.Log(other);
        if (CalebUtils.GetParentOfTriggerCollider(other) == mobMovement.target)
        {
            other.GetComponentInParent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
        }
    }
}
