﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ParasiteScouterAI : MonoBehaviour
{
    private float scoutingRadius;

    private Vector3 tempPlayerBase;

    public bool readyToGoHome;

    private MobMovementBase mobMovement;

    private GameObject researchTarget = null;

    private Coroutine researchCoroutine = null;

    private int requiredBasePoints = 100;//change this later

    private bool ignoreAttacks;

    private bool currentlyResearching;

    private bool followingPlayer = false;

    private GameObject player;

    private RealMob realMob;

    public void Start()
    {
        player = ParasiteFactionManager.Instance.player;
        tempPlayerBase = Vector3.zero;
        realMob = GetComponent<RealMob>();
        scoutingRadius = GetComponent<RealMob>().mob.mobSO.scoutingRadius;
        mobMovement = GetComponent<MobMovementBase>();
        mobMovement.OnWander += WanderToPlayer;
        GetComponent<MobNeutralAI>().OnAggroed += GetHit;
    }

    public void Update()
    {
        if (readyToGoHome)
        {
            ignoreAttacks = true;
            mobMovement.ignoreFleeingOverride = false;
            GetComponent<MobFleeAI>().escapeRadius = 999;//run forever boy!
            GetComponent<MobFleeAI>().predatorDetectionRadius = 999;
            CheckToDespawn();
            return;//if base exists, we should just leave and go home. later change to if base is far enough away from this parasite or object or sumn idk
        }

        if (mobMovement.currentMovement != MobMovementBase.MovementOption.DoNothing && researchTarget == null)//should be in this only if researching i think this check is redudant tho
        {
            mobMovement.ignoreFleeingOverride = false;
            if (researchCoroutine != null)
            {
                //print("STOP IT!!!");
                currentlyResearching = false;
                //researchTarget = null;
                StopCoroutine(researchCoroutine);
            }
        }

        if (mobMovement.currentMovement == MobMovementBase.MovementOption.MoveTowards && researchTarget == null)
        {
            FindManMadeObjects();
        }
        else if (researchTarget != null && !currentlyResearching)
        {
            //print("researching!");
            mobMovement.wanderTarget = researchTarget.transform.position;
            TryToResearch();
        }
    }

    private void WanderToPlayer(object sender, System.EventArgs e)
    {
        if (!followingPlayer && Vector3.Distance(transform.position, player.transform.position) > 150)//if not, then switch and follow. also if too close wander random again
        {
            followingPlayer = true;
            mobMovement.wanderTarget = player.transform.position;
            StartCoroutine(PlayerFollowCooldown());
        }
        else//if already true do nothing and switch to false;
        {
            followingPlayer = false;
        }
    }

    private IEnumerator PlayerFollowCooldown()
    {
        yield return new WaitForSeconds(3f);//feel free to balance later
        mobMovement.wanderTarget = transform.position;
    }

    private void CheckToDespawn()
    {
        if (Vector3.Distance(transform.position, mobMovement.target.transform.position) > 200f)
        {
            if (tempPlayerBase != Vector3.zero && readyToGoHome)
            {
                ParasiteFactionManager.parasiteData.PlayerBase = tempPlayerBase;//do this when we despawn? so u have a chance to kill parasite while its running
                ParasiteFactionManager.parasiteData.PlayerBaseExists = true;
                JournalNoteController.Instance.UnlockSpecificEntry("Base Found");
            }
            GetComponent<RealMob>().Die(false);
        }
    }

    private void GetHit(object sender, CombatArgs e)
    {
        if (researchCoroutine != null)
        {
            currentlyResearching = false;
            researchTarget = null;
            StopCoroutine(researchCoroutine);
        }
        mobMovement.ignoreFleeingOverride = true;//make false again 
    }

    private void FindManMadeObjects()
    {
        Collider[] _targetList = Physics.OverlapSphere(realMob.sprRenderer.bounds.center, scoutingRadius);
        _targetList.OrderBy((d) => Vector3.Distance(d.transform.position, transform.position));

        foreach (Collider _target in _targetList)
        {
            if (_target.GetComponentInParent<RealWorldObject>() != null)
            {
                if (_target.GetComponentInParent<RealWorldObject>().obj.woso.isPlayerMade && !IsAlreadyResearched(_target.gameObject))
                {
                    if (Vector3.Distance(_target.transform.position, ParasiteFactionManager.parasiteData.PlayerBase) > 500f || !ParasiteFactionManager.parasiteData.PlayerBaseExists)
                    {
                        mobMovement.wanderTarget = _target.transform.position;
                        researchTarget = _target.gameObject;
                        //mobMovement.target = _target.gameObject;
                        //mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);
                        return;
                    }
                }
            }
        }
    }

    private void TryToResearch()
    {
        if (Vector3.Distance(transform.position, researchTarget.transform.position) <= 8)
        {
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
            mobMovement.ignoreFleeingOverride = true;
            researchCoroutine = StartCoroutine(GainResearchPoints());
        }
    }

    private IEnumerator GainResearchPoints()
    {
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.DoNothing);
        currentlyResearching = true;
        int _prog = 0;
        while (_prog <= 5)
        {
            yield return new WaitForSeconds(1);
            if (researchTarget != null && Vector3.Distance(transform.position, researchTarget.transform.position) > 8)
            {
                mobMovement.SwitchMovement(MobMovementBase.MovementOption.Wait);
                researchTarget = null;
                currentlyResearching = false;
                yield break;
            }
            _prog++;
        }
        Debug.Log("research done :3");
        ParasiteFactionManager.parasiteData.basePoints += researchTarget.GetComponentInParent<RealWorldObject>().obj.woso.basePoints;
        ParasiteFactionManager.Instance.researchedObjectList.Add(researchTarget.gameObject);
        if (requiredBasePoints > ParasiteFactionManager.parasiteData.basePoints)
        {
            researchTarget = null;
            mobMovement.SwitchMovement(MobMovementBase.MovementOption.Wait);
        }
        else
        {
            researchTarget = null;
            mobMovement.target = GameObject.FindGameObjectWithTag("Player");//should always default to player
            CalculateBasePosition();
        }
        currentlyResearching = false;
    }

    private bool IsAlreadyResearched(GameObject _target)
    {
        foreach (GameObject _object in ParasiteFactionManager.Instance.researchedObjectList)
        {
            if (_target == _object)
            {
                return true;
            }
        }
        return false;
    }

    private void CalculateBasePosition()
    {
        Vector3 _tempPos = Vector3.zero;
        foreach (GameObject _object in ParasiteFactionManager.Instance.researchedObjectList)
        {
            if (_object != null)
            {
                _tempPos += _object.transform.position;
            }
        }
        _tempPos /= ParasiteFactionManager.Instance.researchedObjectList.Count;
        _tempPos.y = 0;//change if we add altitudes
        tempPlayerBase = _tempPos;
        readyToGoHome = true;
        ParasiteFactionManager.Instance.researchedObjectList.Clear();
        ParasiteFactionManager.parasiteData.basePoints = 0;
        ParasiteFactionManager.Instance.GoHomeScouters();
    }
}
