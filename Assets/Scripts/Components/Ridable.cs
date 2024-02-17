using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ridable : MonoBehaviour
{
    RealMob mob;

    private void Awake()
    {
        mob = GetComponent<RealMob>();

        mob.receiveEvent.AddListener(RecieveItems);
        mob.interactEvent.AddListener(OnInteract);
        mob.hasSpecialInteraction = true;

        mob.hoverBehavior.specialCaseModifier.AddListener(CheckItems);
        mob.hoverBehavior.SpecialCase = true;
    }

    private void OnInteract()
    {
        if (mob.mobSaveData.isRidable && mob.player.StateMachine.currentPlayerState != mob.player.deadState)
        {
            mob.player.RideCreature(mob);
        }
    }

    private void RecieveItems()
    {
        foreach (var item in mob.mob.mobSO.acceptableItems)
        {
            if (item == mob.player.heldItem.itemSO)
            {
                GetSaddled(mob.player.heldItem.itemSO);
                mob.player.UseHeldItem();
                return;
            }
        }
    }

    public void GetSaddled(ItemSO item)
    {
        mob.mobSaveData.isRidable = true;
        mob.mobSaveData.saddle = item;
        mob.heldItem.sprite = item.itemSprite;
    }

    private void CheckItems()
    {
        if (mob.mobSaveData.isRidable)
        {
            mob.hoverBehavior.Prefix = "RMB: Ride ";
            mob.hoverBehavior.Name = mob.mob.mobSO.mobName;
            return;
        }

        if (mob.player.isHoldingItem)
        {
            foreach (var item in mob.mob.mobSO.acceptableItems)
            {
                if (item == mob.player.heldItem.itemSO)
                {
                    mob.hoverBehavior.Prefix = "LMB: Place saddle on ";
                    mob.hoverBehavior.Name = mob.mob.mobSO.mobName;
                    return;
                }
            }
        }
        mob.hoverBehavior.Prefix = "";
        mob.hoverBehavior.Name = mob.mob.mobSO.mobName;
    }
}
