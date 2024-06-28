using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CoolingRackBehavior : MonoBehaviour
{
    RealWorldObject obj;
    private Queue<string> itemTypesQueue = new Queue<string>();
    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.hasSpecialInteraction = true;
        obj.interactEvent.AddListener(ReceiveItem);

        obj.hoverBehavior.SpecialCase = true;
        obj.hoverBehavior.specialCaseModifier.AddListener(CheckState);

        obj.onLoaded += OnLoad;
    }

    private void ReceiveItem()
    {
        if (obj.playerMain.hasTongs && obj.playerMain.equippedHandItem.heldItem != null && obj.playerMain.equippedHandItem.heldItem.isHot)
        {
            StartCoroutine(CoolItem());
        }
    }

    private void CheckState()
    {
        if (obj.playerMain.hasTongs && obj.playerMain.equippedHandItem.heldItem != null && obj.playerMain.equippedHandItem.heldItem.isHot)
        {
            obj.hoverBehavior.Prefix = $"RMB: Hang {obj.playerMain.equippedHandItem.heldItem.itemSO.itemName}";
            obj.hoverBehavior.Name = "";
        }
        else
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
    }

    private IEnumerator CoolItem()
    {
        Item _item = obj.playerMain.equippedHandItem.heldItem;
        itemTypesQueue.Enqueue(_item.itemSO.itemType);
        obj.saveData.invItemTypes = itemTypesQueue.ToList();
        var _time = obj.playerMain.equippedHandItem.heldItem.remainingTime;
        obj.playerMain.equippedHandItem.heldItem = null;
        obj.playerMain.RemoveContainedItem();
        yield return new WaitForSeconds(_time/2);
        SpitOutItem(_item);
        itemTypesQueue.Dequeue();
        obj.saveData.invItemTypes = itemTypesQueue.ToList();
    }

    private void SpitOutItem(Item _item)
    {
        var _realItem = RealItem.SpawnRealItem(transform.position, _item, true, false, 0, false, true, true);
        _item.StopBeingHot();
        CalebUtils.RandomDirForceNoYAxis3D(_realItem.GetComponent<Rigidbody>(), 5);
    }

    private void OnLoad(object sender, System.EventArgs e)
    {
        if (obj.saveData.invItemTypes.Count > 0)
        {
            int i = 0;
            foreach (var item in obj.saveData.invItemTypes)
            {
                SpitOutItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(obj.saveData.invItemTypes[i]), amount = 1});
                i++;
            }
        }
    }
}
