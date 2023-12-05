using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSource : MonoBehaviour
{
    RealWorldObject obj;
    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.interactEvent.AddListener(ReceiveWaterContainer);
        obj.hasSpecialInteraction = true;
    }

    private void ReceiveWaterContainer()
    {
        if (Vector3.Distance(obj.playerMain.transform.position, transform.position) > obj.playerMain.collectRange)
        {
            return;
        }

        if (obj.playerMain.isHoldingItem)
        {
            if (obj.playerMain.heldItem.itemSO.itemType == "ClayBowl")
            {
                if (obj.playerMain.heldItem.amount == 1)
                {
                    obj.playerMain.heldItem.itemSO = ItemObjectArray.Instance.SearchItemList("BowlOfWater");
                    obj.actionsLeft--;
                    obj.CheckBroken();
                }
                else if (obj.playerMain.heldItem.amount > 1)
                {
                    obj.playerMain.inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("BowlOfWater"), amount = 1 }, transform.position);
                    obj.actionsLeft--;
                    obj.CheckBroken();
                    obj.playerMain.UseHeldItem();
                }
                return;
            }
            else if (obj.playerMain.heldItem.itemSO.doActionType == Action.ActionType.Water && obj.playerMain.heldItem.itemSO.needsAmmo && obj.playerMain.heldItem.ammo < obj.playerMain.heldItem.itemSO.maxAmmo)
            {
                while (obj.actionsLeft > 0 || obj.playerMain.heldItem.ammo < obj.playerMain.heldItem.itemSO.maxAmmo)
                {
                    obj.playerMain.heldItem.ammo++;
                    obj.actionsLeft--;
                    obj.playerMain.UpdateHeldItemStats();
                    obj.CheckBroken();                 
                }
                return;
            }
        }
        else if (obj.playerMain.isHandItemEquipped && obj.playerMain.equippedHandItem.itemSO.needsAmmo && obj.playerMain.equippedHandItem.ammo < obj.playerMain.equippedHandItem.itemSO.maxAmmo)
        {
            while (obj.actionsLeft > 0 || obj.playerMain.equippedHandItem.ammo < obj.playerMain.equippedHandItem.itemSO.maxAmmo)
            {
                obj.playerMain.equippedHandItem.ammo++;
                obj.actionsLeft--;
                obj.playerMain.UpdateEquippedItem(obj.playerMain.equippedHandItem, obj.playerMain.handSlot);
                obj.CheckBroken();
            }
            return;
        }
    }

    private void OnDestroy()
    {
        obj.interactEvent.RemoveListener(ReceiveWaterContainer);
    }
}
