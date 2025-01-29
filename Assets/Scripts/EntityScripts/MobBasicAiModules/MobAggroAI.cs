using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Unity.Netcode;

public class MobAggroAI : MonoBehaviour//we should decide whether or not if this mob wants to prioritize attacking? or fleeing? maybe fleeing by default? seems pretty balanced to me
{
    private MobMovementBase mobMovement;

    private RealMob realMob;
    public float preyDetectionRadius { get; set; }
    public float abandonRadius { get; set; }
    public float combatRadius { get; set; }
    public List<string> preyList { get; set; }

    public CombatArgs combatArgs = new CombatArgs();

    public event EventHandler<CombatArgs> StartCombat;
    public bool inCombat { get; set; }
    //private PlayerMain player;
    public event EventHandler onAggro;
    public event EventHandler onDeaggro;

    private void Awake()
    {
        
    }

    private void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();

        SetFields();
    }

    public void SetFields()
    {
        preyDetectionRadius = realMob.mob.mobSO.preyDetectionRadius;
        abandonRadius = realMob.mob.mobSO.abandonRadius;
        combatRadius = realMob.mob.mobSO.combatRadius;
        preyList = realMob.mob.mobSO.prey;
    }

    private void Update()
    {
        if (mobMovement.currentMovement == realMob.mob.mobSO.aggroStrategy || mobMovement.currentMovement == MobMovementBase.MovementOption.Chase)
        {
            CheckToInitiateCombat();
            CheckToAbandonPrey();
        }
        else if (mobMovement.currentMovement != MobMovementBase.MovementOption.MoveAway && mobMovement.currentMovement != MobMovementBase.MovementOption.DoNothing)//if we are fleeing, dont bother looking for prey. I think its safe to assume all mobs should think like this
        {
            FindPrey();
        }
    }

    private void CheckToInitiateCombat()//shouldnt have to switch target here...
    {
        //bool wallFound = false;
        Collider[] _targetList = Physics.OverlapSphere(transform.position, combatRadius);

        foreach (Collider _target in _targetList)
        {
            if (!_target.isTrigger)
            {
                continue;
            }
            if (CalebUtils.GetParentOfTriggerCollider(_target) == mobMovement.target)
            {
                StartCombat?.Invoke(this, combatArgs);
                return;
            }
        }
    }

    private void FindPrey()
    {
        Collider[] _targetList = Physics.OverlapSphere(transform.position, preyDetectionRadius);
        bool wallFound = false;
        GameObject wallTarget = gameObject;
        foreach (Collider _target in _targetList)
        {
            if (!_target.isTrigger)
            {
                continue;
            }
            if (_target.GetComponentInParent<RealMob>() != null)
            {
                var _realMob = _target.GetComponentInParent<RealMob>();
                foreach (string _tag in preyList)
                {
                    if (_realMob.mob.mobSO.mobType == _tag)//if mobType = _tag in prey list
                    {
                        AggroTarget(_target.transform.parent.gameObject);
                        return;
                    }
                }
            }
            else if (_target.GetComponentInParent<PlayerMain>() != null)//if is a player
            {
                foreach (string _tag in preyList)
                {
                    if (_target.GetComponentInParent<PlayerMain>() != null && _tag == "Player")
                    {
                        AggroTarget(_target.transform.parent.gameObject);
                        if (!isInEnemyList())
                        {
                            _target.transform.root.GetComponent<PlayerMain>().enemyList.Add(gameObject);
                        }
                        if (_target.transform.root.GetComponent<PlayerMain>().IsLocalPlayer)
                        {
                            MusicManager.Instance.PlayBattleMusic();
                        }
                        return;
                    }
                }
            }
            else if (_target.GetComponentInParent<RealWorldObject>() != null && _target.GetComponentInParent<RealWorldObject>().obj.woso.isPlayerMade
                && _target.GetComponentInParent<RealWorldObject>().woso.isContainer)
            {
                foreach (string _tag in preyList)
                {
                    if (_tag == "PlayerLoot")
                    {
                        AggroTarget(_target.transform.parent.gameObject);
                        if (isInEnemyList())
                        {
                            _target.transform.root.GetComponent<PlayerMain>().enemyList.Remove(gameObject);
                        }
                    }
                }
            }
            else if (_target.GetComponentInParent<RealWorldObject>() != null
                && _target.GetComponentInParent<RealWorldObject>().obj.woso.isPlayerMade
                && !_target.GetComponentInParent<RealWorldObject>().woso.isCWall 
                && !_target.GetComponentInParent<RealWorldObject>().woso.isParasiteMade)//if is a playermade object, also dont target walls if u dont have to
            {
                foreach (string _tag in preyList)
                {
                    if (_tag == "PlayerMade")
                    {
                        wallTarget = _target.transform.parent.gameObject;//i think wallfound should be renamed to playerobject found but idr writing this code so idk
                        wallFound = true;
                    }
                }
            }
            else if (_target.GetComponentInParent<RealItem>() != null) //later on sort by rarity / valuableness? We'll need to store that value in the item class...
            {
                foreach (string _tag in preyList)
                {
                    if (_tag == "PlayerLoot")
                    {
                        AggroTarget(_target.transform.parent.gameObject);
                        if (isInEnemyList())
                        {
                            _target.transform.root.GetComponent<PlayerMain>().enemyList.Remove(gameObject);
                        }
                    }
                }
            }
            else if (_target.GetComponentInParent<RealWorldObject>() != null && _target.GetComponentInParent<RealWorldObject>().woso.isPlayerMade && _target.GetComponentInParent<RealWorldObject>().woso.isCWall)
            {
                foreach (string _tag in preyList)
                {
                    if (_tag == "PlayerWalls")
                    {
                        AggroTarget(_target.transform.parent.gameObject);
                        if (isInEnemyList())
                        {
                            _target.transform.root.GetComponent<PlayerMain>().enemyList.Remove(gameObject);
                        }
                    }
                }
            }
               
        }
        if (wallFound)
        {
            AggroTarget(wallTarget);
            if (isInEnemyList())
            {
                GameManager.Instance.localPlayer.GetComponent<PlayerMain>().enemyList.Remove(gameObject);
            }
        }
    }

    private void AggroTarget(GameObject target)
    {
        mobMovement.target = target;
        combatArgs.combatTarget = target;
        mobMovement.SwitchMovement(mobMovement.realMob.mob.mobSO.aggroStrategy);
        onAggro?.Invoke(this, EventArgs.Empty);
    }

    private void CheckToAbandonPrey()
    {
        Collider[] _targetList = Physics.OverlapSphere(transform.position, abandonRadius);

        foreach (Collider _target in _targetList)
        {
            if (!_target.isTrigger)
            {
                continue;
            }
            else if (CalebUtils.GetParentOfTriggerCollider(_target) == mobMovement.target)
            {
                return;
            }
        }
        if (mobMovement.target.tag == "Player")
        {
            mobMovement.target.transform.root.GetComponent<PlayerMain>().enemyList.Remove(gameObject);
        }
        if (isInEnemyList())
        {
            GameManager.Instance.localPlayer.GetComponent<PlayerMain>().enemyList.Remove(gameObject);
        }
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.Wait);
        onDeaggro?.Invoke(this, EventArgs.Empty);

    }

    private bool isInEnemyList()
    {
        foreach (GameObject obj in GameManager.Instance.localPlayer.GetComponent<PlayerMain>().enemyList)
        {
            if (obj == gameObject)
            {
                return true;
            }
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)//attack walls when u bump into them only
    {
        if (collision.collider.GetComponent<RealWorldObject>() != null && collision.collider.GetComponent<RealWorldObject>().woso.isCWall && collision.collider.GetComponent<RealWorldObject>().woso.isPlayerMade)
        {
            mobMovement.target = collision.gameObject;
            combatArgs.combatTarget = collision.gameObject;
            StartCombat?.Invoke(this, combatArgs);
            if (isInEnemyList())
            {
                GameManager.Instance.localPlayer.GetComponent<PlayerMain>().enemyList.Remove(gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject obj in GameManager.Instance.localPlayer.GetComponent<PlayerMain>().enemyList)
        {
            if (obj == gameObject)
            {
                GameManager.Instance.localPlayer.GetComponent<PlayerMain>().enemyList.Remove(obj);
                return;
            }
        }
    }
}
