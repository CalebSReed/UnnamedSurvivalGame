using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePitBehavior : MonoBehaviour
{
    RealWorldObject obj;
    PlayerMain player;

    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.receiveEvent.AddListener(ReceiveItem);
        player = obj.playerMain;

        obj.hoverBehavior.SpecialCase = true;
        obj.hoverBehavior.specialCaseModifier.AddListener(CheckItems);
    }

    private void CheckItems()
    {
        if (!player.isHoldingItem)
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
            return;
        }
        if (player.heldItem.itemSO.isFuel)
        {
            obj.hoverBehavior.Prefix = "LMB: Add Fuel";
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
        if (player.heldItem.itemSO.isFuel)
        {
            var realObj = RealWorldObject.SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
            realObj.transform.localScale = new Vector3(1, 1, 1);
            realObj.actionsLeft = player.heldItem.itemSO.fuelValue;
            obj.Break(true);
            player.UseHeldItem();
        }
    }
}
