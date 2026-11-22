using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SandSource : MonoBehaviour
{
    RealWorldObject obj;

    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.interactEvent.AddListener(ReceiveBowl);
        obj.hasSpecialInteraction = true;

        obj.receiveEvent.AddListener(ReceiveBowl);
        obj.hoverBehavior.specialCaseModifier.AddListener(CheckItems);
        obj.hoverBehavior.SpecialCase = true;
    }

    private void ReceiveBowl()
    {
        var potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;
        if (potentialItem != null && potentialItem.itemSO.itemType == "ClayBowl" && Vector3.Distance(obj.playerMain.transform.position, transform.position) <= 10)
        {
            PlayerMain.Instance.inventory.SubtractItem(UI_ItemSlotController.Instance.selectedItemSlot.itemSlotNumber);
            obj.playerMain.inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("sandbowl"), amount = 1}, obj.playerMain.transform.position);
            obj.actionsLeft--;
            obj.CheckBroken(GameManager.Instance.localPlayerMain);
        }
    }

    private void CheckItems()
    {
        var potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;
        if (potentialItem != null && potentialItem.itemSO.itemType == "ClayBowl" && Vector3.Distance(obj.playerMain.transform.position, transform.position) <= 10)
        {
            obj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Scoop sand in ";
            obj.hoverBehavior.Name = obj.playerMain.heldItem.itemSO.itemName;
        }
        else
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
    }
}
