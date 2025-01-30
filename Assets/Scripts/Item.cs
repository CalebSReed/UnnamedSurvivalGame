using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SerializeField]
public class Item 
{
    public enum EquipType
    {
        Null,
        HandGear,
        HeadGear,
        ChestGear,
        LegGear,
        FootGear
    }

    public ItemSO itemSO;

    public EquipType equipType;
    public int amount;
    public int uses;
    public int ammo;
    public bool isHot;
    public Item heldItem { get; set; }
    public Item[] containedItems;
    public float remainingTime;
    public Coroutine hotRoutine;

    public IEnumerator BecomeHot()
    {                
        isHot = true;
        remainingTime = 12;
        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(1);
            remainingTime--;
        }
        isHot = false;
    }

    public IEnumerator RemainHot(float remainingTime)
    {
        this.remainingTime = remainingTime;
        isHot = true;
        while (this.remainingTime > 0)
        {
            yield return new WaitForSeconds(1);
            this.remainingTime -= 1f;
        }
        isHot = false;
    }


    public static Item DupeItem(Item item)
    {        
        List<Item> dupedContainerItems = new List<Item>();
        if (item.itemSO.maxStorageSpace > 0)
        {
            for (int i = 0; i < item.containedItems.Length; i++)
            {
                if (item.containedItems[i] != null)
                {
                    Item newContainedItem = new Item
                    {
                        itemSO = item.containedItems[i].itemSO,
                        amount = item.containedItems[i].amount,
                        ammo = item.containedItems[i].ammo,
                        equipType = item.containedItems[i].equipType,
                        uses = item.containedItems[i].uses
                    };
                    dupedContainerItems.Add(newContainedItem);
                }
                else
                {
                    dupedContainerItems.Add(null);
                }
            }
        }

        Item newItem = new Item
        {
            itemSO = item.itemSO,
            amount = item.amount,
            ammo = item.ammo,
            equipType = item.equipType,
            uses = item.uses,
            containedItems = dupedContainerItems.ToArray()
        };
        return newItem;
    }

    public void StopBeingHot()
    {
        isHot = false;
        remainingTime = 0f;
    }
}
