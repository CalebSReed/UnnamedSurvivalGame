using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SulfurCystAttackAI : MonoBehaviour, IAttackAI
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

        StartCoroutine(TryToExplode());
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

    private IEnumerator TryToExplode()
    {
        anim.SetBool("exploding", true);
        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForSeconds(.1f);
            if (Vector3.Distance(transform.position, target.transform.position) > realMob.mob.mobSO.preyDetectionRadius + 4)
            {
                anim.SetBool("exploding", false);
                yield return new WaitForSeconds(.5f);
                attacking = false;
                mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
                yield break;
            }
        }
        Explode(15);
        realMob.Die(false, false);
    }

    private void Explode(float radius)
    {
        Collider[] _hitEnemies = Physics.OverlapSphere(transform.position, radius);
        if (GameManager.Instance.isServer)
        {
            RealWorldObject.SpawnWorldObject(transform.position, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("explosion")});
        }
        foreach (Collider _enemy in _hitEnemies)
        {
            if (!_enemy.isTrigger)
            {
                continue;
            }
            else if (_enemy.GetComponentInParent<HealthManager>() != null)
            {
                _enemy.GetComponentInParent<HealthManager>().TakeDamage(realMob.mob.mobSO.damage, realMob.mob.mobSO.mobType, gameObject);
                /*if (_enemy.GetComponentInParent<PlayerMain>().godMode)
                {
                    GetComponent<HealthManager>().TakeDamage(999999, "Player", _enemy.gameObject);
                    return true;
                }
            }
            if (CalebUtils.GetParentOfTriggerCollider(_enemy) == mobMovement.target)
            {
                _enemy.GetComponentInParent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
                return true;*/
            }
        }
    }
}
