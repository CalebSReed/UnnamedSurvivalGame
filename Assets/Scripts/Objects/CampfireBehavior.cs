using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class CampfireBehavior : MonoBehaviour
{
    public Item item;
    public bool isCooking;
    private int currentCookingTime = 0;
    private int cookingTimeRequired = 2;//change to get roasting time from item class probs
    private Inventory inventory;
    private RealWorldObject obj;

    public event EventHandler OnFinishedCooking;

    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.interactEvent.AddListener(ReceiveItem);
        obj.hoverBehavior.SpecialCase = true;
        obj.hoverBehavior.specialCaseModifier.AddListener(CheckItems);
    }

    private void CheckItems()
    {
        Item potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;

        if (potentialItem == null)
        {
            obj.hoverBehavior.Prefix = $"";
            obj.hoverBehavior.Name = obj.woso.objName;
            return;
        }

        if (potentialItem.itemSO.isCookable && Vector3.Distance(obj.playerMain.transform.position, transform.position) <= obj.playerMain.collectRange && !isCooking)
        {
            obj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Cook {obj.playerMain.heldItem}";
            obj.hoverBehavior.Name = "";
        }
        else if (potentialItem.itemSO.isFuel && obj.actionsLeft < obj.woso.maxUses)
        {
            obj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Add Fuel";
            obj.hoverBehavior.Name = "";
        }
        else
        {
            obj.hoverBehavior.Prefix = $"";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
    }

    public void StartCooking(Item _item, Inventory _inventory)
    {
        obj.storedItemRenderer.sprite = _item.itemSO.itemSprite;
        item = new Item { itemSO = _item.itemSO, amount = 1 };
        currentCookingTime = 0;
        isCooking = true;
        StartCoroutine(Cook());
        inventory = _inventory;
    }

    private IEnumerator Cook()
    {
        yield return new WaitForSeconds(1f);
        currentCookingTime++;
        if (currentCookingTime >= cookingTimeRequired)
        {
            for (int i = 0; i < inventory.GetItemList().Length; i++)
            {
                if (inventory.GetItemList()[i] != null)
                {
                    if (inventory.GetItemList()[i].itemSO.itemType == item.itemSO.itemType)
                    {
                        inventory.RemoveItemBySlot(i);
                        break;
                    }
                }
            }
            item.itemSO = item.itemSO.cookingReward;
            item.amount = 1;
            if (GameManager.Instance.isServer)
            {
                item.SaveData();
                RealItem newItem = RealItem.SpawnRealItem(transform.position, item.itemData);
                CalebUtils.RandomDirForceNoYAxis3D(newItem.GetComponent<Rigidbody>(), 5);
            }
            else
            {
                ClientHelper.Instance.AskToSpawnItemBasicRPC(transform.position, item.itemSO.itemType, true);
            }

            item = null;
            isCooking = false;
            obj.storedItemRenderer.sprite = null;
            OnFinishedCooking?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            StartCoroutine(Cook());
        }
    }

    public void ReceiveItem()
    {
        Item potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;

        if (potentialItem == null)
        {
            return;
        }

        if (potentialItem.itemSO.isCookable && Vector3.Distance(obj.playerMain.transform.position, transform.position) <= obj.playerMain.collectRange && !isCooking)
        {
            StartCooking(potentialItem, obj.inventory);
            PlayerMain.Instance.inventory.SubtractItem(UI_ItemSlotController.Instance.selectedItemSlot.itemSlotNumber);
        }
        else if (potentialItem.itemSO.isFuel && obj.actionsLeft < obj.woso.maxUses)
        {
            obj.ReplenishUses(potentialItem.itemSO.fuelValue);

            if (obj.woso.objType == "firepit")
            {
                obj.ReplenishUses(potentialItem.itemSO.fuelValue);
            }

            PlayerMain.Instance.inventory.SubtractItem(UI_ItemSlotController.Instance.selectedItemSlot.itemSlotNumber);
        }
    }
    private void OnDestroy()
    {
        if (isCooking)
        {
            if (GameManager.Instance.isServer)
            {
                item.SaveData();
                RealItem newItem = RealItem.SpawnRealItem(transform.position, item.itemData);
                CalebUtils.RandomDirForceNoYAxis3D(newItem.GetComponent<Rigidbody>(), 5);
            }
            else
            {
                ClientHelper.Instance.AskToSpawnItemBasicRPC(transform.position, item.itemSO.itemType, true);
            }
        }
        obj.receiveEvent.RemoveListener(ReceiveItem);
    }
}
