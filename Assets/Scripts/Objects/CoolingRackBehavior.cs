using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

public class CoolingRackBehavior : NetworkBehaviour
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
        if (GameManager.Instance.localPlayerMain.hasTongs && GameManager.Instance.localPlayerMain.equippedHandItem.heldItem != null && GameManager.Instance.localPlayerMain.equippedHandItem.heldItem.isHot)
        {
            if (GameManager.Instance.isServer)
            {
                StartCoroutine(CoolItem(GameManager.Instance.localPlayerMain.equippedHandItem.heldItem));
                GameManager.Instance.localPlayerMain.equippedHandItem.heldItem = null;
                GameManager.Instance.localPlayerMain.RemoveContainedItem();
            }
            else
            {
                int[] newTypes = null;
                if (GameManager.Instance.localPlayerMain.equippedHandItem.heldItem.containedItems != null)
                {
                    newTypes = RealItem.ConvertContainedItemTypes(GameManager.Instance.localPlayerMain.equippedHandItem.heldItem.containedItems);
                }
                AskToCoolItemRPC(GameManager.Instance.localPlayerMain.equippedHandItem.heldItem.itemSO.itemType, GameManager.Instance.localPlayerMain.equippedHandItem.heldItem.remainingTime, newTypes);
                GameManager.Instance.localPlayerMain.equippedHandItem.heldItem = null;
                GameManager.Instance.localPlayerMain.RemoveContainedItem();
            }
        }
    }

    private void CheckState()
    {
        if (GameManager.Instance.localPlayerMain.hasTongs && GameManager.Instance.localPlayerMain.equippedHandItem.heldItem != null && GameManager.Instance.localPlayerMain.equippedHandItem.heldItem.isHot)
        {
            obj.hoverBehavior.Prefix = $"RMB: Hang {GameManager.Instance.localPlayerMain.equippedHandItem.heldItem.itemSO.itemName}";
            obj.hoverBehavior.Name = "";
        }
        else
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
    }

    [Rpc(SendTo.Server)]
    private void AskToCoolItemRPC(string itemType, float time, int[] containedItems = null)
    {
        Item newItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(itemType), remainingTime = time };

        if (containedItems != null)
        {
            newItem.containedItems = new Item[containedItems.Length];
            for (int i = 0; i < containedItems.Length; i++)
            {
                if (containedItems[i] != -1)
                {
                    newItem.containedItems[i] = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(containedItems[i]), amount = 1 };
                }
                else
                {
                    newItem.containedItems[i] = null;
                }
            }
        }
        StartCoroutine(CoolItem(newItem));
    }

    private IEnumerator CoolItem(Item _item)
    {
        itemTypesQueue.Enqueue(_item.itemSO.itemType);
        obj.saveData.invItemTypes = itemTypesQueue.ToList();
        var _time = _item.remainingTime;

        yield return new WaitForSeconds(_time/2);
        SpitOutItem(_item);
        itemTypesQueue.Dequeue();
        obj.saveData.invItemTypes = itemTypesQueue.ToList();
    }

    private void SpitOutItem(Item _item)//We only run on server anyways
    {
        _item.StopBeingHot();

        var _realItem = RealItem.SpawnRealItem(transform.position, _item, true, false, 0, false, true, true);
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
