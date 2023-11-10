using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BunnyAI : MonoBehaviour
{
    private MobMovementBase mobMovement;

    private void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
    }

    private void Update()//add go home function to go back into rabbit hole when close enough or when sunset/night, or after certain amount of time
    {
        var _targetList = Physics2D.OverlapCircleAll(transform.position, GetComponent<MobFleeAI>().predatorDetectionRadius);
        if (mobMovement.currentMovement != MobMovementBase.MovementOption.MoveAway)//if not fleeing, chase bait
        {
            FindBait(_targetList);
        }
    }

    private void FindBait(Collider2D[] _targetList)
    {
        foreach (Collider2D _target in _targetList)
        {
            if (_target.CompareTag("Item"))
            {
                Item _item = _target.GetComponent<RealItem>().GetItem();
                if (_item.itemSO == ItemObjectArray.Instance.SearchItemList("WildCarrot") || _item.itemSO == ItemObjectArray.Instance.SearchItemList("lumbnut"))//change to use bait list maybe
                {
                    mobMovement.SwitchMovement(MobMovementBase.MovementOption.Chase);//will chase but will still flee if predator is nearby
                    mobMovement.trueTarget = _target.transform.position;
                    return;
                }
            }
        }
    }
}
