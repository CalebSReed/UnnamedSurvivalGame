using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirePitBehavior : MonoBehaviour
{
    RealWorldObject obj;
    PlayerMain player;

    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.interactEvent.AddListener(ReceiveItem);
        player = obj.playerMain;

        obj.hoverBehavior.SpecialCase = true;
        obj.hoverBehavior.specialCaseModifier.AddListener(CheckItems);
    }

    private void CheckItems()
    {
        var potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;

        if (potentialItem == null)
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
            return;
        }
        if (potentialItem.itemSO.isFuel)
        {
            obj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Add Fuel";
            obj.hoverBehavior.Name = "";
        }
        else
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
    }

    private void ReceiveItem()
    {
        var potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;
        if (potentialItem.itemSO.isFuel)
        {
            var realObj = RealWorldObject.SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
            realObj.transform.localScale = new Vector3(1, 1, 1);
            realObj.actionsLeft = potentialItem.itemSO.fuelValue * 2;
            obj.Break(true);
            PlayerMain.Instance.inventory.SubtractItem(UI_ItemSlotController.Instance.selectedItemSlot.itemSlotNumber);
        }
    }
}
