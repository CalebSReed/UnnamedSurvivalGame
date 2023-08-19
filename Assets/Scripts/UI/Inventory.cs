using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class Inventory : MonoBehaviour
{
    private List<Item> itemList; //make list that stores 'Item' types from ITEM class. 'itemList' is a new field, or essentially the actual inventory.

    public event EventHandler OnItemListChanged;

    [SerializeField]
    GameObject player;

    private TextMeshProUGUI txt;

    private void Awake()
    {
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
    }

    public Inventory() //setup constructor whatever that is
    {
        itemList = new List<Item>(); //initialize the list
    }
    //bro straight up CLEAN THIS SHIT UP WHEN UR DONE PLEASE
    public void AddItem(Item item, Collider2D collider) //adds 'Item' type of item into list 'itemList'
    {
        RealItem realItem = collider.GetComponent<RealItem>();
        SpriteRenderer itemSprite = collider.GetComponent<SpriteRenderer>();
        if (item.isStackable())
        {
            bool itemAlreadyInInventory = false;
            bool oneOrMoreSlotsAreFull = false;
            bool itemAdded = false;
            foreach (Item inventoryItem in itemList) //check if this item type already exists in inventory
            {
                if (inventoryItem.itemType == item.itemType) //if it does exist, add on to the amount of the item
                {
                    inventoryItem.amount += item.amount;//11+11 = 22
                    itemAlreadyInInventory = true;
                    int invItemTempAmount = inventoryItem.amount;
                    if (invItemTempAmount > item.GetItemStackSize() && itemList.Count >= 16)//if itemslot exceeds stack size AND inventory has no extra slots, set amount to max stack size, and spit out remaining amount back into world
                    {
                        //Debug.LogError("wait what " + invItemTempAmount + " temp amount, and item amount: " + item.amount);
                        invItemTempAmount -= item.GetItemStackSize();
                        realItem.item.amount = invItemTempAmount;
                        realItem.RefreshAmount(item);
                        inventoryItem.amount = item.GetItemStackSize();
                        itemSprite.color = new Color(1f, 1f, 1f, 1f);
                        //break;
                    }
                    else if (invItemTempAmount > item.GetItemStackSize() && itemList.Count < 16 && !oneOrMoreSlotsAreFull)//if exceeds BUT inventory has extra slots, set first slot amount to max, next slot to remaining, then destroy ingame item
                    {
                        oneOrMoreSlotsAreFull = true;//sets first stack to max size, now keep searching thru the inventory for an un-full stack...
                        invItemTempAmount -= item.GetItemStackSize();
                        item.amount = invItemTempAmount;
                        inventoryItem.amount = item.GetItemStackSize();
                    }
                    else if (oneOrMoreSlotsAreFull && inventoryItem.itemType == item.itemType && invItemTempAmount > item.GetItemStackSize())//found the un-full stack, but its too big to fit it all, set to max, but we should keep cycling if we add inventory organization...
                    {
                        //invItemTempAmount += item.amount;
                        //if (invItemTempAmount)
                        inventoryItem.amount = item.GetItemStackSize();
                        invItemTempAmount -= item.GetItemStackSize();
                        item.amount = invItemTempAmount;
                        //itemAdded = true;
                        //realItem.DestroySelf();
                        //must keep cycling actually or sumn idk, item limit is goign over
                    }                    
                    else if (oneOrMoreSlotsAreFull && inventoryItem.itemType == item.itemType && invItemTempAmount <= item.GetItemStackSize())//found a stack that can fit.
                    {
                        itemAdded = true;
                        realItem.DestroySelf();
                        break;
                    }
                    else
                    {
                        realItem.DestroySelf();
                        Debug.Log("Nothing is true");
                    }
                }
            }
            if (oneOrMoreSlotsAreFull && !itemAdded)//if we never found a stack, inv should already not be full i think.... so add the item
            {
                itemList.Add(item);
                realItem.DestroySelf();
            }
            if (!itemAlreadyInInventory && itemList.Count < 16) //if it doesnt exist, and inventory isnt full, create the new item in inventory
            {
                itemList.Add(item);
                realItem.DestroySelf();
            }
            else if (!itemAlreadyInInventory && itemList.Count >= 16)//if it doesnt exist, is stackable, and inv IS full, dont add it
            {
                itemSprite.color = new Color(1f, 1f, 1f, 1f);
                Debug.Log("inv full");
            }
        }
        else if (itemList.Count <= 15 && !item.isStackable())//if not stackable but can fit
        {
            itemList.Add(item);
            realItem.DestroySelf();
        }
        else//unstackable and full inventory
        {
            itemSprite.color = new Color(1f, 1f, 1f, 1f);
            Debug.Log("inv full");
        }
        OnItemListChanged?.Invoke(this, EventArgs.Empty); //these events remind me of signals from godot...
    }

    public void SubtractItem(Item _item, int _slot_num)
    {
        itemList[_slot_num].amount--;
        if (itemList[_slot_num].amount <= 0)
        {
            itemList.RemoveAt(_slot_num);
        }
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void DropAllItems(Vector3 position)
    {
        if (itemList.Count != 0)
        {
            foreach (Item item in itemList)
            {
                Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
                RealItem newItem = RealItem.SpawnRealItem(position, item, true, true);
                newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
            }
            itemList.Clear();
            OnItemListChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddLootItems(List<Item> newItemList)
    {
        //itemList. add multiple items to the inventory
        itemList = newItemList;
    }

    public void SimpleAddItem(Item _item)
    {
        itemList.Add(_item);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItemBySlot(int slotNumber)
    {
        itemList.RemoveAt(slotNumber);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool GetItemTypeInInventory(Item.ItemType itemType)
    {
        foreach (Item inventoryItem in itemList)
        {
            if (inventoryItem.itemType == itemType)
            {
                return true;
            }
        }
        return false;
    }

    public int GetItemAmount(Item.ItemType itemType)
    {
        int itemAmount = 0;
        
        foreach (Item inventoryItem in itemList)
        {
            if (inventoryItem.itemType == itemType)
            {
                itemAmount += inventoryItem.amount;
            }
        }
        return itemAmount;
    }

    public void RefreshEmptySlots()
    {
        int num_of_empties = 0;
        foreach (Item inventoryItem in itemList)
        {
            if (inventoryItem.amount == 0)
            {
                num_of_empties++;
            }
        }
        while (num_of_empties > 0)
        {
            int i = 0;
            foreach (Item inventoryItem in itemList)
            {
                if (inventoryItem.amount == 0)
                {
                    itemList.RemoveAt(i);
                    num_of_empties--;
                    break;
                }
                i++;
            }
        }
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool isInvFull()//check there are enough slots to fit this item inside the inventory
    {
        return false;
    }

    public List<Item> GetItemList()
    {
        return itemList;
    }

}
