using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkirmisherAttackAI : MonoBehaviour, IAttackAI
{
    public MobMovementBase mobMovement { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public RealMob realMob { get; set; }

    private bool fallback;
    public void Start()
    {
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        realMob = GetComponent<RealMob>();
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    public void StartCombat(object sender, CombatArgs e)
    {
        if (attacking)
        {
            return;
        }
        mobMovement.target = e.combatTarget;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        attacking = true;
        if (DistanceCheck())
        {
            return;
        }
        StartCoroutine(GetReadyToShoot());
    }

    private IEnumerator FallBack()
    {
        int i = 0;
        while (i < 60)
        {
            transform.position = CalebUtils.MoveAway(transform.position, mobMovement.target.transform.position, realMob.mob.mobSO.speed * 2 * Time.deltaTime);
            yield return null;
            i++;
        }
        Debug.Log("done");
        //attacking = false;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
        fallback = false;
        Shoot();
    }

    private bool DistanceCheck()
    {
        if (Vector3.Distance(transform.position, mobMovement.target.transform.position) < atkRadius / 2)
        {
            fallback = true;
            StartCoroutine(FallBack());
        }
        return fallback;
    }

    private IEnumerator GetReadyToShoot()
    {
        yield return new WaitForSeconds(1.5f);
        if (DistanceCheck())
        {
            yield break;
        }
        Shoot();
    }

    private void Shoot()
    {
        var _projectile = Instantiate(ItemObjectArray.Instance.pfProjectile, transform.position, Quaternion.identity);
        var vel = _projectile.GetComponent<Rigidbody2D>().velocity = (mobMovement.target.transform.position - transform.position) * 2;
        _projectile.GetComponent<ProjectileManager>().SetProjectile(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("SkirmisherProjectile"), amount = 1 }, transform.position, gameObject, vel, false, true);
        //_projectile.LookAt(mobMovement.target.transform.position);
        attacking = false;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
    }
}
