using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (obj.playerMain.isHoldingItem && obj.playerMain.heldItem.itemSO.itemType == "ClayBowl" && Vector3.Distance(obj.playerMain.transform.position, transform.position) <= 10)
        {
            obj.playerMain.UseHeldItem();
            obj.playerMain.inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("sandbowl"), amount = 1}, obj.playerMain.transform.position);
            obj.actionsLeft--;
            obj.CheckBroken(GameManager.Instance.localPlayerMain);
        }
    }

    private void CheckItems()
    {
        if (obj.playerMain.isHoldingItem && obj.playerMain.heldItem.itemSO.itemType == "ClayBowl" && Vector3.Distance(obj.playerMain.transform.position, transform.position) <= 10)
        {
            obj.hoverBehavior.Prefix = "LMB: Scoop sand in ";
            obj.hoverBehavior.Name = obj.playerMain.heldItem.itemSO.itemName;
        }
        else
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
    }
}
