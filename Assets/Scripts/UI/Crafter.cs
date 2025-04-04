﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafter : MonoBehaviour
{
    public Inventory inventory;
    Item item;

    private UI_Inventory uiInventory;

    [SerializeField] private GameObject player;
    private UI_CraftMenu_Controller uiCrafter;
    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
        uiInventory = player.GetComponent<PlayerMain>().uiInventory;
        uiCrafter = SceneReferences.Instance.uiCrafter;
    }

    private int ingredient1Pos;
    private int ingredient2Pos;
    private int ingredient3Pos;
    private int ingredient1Amount;
    private int ingredient2Amount;
    private int ingredient3Amount;

    public EventHandler<CraftingArgs> onCrafted;
    private CraftingArgs craftArgs = new CraftingArgs();
    //CLEAN THIS SHIT UP WHEN UR DONE TOO HOLYYY
    public void Craft(ItemSO ingredient1, int ingredient1AmountRequired, ItemSO ingredient2, int ingredient2AmountRequired, ItemSO ingredient3, int ingredient3AmountRequired, Item Reward)//for recipes with more ingredients, perhaps make 3rd ingredient option by making it default to 0
    {
        Inventory inv = player.GetComponent<PlayerMain>().inventory;//player inventory
        if (player.GetComponent<PlayerMain>().freeCrafting)
        {
            inv.AddItem(new Item { itemSO = Reward.itemSO, ammo = 0, amount = Reward.amount, equipType = Reward.itemSO.equipType, uses = Reward.itemSO.maxUses}, player.transform.position, false);
            return;
        }
        //bool ingredient1Found = false;
        //bool ingredient2Found = false;
        ingredient1Amount = 0;
        ingredient2Amount = 0;
        ingredient3Amount = 0;

        for (int i = 0; i < inventory.GetItemList().Length; i++)
        {
            if (inventory.GetItemList()[i] != null)
            {
                Item inventoryItem = inventory.GetItemList()[i];

                if (inventoryItem.itemSO.itemType == ingredient1.itemType)//if we have the item for ingredient 1, add temp amount value each time we go thru an item on the list
                {
                    ingredient1Amount += inventoryItem.amount;
                    //ingredient1Found = true;
                    ingredient1Pos = i; // need to not change pos when we find a 2nd slot with item, and need to remove appropriate number of slots when successfully crafting
                    Debug.Log("1 found");
                }

                if (ingredient2 != null)
                {
                    if (inventoryItem.itemSO.itemType == ingredient2.itemType)//same for ingredient 2
                    {
                        ingredient2Amount += inventoryItem.amount;
                        //ingredient2Found = true;
                        ingredient2Pos = i;
                        Debug.Log("2 found");
                    }
                }

                if (ingredient3 != null)
                {
                    if (inventoryItem.itemSO.itemType == ingredient3.itemType)//same for ingredient 3
                    {
                        ingredient3Amount += inventoryItem.amount;
                        //ingredient2Found = true;
                        ingredient3Pos = i;
                        Debug.Log("3 found");
                    }
                }
            }
        }
        Debug.Log("we have " + ingredient1Amount + " amount of ing 1 and "+ ingredient2Amount + " amount of ing 2 and " + ingredient3Amount + " amount of ing 3.");
        if (ingredient1Amount >= ingredient1AmountRequired && ingredient2Amount >= ingredient2AmountRequired && ingredient3Amount >= ingredient3AmountRequired) //if we have the right amount, delete item amounts, and grant reward and refresh UI
        {
            if (!inventory.isInvFull())//need new function to subtract items from inventory starting from beginning to end, if stack isnt enough we can check next stacks and keep subtracting until we done
            {
                int i = 0;
                Array.Reverse(inventory.GetItemList());//reverse so we go from right to left subtracting
                bool doneSubtracting1 = false;
                bool doneSubtracting2 = false;
                bool doneSubtracting3 = false;
                int tempAmountRequired1 = ingredient1AmountRequired;
                int tempAmountRequired2 = ingredient2AmountRequired;
                int tempAmountRequired3 = ingredient3AmountRequired;

                for (i = 0; i < inventory.GetItemList().Length; i++)
                {
                    if (inventory.GetItemList()[i] != null)
                    {
                        Item inventoryItem = inventory.GetItemList()[i];
                        if (inventoryItem.itemSO.itemType == ingredient1.itemType && !doneSubtracting1)//if ing1 found, subtract, if lower than 0, set to 0, and set tempRequired to positive amount of ing1leftovers
                        {
                            inventoryItem.amount -= tempAmountRequired1;

                            if (inventoryItem.amount < 0)
                            {
                                tempAmountRequired1 = inventoryItem.amount * -1;
                                inventoryItem.amount = 0;
                            }
                            else// if item amount is 0 or more after subtracting, stop.
                            {
                                doneSubtracting1 = true;
                            }
                        }

                        if (ingredient2 != null)
                        {
                            if (inventoryItem.itemSO.itemType == ingredient2.itemType && !doneSubtracting2)//same for ingredient 2
                            {
                                inventoryItem.amount -= tempAmountRequired2;

                                if (inventoryItem.amount < 0)
                                {
                                    tempAmountRequired2 = inventoryItem.amount * -1;
                                    inventoryItem.amount = 0;
                                }
                                else//same as ing1
                                {
                                    doneSubtracting2 = true;
                                }
                            }
                        }

                        if (ingredient3 != null)
                        {
                            if (inventoryItem.itemSO.itemType == ingredient3.itemType && !doneSubtracting3)//same for ingredient 2
                            {
                                inventoryItem.amount -= tempAmountRequired3;

                                if (inventoryItem.amount < 0)
                                {
                                    tempAmountRequired3 = inventoryItem.amount * -1;
                                    inventoryItem.amount = 0;
                                }
                                else//same as ing1
                                {
                                    doneSubtracting3 = true;
                                }
                            }
                        }
                    }
                }
                Array.Reverse(inventory.GetItemList());//back to normal
                inventory.RefreshEmptySlots();
                inv.AddItem(new Item { itemSO = Reward.itemSO, ammo = 0, amount = Reward.amount, equipType = Reward.itemSO.equipType, uses = Reward.itemSO.maxUses}, player.transform.position, false);
                uiInventory.RefreshInventoryItems();
                uiCrafter.RefreshCraftingMenuRecipes();
                if (ingredient1.isBowl && ingredient1AmountRequired - Reward.amount > 0 && Reward.itemSO.isBowl)
                {
                    inv.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = ingredient1AmountRequired - Reward.amount }, player.transform.position, false);
                }
                else if(ingredient1.isBowl && !Reward.itemSO.isBowl)
                {
                    inv.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = ingredient1AmountRequired }, player.transform.position, false);
                }

                if (ingredient2 != null && ingredient2.isBowl && ingredient2AmountRequired - Reward.amount > 0)
                {
                    inv.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = ingredient2AmountRequired - Reward.amount }, player.transform.position, false);
                }
                else if (ingredient2 != null && ingredient2.isBowl && !Reward.itemSO.isBowl)
                {
                    inv.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = ingredient2AmountRequired }, player.transform.position, false);
                }

                if (ingredient3 != null && ingredient3.isBowl && ingredient3AmountRequired - Reward.amount > 0)
                {
                    inv.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = ingredient3AmountRequired - Reward.amount }, player.transform.position, false);
                }
                else if (ingredient3 != null && ingredient3.isBowl && !Reward.itemSO.isBowl)
                {
                    inv.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = ingredient3AmountRequired }, player.transform.position, false);
                }
                craftArgs.rewardItem = Reward.itemSO;
                onCrafted?.Invoke(this, craftArgs);
                Debug.Log("crafted");
            }
            else
            {
                Debug.Log("inv full!");
            }
        }
    }
}
