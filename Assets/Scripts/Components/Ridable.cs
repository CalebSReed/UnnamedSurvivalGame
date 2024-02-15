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
    }

    private void OnInteract()
    {
        if (mob.mobSaveData.isRidable)
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
                GetSaddled(mob.player.heldItem.itemSO.itemSprite);
                mob.player.UseHeldItem();
                return;
            }
        }
    }

    private void GetSaddled(Sprite spr)
    {
        mob.mobSaveData.isRidable = true;
        mob.heldItem.sprite = spr;
    }
}
