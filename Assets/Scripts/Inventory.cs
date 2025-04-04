﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using System.Linq;
using Unity.Netcode;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private Item[] itemList; //make list that stores 'Item' types from ITEM class. 'itemList' is a new field, or essentially the actual inventory.

    private int maxItemsAllowed;

    public event EventHandler OnItemListChanged;
    public event EventHandler<InventoryArgs> OnItemAdded;
    public InventoryArgs invArgs = new InventoryArgs();

    //[SerializeField]
    //private GameObject player;

    private TextMeshProUGUI txt;

    private void Start()
    {
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
    }

    public Inventory(int _maxAmount) //setup constructor whatever that is
    {
        //player = GameObject.FindGameObjectWithTag("Player");
        maxItemsAllowed = _maxAmount;
        itemList = new Item[_maxAmount]; //initialize the list
    }
    //bro straight up CLEAN THIS SHIT UP WHEN UR DONE PLEASE
    public void AddItem(Item item, Vector3 returnPos, bool autoEquip = true) //adds 'Item' type of item into list 'itemList'
    {
        invArgs.item = item.itemSO;
        GameObject player = GameManager.Instance.localPlayer;
        int leftoverAmount = item.amount;
        if (item.itemSO.isStackable)
        {
            bool itemAlreadyInInventory = false;
            bool oneOrMoreSlotsAreFull = false;
            bool itemAdded = false;
            Debug.Log(ItemCount());
            if (ItemCount() == 0)
            {
                itemList.SetValue(item, 0);
                OnItemListChanged?.Invoke(this, EventArgs.Empty);
                OnItemAdded?.Invoke(this, invArgs);
                return;
            }

            for (int i = 0; i < itemList.Length; i++)
            {
                if (itemList[i] != null)
                {
                    if (itemList[i].itemSO.itemType == item.itemSO.itemType)//paste here
                    {
                        itemList[i].amount += item.amount;//11+11 = 22
                        itemAlreadyInInventory = true;
                        int invItemTempAmount = itemList[i].amount;
                        if (invItemTempAmount > item.itemSO.maxStackSize && ItemCount() >= maxItemsAllowed)//if itemslot exceeds stack size AND inventory has no extra slots, set amount to max stack size, and spit out remaining amount back into world
                        {
                            //Debug.LogError("wait what " + invItemTempAmount + " temp amount, and item amount: " + item.amount);
                            invItemTempAmount -= item.itemSO.maxStackSize;
                            leftoverAmount = invItemTempAmount;
                            //realItem.RefreshAmount(item);
                            itemList[i].amount = item.itemSO.maxStackSize;
                            //itemSprite.color = new Color(1f, 1f, 1f, 1f);
                            //break;
                        }
                        else if (invItemTempAmount > item.itemSO.maxStackSize && ItemCount() < maxItemsAllowed && !oneOrMoreSlotsAreFull)//if exceeds BUT inventory has extra slots, set first slot amount to max, next slot to remaining, then destroy ingame item
                        {
                            oneOrMoreSlotsAreFull = true;//sets first stack to max size, now keep searching thru the inventory for an un-full stack...
                            invItemTempAmount -= item.itemSO.maxStackSize;
                            item.amount = invItemTempAmount;
                            itemList[i].amount = item.itemSO.maxStackSize;
                        }
                        else if (oneOrMoreSlotsAreFull && itemList[i].itemSO.itemType == item.itemSO.itemType && invItemTempAmount > item.itemSO.maxStackSize)//found the un-full stack, but its too big to fit it all, set to max, but we should keep cycling if we add inventory organization...
                        {
                            //invItemTempAmount += item.amount;
                            //if (invItemTempAmount)
                            itemList[i].amount = item.itemSO.maxStackSize;
                            invItemTempAmount -= item.itemSO.maxStackSize;
                            item.amount = invItemTempAmount;
                            //itemAdded = true;
                            //realItem.DestroySelf();
                            //must keep cycling actually or sumn idk, item limit is goign over
                        }
                        else if (oneOrMoreSlotsAreFull && itemList[i].itemSO.itemType == item.itemSO.itemType && invItemTempAmount <= item.itemSO.maxStackSize)//found a stack that can fit.
                        {
                            itemAdded = true;
                            //realItem.DestroySelf();
                            break;
                        }
                        else//found the first stack that fits i think
                        {
                            //realItem.DestroySelf();
                            itemAdded = true;
                            Debug.Log("Nothing is true");
                            break;
                        }
                    }
                }
            }
            if (oneOrMoreSlotsAreFull && !itemAdded)//if we never found a stack, inv should already not be full i think.... so add the item
            {
                SetValue(item);
                itemAdded = true;
                //realItem.DestroySelf();
            }
            if (!itemAlreadyInInventory && ItemCount() < maxItemsAllowed) //if it doesnt exist, and inventory isnt full, create the new item in inventory
            {
                SetValue(item);
                itemAdded = true;
                //realItem.DestroySelf();
            }
            else if (!itemAlreadyInInventory && ItemCount() >= maxItemsAllowed)//if it doesnt exist, is stackable, and inv IS full, dont add it
            {
                //itemSprite.color = new Color(1f, 1f, 1f, 1f);
                Item itemToDrop = new Item { itemSO = item.itemSO, amount = item.amount, equipType = item.equipType };
                if (GameManager.Instance.isServer)
                {
                    RealItem newItem = RealItem.SpawnRealItem(returnPos, itemToDrop, true, true, item.ammo, false, true);
                    CalebUtils.RandomDirForceNoYAxis3D(newItem.GetComponent<Rigidbody>(), 5);
                }
                else
                {
                    SpitOutItem(itemToDrop, returnPos);
                }
                Debug.Log("inv full");
            }
            else if (leftoverAmount > 0 && !itemAdded)//if we have leftover amounts and if item is not added
            {
                Item itemToDrop = new Item { itemSO = item.itemSO, amount = leftoverAmount, equipType = item.equipType };
                if (GameManager.Instance.isServer)
                {
                    RealItem newItem = RealItem.SpawnRealItem(returnPos, itemToDrop, true, true, item.ammo, false, true);
                    CalebUtils.RandomDirForceNoYAxis3D(newItem.GetComponent<Rigidbody>(), 5);
                }
                else
                {
                    SpitOutItem(itemToDrop, returnPos);
                }
                Debug.Log("SPITTING OUT ITEM");
            }
        }
        else if (!item.itemSO.isStackable && item.itemSO.isEquippable && !player.GetComponent<PlayerMain>().isHandItemEquipped && item.equipType == Item.EquipType.HandGear && autoEquip && ItemCount() <= maxItemsAllowed - 1)
        {       //if equippable, no item is equipped, and not recently unequipped, equip. inv fullness irrelevent
            SetValue(item);
            player.GetComponent<PlayerMain>().EquipItem(item);
            //realItem.DestroySelf();
        }
        else if (ItemCount() <= maxItemsAllowed-1 && !item.itemSO.isStackable)//if not stackable but can fit
        {
            SetValue(item);
            //realItem.DestroySelf();
        }
        else//unstackable and full inventory
        {
            //itemSprite.color = new Color(1f, 1f, 1f, 1f);
            Item itemToDrop = new Item { itemSO = item.itemSO, amount = 1, uses = item.uses, equipType = item.equipType, containedItems = item.containedItems };
            if (GameManager.Instance.isServer)
            {
                RealItem newItem = RealItem.SpawnRealItem(returnPos, itemToDrop, true, true, item.ammo, false, true);//uses are only set in this line, hopefully thats ok
                CalebUtils.RandomDirForceNoYAxis3D(newItem.GetComponent<Rigidbody>(), 5);
            }
            else
            {
                SpitOutItem(itemToDrop, returnPos);
            }
            Debug.Log("inv full");
        }
        OnItemListChanged?.Invoke(this, EventArgs.Empty); //these events remind me of signals from godot...
        OnItemAdded?.Invoke(this, invArgs);
    }

    private void SpitOutItem(Item item, Vector3 returnPos)
    {
        int[] containedItemTypes = null;
        int[] containedItemAmounts = null;

        if (item.containedItems != null)
        {
            containedItemTypes = RealItem.ConvertContainedItemTypes(item.containedItems);
            containedItemAmounts = RealItem.ConvertContainedItemAmounts(item.containedItems);
        }

        string heldItemType = null;
        if (item.heldItem != null)
        {
            heldItemType = item.heldItem.itemSO.itemType;
        }

        ClientHelper.Instance.AskToSpawnItemSpecificRPC(returnPos, false, false, item.itemSO.itemType, item.amount, item.uses, item.ammo, (int)item.itemSO.equipType, item.isHot, item.remainingTime, containedItemTypes, containedItemAmounts, heldItemType, true);
    }

    public void RefreshInventory()
    {
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool InventoryHasOpenSlot()
    {
        if (ItemCount() <= maxItemsAllowed - 1)
        {
            return true;
        }
        return false;
    }

    public int LastItem()
    {
        int itemIndex = 0;
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i] != null)
            {
                itemIndex = i;
            }
        }
        return itemIndex;
    }

    public int ItemCount()
    {
        int itemCount = 0;
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i] != null)
            {
                itemCount++;
            }
        }
        return itemCount;
    }

    public void SetValue(Item _item)
    {
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i] == null)
            {
                itemList.SetValue(_item, i);
                return;
            }
        }
        Debug.Log("inventory is full dumass");
    }

    public void SetNull(int index)
    {
        itemList.SetValue(null, index);
    }

    public void ClearArray()
    {
        Array.Clear(itemList, 0, itemList.Length);
    }

    public void SubtractItem(Item _item, int _slot_num)
    {
        itemList[_slot_num].amount--;
        if (itemList[_slot_num].amount <= 0)
        {
            SetNull(_slot_num);
        }
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void DropAllItems(Vector3 position, bool goToPlayer = false, bool magnetized = true, PlayerMain player = null)
    {
        Inventory playerInv = null;
        if (player != null)
        {
            playerInv = player.inventory;
        }
        if (itemList.Count() != 0)
        {
            for (int i = 0; i < itemList.Length; i++)
            {
                if (itemList[i] != null)
                {
                    if (goToPlayer)
                    {
                        playerInv.AddItem(itemList[i], position, false);
                    }
                    
                    else if (magnetized)
                    {
                        if (!GameManager.Instance.isServer)
                        {
                            ClientHelper.Instance.AskToSpawnItemBasicRPC(position, itemList[i].itemSO.itemType, magnetized);
                        }
                        else
                        {
                            RealItem newItem = RealItem.SpawnRealItem(position, itemList[i], true, true, itemList[i].ammo, false, true, true);
                            CalebUtils.RandomDirForceNoYAxis3D(newItem.GetComponent<Rigidbody>(), 5);
                        }
                    }
                    else
                    {
                        if (!GameManager.Instance.isServer)
                        {
                            ClientHelper.Instance.AskToSpawnItemBasicRPC(position, itemList[i].itemSO.itemType, magnetized);
                        }
                        else
                        {
                            RealItem newItem = RealItem.SpawnRealItem(position, itemList[i], true, true, itemList[i].ammo, false);
                            CalebUtils.RandomDirForceNoYAxis3D(newItem.GetComponent<Rigidbody>(), 5);
                        }
                    }
                }
            }
            ClearArray();
            OnItemListChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddLootItems(List<ItemSO> newItemList, List<int> newItemAmounts, List<int> newItemChances)
    {
        int i = 0;
        foreach (ItemSO _item in newItemList)
        {
            int randVal = Random.Range(1, 101);
            if (randVal <= newItemChances[i])
            {
                int tempAmount = newItemAmounts[i];
                while (tempAmount > 0)
                {
                    SetValue(new Item { itemSO = _item, amount = 1, equipType = _item.equipType, uses = _item.maxUses  });
                    tempAmount--;
                }
            }
            i++;
        }
    }

    public void SimpleAddItemArray(ItemSO[] itemArray)
    {
        for (int i = 0; i < itemArray.Length; i++)
        {
            Item _item = new Item { itemSO = itemArray[i], amount = 1 };
            SetValue(_item);
        }
    }

    public void SimpleAddItem(Item _item)
    {
        SetValue(_item);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItemBySlot(int slotNumber)
    {
        SetNull(slotNumber);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool GetItemTypeInInventory(ItemSO itemType)
    {
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i] != null)
            {
                if (itemList[i].itemSO.itemType == itemType.itemType)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int GetItemAmount(ItemSO itemType)
    {
        int itemAmount = 0;

        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i] != null)
            {
                if (itemList[i].itemSO.itemType == itemType.itemType)
                {
                    itemAmount += itemList[i].amount;
                }
            }
        }
        return itemAmount;
    }

    public void RefreshEmptySlots()
    {
        int num_of_empties = 0;
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i] != null)
            {
                if (itemList[i].amount <= 0 || itemList[i].uses <= 0 && itemList[i].itemSO.maxUses > 0)
                {
                    num_of_empties++;
                }
            }
        }       
        while (num_of_empties > 0)
        {
            int i;
            for (i = 0; i < itemList.Length; i++)
            {
                if (itemList[i] != null)
                {
                    if (itemList[i].amount <= 0 || itemList[i].uses <= 0 && itemList[i].itemSO.maxUses > 0)
                    {
                        SetNull(i);
                        num_of_empties--;
                        break;
                    }
                }
            }
        }
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetItemList(Item[] newList)
    {
        itemList = newList;
    }

    public bool isInvFull()//check there are enough slots to fit this item inside the inventory
    {
        return false;
    }

    public Item[] GetItemList()
    {
        return itemList;
    }

    public Item FindFirstNonEquippedItem(string itemType)
    {
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i] != null && itemList[i].itemSO.itemType == itemType && itemList[i] != GameManager.Instance.localPlayer.GetComponent<PlayerMain>().equippedHandItem)
            {
                return itemList[i];
            }
        }
        return null;
    }

    public int FindFirstNonEquippedItemIndex(string itemType)
    {
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i] != null && itemList[i].itemSO.itemType == itemType && itemList[i] != GameManager.Instance.localPlayer.GetComponent<PlayerMain>().equippedHandItem)
            {
                return i;
            }
        }
        return -1;
    }
}
