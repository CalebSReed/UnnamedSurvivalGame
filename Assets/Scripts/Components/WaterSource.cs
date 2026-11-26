using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaterSource : MonoBehaviour
{
    RealWorldObject obj;
    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.interactEvent.AddListener(ReceiveWaterContainer);
        obj.hasSpecialInteraction = true;

        //obj.receiveEvent.AddListener(ReceiveWaterContainer);

        obj.hoverBehavior.specialCaseModifier.AddListener(CheckItems);
        obj.hoverBehavior.SpecialCase = true;
    }

    private void ReceiveWaterContainer()
    {
        if (Vector3.Distance(obj.playerMain.transform.position, transform.position) > obj.playerMain.collectRange)
        {
            return;
        }

        var potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;

        if (potentialItem != null)
        {
            if (potentialItem.itemSO.itemType == "ClayBowl")
            {
                if (potentialItem.amount == 1)
                {
                    potentialItem.itemSO = ItemObjectArray.Instance.SearchItemList("BowlOfWater");
                    obj.actionsLeft--;
                    obj.playerMain.inventory.RefreshInventory();
                    obj.CheckBroken(GameManager.Instance.localPlayerMain);
                }
                else if (potentialItem.amount > 1)
                {
                    obj.playerMain.inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("BowlOfWater"), amount = 1 }, transform.position);
                    obj.actionsLeft--;
                    obj.CheckBroken(GameManager.Instance.localPlayerMain);
                    obj.playerMain.inventory.SubtractItem(UI_ItemSlotController.Instance.selectedItemSlot.itemSlotNumber);
                }
                return;
            }
            else if (obj.playerMain.isHandItemEquipped && obj.playerMain.equippedHandItem.itemSO.needsAmmo && obj.playerMain.equippedHandItem.ammo < obj.playerMain.equippedHandItem.itemSO.maxAmmo && obj.playerMain.equippedHandItem.itemSO.doActionType == Action.ActionType.Water)
            {
                while (obj.actionsLeft > 0 && obj.playerMain.equippedHandItem.ammo < obj.playerMain.equippedHandItem.itemSO.maxAmmo)
                {
                    obj.playerMain.equippedHandItem.ammo++;
                    obj.actionsLeft--;
                    obj.playerMain.UpdateEquippedItem(obj.playerMain.equippedHandItem);
                    obj.CheckBroken(GameManager.Instance.localPlayerMain);
                }
                return;
            }
        }
    }

    private void CheckItems()
    {
        var potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;
        if (potentialItem != null)
        {
            if (potentialItem.itemSO.itemType == "ClayBowl")
            {
                obj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Scoop water in ";
                obj.hoverBehavior.Name = potentialItem.itemSO.itemName;
                return;
            }
            else if (potentialItem.itemSO.doActionType == Action.ActionType.Water && potentialItem.itemSO.needsAmmo && potentialItem.ammo < potentialItem.itemSO.maxAmmo)
            {
                obj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Scoop water in ";
                obj.hoverBehavior.Name = potentialItem.itemSO.itemName;
                return;
            }
        }
        else if (obj.playerMain.isHandItemEquipped && obj.playerMain.equippedHandItem.itemSO.needsAmmo && obj.playerMain.equippedHandItem.ammo < obj.playerMain.equippedHandItem.itemSO.maxAmmo && obj.playerMain.equippedHandItem.itemSO.doActionType == Action.ActionType.Water)
        {
            obj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Scoop water in ";
            obj.hoverBehavior.Name = obj.playerMain.equippedHandItem.itemSO.itemName;
            return;
        }
        else
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
    }

    private void OnDestroy()
    {
        obj.interactEvent.RemoveListener(ReceiveWaterContainer);
        obj.receiveEvent.RemoveListener(ReceiveWaterContainer);
    }
}
