using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SerializeField]
public class Item //You know... scriptable items aren't looking too bad rn Morty....
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
    //public ItemType itemType;//change to itemso.itemtype     nvm u cant do that....
    public EquipType equipType;
    public int amount;
    public int uses;
    public int ammo;
    public bool isHot;
    public Item containedItem;
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

    public static Item DupeItem(Item item)
    {
        Item newItem = new Item { itemSO = item.itemSO, amount = item.amount, ammo = item.ammo, equipType = item.equipType, uses = item.uses };
        return newItem;
    }

    public void StopBeingHot()
    {
        isHot = false;
    }
}
