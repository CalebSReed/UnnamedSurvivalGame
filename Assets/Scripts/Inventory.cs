using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using System.Linq;

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
        //player = GameObject.FindGameObjectWithTag("Player");
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        int leftoverAmount = item.amount;
        if (item.itemSO.isStackable)
        {
            bool itemAlreadyInInventory = false;
            bool oneOrMoreSlotsAreFull = false;
            bool itemAdded = false;
            //Debug.Log(ItemCount());
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
                Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
                RealItem newItem = RealItem.SpawnRealItem(returnPos, new Item { itemSO = item.itemSO, amount = item.amount , equipType = item.equipType}, true, true, item.ammo, false, true);
                newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
                Debug.Log("inv full");
            }
            else if (leftoverAmount > 0 && !itemAdded)//if we have leftover amounts and if item is not added
            {
                Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
                RealItem newItem = RealItem.SpawnRealItem(returnPos, new Item { itemSO = item.itemSO, amount = leftoverAmount, equipType = item.equipType }, true, true, item.ammo, false, true);
                newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
                Debug.Log("SPITTING OUT ITEM");
            }
        }
        else if (!item.itemSO.isStackable && item.itemSO.isEquippable && !player.GetComponent<PlayerMain>().itemJustUnequipped && !player.GetComponent<PlayerMain>().isHandItemEquipped && item.equipType == Item.EquipType.HandGear && autoEquip)//if equippable, no item is equipped, and not recently unequipped, equip. inv fullness irrelevent
        {
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
            Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
            RealItem newItem = RealItem.SpawnRealItem(returnPos, new Item { itemSO = item.itemSO, amount = 1, uses = item.uses, equipType = item.equipType}, true, true, item.ammo, false, true);//uses are only set in this line, hopefully thats ok
            newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
            Debug.Log("inv full");
        }
        OnItemListChanged?.Invoke(this, EventArgs.Empty); //these events remind me of signals from godot...
        OnItemAdded?.Invoke(this, invArgs);
    }

    public void RefreshInventory()
    {
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
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

    public void DropAllItems(Vector3 position, bool goToPlayer = false, bool magnetized = true)
    {
        Inventory playerInv = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>().inventory;
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
                        Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
                        RealItem newItem = RealItem.SpawnRealItem(position, itemList[i], true, true, itemList[i].ammo, false, true, true);
                        newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
                    }
                    else
                    {
                        Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
                        RealItem newItem = RealItem.SpawnRealItem(position, itemList[i], true, true, itemList[i].ammo, false);
                        newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
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
            if (randVal < newItemChances[i])
            {
                int tempAmount = newItemAmounts[i];
                while (tempAmount > 0)
                {
                    SetValue(new Item { itemSO = _item, amount = 1 });
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
                if (itemList[i].amount == 0)
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
                    if (itemList[i].amount == 0)
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

    public bool isInvFull()//check there are enough slots to fit this item inside the inventory
    {
        return false;
    }

    public Item[] GetItemList()
    {
        return itemList;
    }

}
