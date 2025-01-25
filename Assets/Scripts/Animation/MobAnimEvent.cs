using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class MobAnimEvent : MonoBehaviour
{
    [SerializeField] private RealMob mob;
    [SerializeField] private Rigidbody rb;
    public event EventHandler onAttackEnded;
    public event EventHandler becomeProjectile;
    public event EventHandler unbecomeProjectile;
    public event EventHandler<ComboArgs> SetCombo;
    public event EventHandler<AttackEventArgs> checkAttackConditions;
    public event EventHandler beginHitStun;
    public event EventHandler endHitStun;
    public bool beingComboed;
    private ComboArgs comboArgs;
    bool moving;
    bool jumping;
    float jumpDelta;
    Vector3 dir;
    float speedmult;
    float newY;
    [SerializeField] NetworkManager networkManager;


    private void Start()
    {
        comboArgs = new ComboArgs();
    }

    public void SetFields(RealMob mob, Rigidbody rb)
    {
        this.mob = mob;
        this.rb = rb;
    }

    public void OnHitEnemies(int dmgMult = 1)
    {
        mob.HitEnemies(mob.mob.mobSO.combatRadius, dmgMult);
    }

    public void OnHitEnemiesUnparriable(int dmgMult = 1)
    {
        mob.HitEnemies(mob.mob.mobSO.combatRadius, dmgMult, false, false);
    }

    public void OnDamageEnemiesRadius(AnimationEvent animEvent)
    {
        mob.HitEnemies(mob.mob.mobSO.combatRadius * animEvent.floatParameter, animEvent.intParameter);
    }

    public void OnDamageEnemiesRadiusUnparriable(AnimationEvent animEvent)
    {
        mob.HitEnemies(mob.mob.mobSO.combatRadius * animEvent.floatParameter, animEvent.intParameter, false, false);
    }

    public void SpawnObject(string obj)
    {
        if (!GameManager.Instance.isServer)
        {
            return;
        }
        RealWorldObject.SpawnWorldObject(rb.transform.position, new WorldObject { woso = WosoArray.Instance.SearchWOSOList(obj) });
    }

    public void Jump(AnimationEvent animEvent)
    {
        jumpDelta = -2;
        jumping = true;
        dir = mob.mobMovement.target.transform.position - transform.position;
    }

    public void SetComboCount(int count)
    {
        comboArgs.comboCount = count;
        SetCombo?.Invoke(this, comboArgs);
    }

    public void BeginStun()
    {
        beginHitStun?.Invoke(this, EventArgs.Empty);
    }

    public void EndStun()
    {
        endHitStun?.Invoke(this, EventArgs.Empty);
    }

    public void OnCritWalls(int dmgMult = 1)
    {
        if (mob.mobMovement.target.GetComponent<RealWorldObject>() != null && mob.mobMovement.target.GetComponent<RealWorldObject>().woso.isCWall)
        {
            mob.HitEnemies(mob.mob.mobSO.combatRadius, dmgMult * 10);
        }
        else
        {
            mob.HitEnemies(mob.mob.mobSO.combatRadius, dmgMult);
        }
    }

    public void OnTryGrabItems(int dmgMult = 1)
    {
        if (mob.mobMovement.target != null && mob.mobMovement.target.GetComponent<RealItem>() != null)
        {
            mob.HitEnemies(mob.mob.mobSO.combatRadius, dmgMult, true);
        }
        else if (mob.mobMovement.target != null && mob.mobMovement.target.GetComponent<RealWorldObject>() != null && mob.mobMovement.target.GetComponent<RealWorldObject>().woso.isContainer)
        {
            mob.HitEnemies(mob.mob.mobSO.combatRadius, dmgMult * 10);
        }
        else
        {
            mob.HitEnemies(mob.mob.mobSO.combatRadius, dmgMult);
        }
    }

    public void ShootProjectile(ItemSO item)
    {
        if (!GameManager.Instance.isServer)
        {
            return;
        }
        if (mob.mobMovement.target != null)
        {
            var _projectile = Instantiate(ItemObjectArray.Instance.pfProjectile, transform.position, Quaternion.identity);
            _projectile.position = new Vector3(_projectile.position.x, transform.position.y + 1, _projectile.position.z);
            var vel = _projectile.GetComponent<Rigidbody>().velocity = (mob.mobMovement.target.transform.position - transform.position).normalized * 100;
            vel.y = 1;
            _projectile.GetComponent<ProjectileManager>().SetProjectile(new Item { itemSO = item, amount = 1 }, transform.position, mob.gameObject, vel, false, true);
            //_projectile.GetComponent<CapsuleCollider>().radius = .5f; capsule collider now
            _projectile.GetChild(0).gameObject.AddComponent<BillBoardBehavior>();
            _projectile.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void OnDisableMovement()
    {
        mob.mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        moving = false;
    }

    public void GainSuperArmor()
    {
        mob.willStun = false;
    }

    public void BecomeProjectile()
    {
        becomeProjectile?.Invoke(this, EventArgs.Empty);
    }

    public void UnBecomeProjectile()
    {
        unbecomeProjectile?.Invoke(this, EventArgs.Empty);
    }

    public void ResumeChasing()
    {
        if (beingComboed)
        {
            Debug.Log("still being comboed...");
            onAttackEnded?.Invoke(this, EventArgs.Empty);
            return;
        }

        Debug.Log("goin back now");

        if (mob.mob.mobSO.aggroType == MobAggroType.AggroType.Aggressive)
        {
            mob.mobMovement.SwitchMovement(mob.mob.mobSO.aggroStrategy);
        }
        else
        {
            mob.mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveAway);
        }
        onAttackEnded?.Invoke(this, EventArgs.Empty);
    }

    public void CheckAttackConditions(AnimationEvent animEvent)
    {
        var args = new AttackEventArgs();
        args.checkType = animEvent.stringParameter;
        checkAttackConditions?.Invoke(this, args);
    }

    public void MoveTowardsTarget(AnimationEvent animEvent)
    {
        dir = mob.mobMovement.target.transform.position - transform.position;
        mob.transform.LookAt(mob.mobMovement.target.transform.position, Vector3.up);
        speedmult = animEvent.floatParameter;
        moving = true;
    }

    public void StopMovingTowardsTarget()
    {
        moving = false;
    }

    private void Update()
    {
        if (jumping)
        {
            jumpDelta += Time.deltaTime * 3.5f;
            newY = -Mathf.Pow(jumpDelta, 2) * 2 + 10f;
            if (mob.etherTarget == true)
            {
                newY += 250;
            }
            transform.position = new Vector3(rb.transform.position.x, newY, rb.transform.position.z);
            if (jumpDelta > 2.25f)
            {
                jumping = false;
                ResetYLevel();
            }

            if (jumping)
            {
                dir.y = 0;
            }
            rb.transform.position = transform.position + dir * Time.deltaTime;
        }
        if (moving)
        {

            rb.transform.position = transform.position + dir.normalized * mob.mob.mobSO.walkSpeed * speedmult * Time.deltaTime;
        }

    }

    private int ResetYLevel()
    {
        if (mob.etherTarget == true)
        {
            rb.transform.position = new Vector3(rb.transform.position.x, 250, rb.transform.position.z);
            return 250;
        }
        else
        {
            rb.transform.position = new Vector3(rb.transform.position.x, 0, rb.transform.position.z);
            return 0;
        }
    }
}
