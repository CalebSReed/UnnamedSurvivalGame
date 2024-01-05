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
    }

    private void ReceiveBowl()
    {
        if (obj.playerMain.isHoldingItem && obj.playerMain.heldItem.itemSO.itemType == "ClayBowl" && Vector3.Distance(obj.playerMain.transform.position, transform.position) <= 10)
        {
            obj.playerMain.UseHeldItem();
            obj.playerMain.inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("sandbowl"), amount = 1}, obj.playerMain.transform.position);
            obj.actionsLeft--;
            obj.CheckBroken();
        }
    }
}
