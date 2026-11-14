using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ridable : MonoBehaviour
{
    RealMob mob;

    private void Awake()
    {
        mob = GetComponent<RealMob>();

        //mob.receiveEvent.AddListener(RecieveItems);
        mob.interactEvent.AddListener(OnInteract);
        mob.hasSpecialInteraction = true;

        mob.hoverBehavior.specialCaseModifier.AddListener(CheckItems);
        mob.hoverBehavior.SpecialCase = true;
    }

    private void OnInteract()
    {
        if (mob.mobSaveData.isRidable && GameManager.Instance.localPlayerMain.StateMachine.currentPlayerState != GameManager.Instance.localPlayerMain.deadState)
        {
            GameManager.Instance.localPlayerMain.RideCreature(mob);
            return;
        }

        Item potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;

        if (potentialItem == null)
        {
            return;
        }
        foreach (var item in mob.mob.mobSO.acceptableItems)
        {
            if (item == potentialItem.itemSO)
            {
                GetSaddled(potentialItem.itemSO);
                GameManager.Instance.localPlayerMain.inventory.RemoveItemBySlot(UI_ItemSlotController.Instance.selectedItemSlot.itemSlotNumber);
                return;
            }
        }
    }

    public void GetSaddled(ItemSO item)
    {
        mob.mobSaveData.isRidable = true;
        mob.mobSaveData.saddle = item.itemType;
        mob.heldItem.sprite = item.itemSprite;
    }

    private void CheckItems()
    {
        if (mob.mobSaveData.isRidable)
        {
            mob.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Ride ";
            mob.hoverBehavior.Name = mob.mob.mobSO.mobName;
            return;
        }

        Item potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;

        if (potentialItem == null)
        {
            return;
        }

        foreach (var item in mob.mob.mobSO.acceptableItems)
        {
            if (item == potentialItem.itemSO)
            {
                mob.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Place saddle on ";
                mob.hoverBehavior.Name = mob.mob.mobSO.mobName;
                return;
            }
        }

        mob.hoverBehavior.Prefix = "";
        mob.hoverBehavior.Name = mob.mob.mobSO.mobName;
    }
}
