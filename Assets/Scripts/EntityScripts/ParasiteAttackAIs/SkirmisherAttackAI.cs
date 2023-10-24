using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkirmisherAttackAI : MonoBehaviour, IAttackAI
{
    public MobMovementBase mobMovement { get; set; }
    public Animator anim { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public RealMob realMob { get; set; }

    private bool fallback;
    public void Start()
    {
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<MobNeutralAI>().OnAggroed += StartCombat;
        GetComponent<MobAggroAI>().StartCombat += StartCombat;
    }

    public void StartCombat(object sender, CombatArgs e)
    {
        mobMovement.target = e.combatTarget;
        if (attacking)
        {
            return;
        }
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
        anim.Play("Retreat");
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
        StartCoroutine(Shoot());
    }

    private bool DistanceCheck()
    {
        if (mobMovement.target != null && Vector3.Distance(transform.position, mobMovement.target.transform.position) < atkRadius / 2)
        {
            fallback = true;
            StartCoroutine(FallBack());
        }
        return fallback;
    }

    private IEnumerator GetReadyToShoot()
    {
        anim.Play("ReadyToShoot");
        yield return new WaitForSeconds(1.5f);
        if (DistanceCheck())
        {
            yield break;
        }
        StartCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        if (mobMovement.target != null)
        {
            anim.Play("Shoot");
            var _projectile = Instantiate(ItemObjectArray.Instance.pfProjectile, transform.position, Quaternion.identity);
            var vel = _projectile.GetComponent<Rigidbody2D>().velocity = (mobMovement.target.transform.position - transform.position) * 2;
            _projectile.GetComponent<ProjectileManager>().SetProjectile(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("SkirmisherProjectile"), amount = 1 }, transform.position, gameObject, vel, false, true);
            _projectile.GetComponent<CircleCollider2D>().radius = .5f;
            yield return new WaitForSeconds(.5f);
        }
        attacking = false;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
    }
}
