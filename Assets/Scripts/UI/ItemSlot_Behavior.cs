using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlot_Behavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item item;
    public bool isMouseHoveringOver;
    public int itemSlotNumber;
    public bool isChestSlot = false;
    private TextMeshProUGUI txt;
    [SerializeField] private Hoverable hoverBehavior;

    public Inventory inventory;
    public UI_Inventory uiInventory;

    [SerializeField] private PlayerMain player;
    [SerializeField] private UI_ItemSlotController slotController;

    private void Awake()
    {
        //hoverBehavior = GetComponent<Hoverable>();
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
        slotController = GameObject.FindGameObjectWithTag("SlotController").GetComponent<UI_ItemSlotController>();
    }

    public void OnAcceptButtonPressed()
    {
        if (player.StateMachine.currentPlayerState == player.deployState || player.playerInput.PlayerDefault.SpecialModifier.ReadValue<float>() == 1)
        {
            Debug.Log("bye");
            return;
        }
        if (item == null)
        {
            if (player.isHoldingItem)
            {
                inventory.GetItemList().SetValue(player.heldItem, itemSlotNumber);
                player.heldItem = null;
                player.StopHoldingItem();
                uiInventory.RefreshInventoryItems();
            }
            return;
        }

        if (!player.isHoldingItem)
        {
            player.HoldItem(item);
            inventory.RemoveItemBySlot(itemSlotNumber);

        }
        else if (player.isHoldingItem)
        {
            if (item.itemSO.itemType == player.heldItem.itemSO.itemType && item.itemSO.isStackable)//if item types match, and are stackable obvs...
            {
                item.amount += player.heldItem.amount;//add amounts
                player.heldItem.amount = 0;
                if (item.amount > item.itemSO.maxStackSize)//if exceeds stack size
                {
                    player.heldItem.amount = item.amount - item.itemSO.maxStackSize;//exceeding limit like 24 - max stack size like 20 = 4
                    item.amount = item.itemSO.maxStackSize;
                    if (player.heldItem.amount != 1)
                    {
                        //player.amountTxt.text = player.heldItem.amount.ToString();
                    }
                    else
                    {
                        //player.amountTxt.text = "";
                    }
                }

                if (player.heldItem.amount <= 0)
                {
                    player.heldItem = null;
                    player.StopHoldingItem();
                }

                uiInventory.RefreshInventoryItems();
            }
            else//swap items if types dont match
            {
                Item tempItem = null;
                tempItem = item;//new Item { itemSO = item.itemSO, ammo = item.ammo, amount = item.amount, uses = item.uses, equipType = item.equipType };//not sure if this is required because pointers and such but whatevs.
                item = player.heldItem;
                inventory.GetItemList().SetValue(item, itemSlotNumber);
                player.heldItem = tempItem;
                player.UpdateHeldItemStats();

                uiInventory.RefreshInventoryItems();
            }


        }
    }

    public void SubtractItem()
    {
        item.amount--;
        if (item.amount <= 0)
        {
            inventory.RemoveItemBySlot(itemSlotNumber);
        }
        inventory.RefreshInventory();
    }

    public void CombineItem(Item playerItem, int actionType)
    {
        bool isStackable = item.itemSO.isStackable;
        Item tempItem = new Item { itemSO = playerItem.itemSO };//just a reference to read values

        if (tempItem.itemSO.needsAmmo)
        {
            playerItem.ammo--;
        }

        if (!item.itemSO.isWall)
        {
            if (playerItem.itemSO.isEquippable)
            {
                playerItem.uses--;//no need to check, we use the update item function at the end.
            }
            else
            {
                playerItem.amount--;
            }
        }

        if (actionType == 1)
        {
            int i = 0;
            item.amount--;
            foreach (ItemSO _itemType in item.itemSO.actionReward)
            {
                if (isStackable || item.itemSO.actionReward.Length > 1)
                {
                    player.inventory.AddItem(new Item { itemSO = item.itemSO.actionReward[i], amount = 1, equipType = item.itemSO.actionReward[i].equipType, uses = item.itemSO.actionReward[i].maxUses }, player.transform.position, false);
                }
                else if (!isStackable && item.itemSO.actionReward.Length == 1)
                {
                    inventory.GetItemList()[itemSlotNumber] = new Item { itemSO = item.itemSO.actionReward[i], amount = 1, equipType = item.itemSO.actionReward[i].equipType, uses = item.itemSO.actionReward[i].maxUses, ammo = 0 };
                    inventory.RefreshInventory();
                    this.item = inventory.GetItemList()[itemSlotNumber];
                    uiInventory.RefreshInventoryItems();
                }
                i++;
            }
        }
        else if (actionType == 2)
        {
            int i = 0;
            item.amount--;
            foreach (ItemSO _itemType in item.itemSO.actionReward2)
            {
                if (isStackable || item.itemSO.actionReward2.Length > 1)
                {
                    player.inventory.AddItem(new Item { itemSO = item.itemSO.actionReward2[i], amount = 1, equipType = item.itemSO.actionReward2[i].equipType, uses = item.itemSO.actionReward2[i].maxUses, ammo = 0 }, player.transform.position, false);
                }
                else if (!isStackable && item.itemSO.actionReward2.Length == 1)
                {
                    inventory.GetItemList()[itemSlotNumber] = new Item { itemSO = item.itemSO.actionReward2[i], amount = 1, equipType = item.itemSO.actionReward2[i].equipType, uses = item.itemSO.actionReward2[i].maxUses, ammo = 0 };
                    inventory.RefreshInventory();
                    this.item = inventory.GetItemList()[itemSlotNumber];
                    uiInventory.RefreshInventoryItems();
                }
                i++;
            }
        }

        if (item.amount <= 0)
        {
            inventory.RemoveItemBySlot(itemSlotNumber);
        }

        int randVal = Random.Range(1, 4);
        player.audio.Play($"Chop{randVal}", player.transform.position, gameObject, true);
    }

    public void StoreItem(Item playerItem)
    {
        int i = 0;
        foreach(ItemSO validItem in item.itemSO.validStorableItems)
        {
            if (playerItem.itemSO == validItem)
            {
                bool isStackable = item.itemSO.isStackable;
                if (item.itemSO.isBowl && player.heldItem.itemSO.isBowl)
                {
                    Debug.Log("ADD BOWL");
                    player.inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 }, player.transform.position, false);
                }
                player.UseHeldItem();

                if (isStackable)
                {
                    item.amount--;
                    player.inventory.AddItem(new Item { itemSO = item.itemSO.storedItemReward[i], amount = 1 }, player.transform.position, false);
                    if (item.amount <= 0)
                    {
                        inventory.RemoveItemBySlot(itemSlotNumber);
                    }
                }
                else
                {
                    item = new Item { itemSO = item.itemSO.storedItemReward[i], amount = 1, equipType = item.itemSO.storedItemReward[i].equipType, uses = item.itemSO.storedItemReward[i].maxUses, ammo = 0 };
                    inventory.GetItemList()[itemSlotNumber] = item;
                    uiInventory.RefreshInventoryItems();
                }
            }
            i++;
        }
    }

    private void ChangeItem(Item item)
    {
        this.item = item;
    }

    public void RefreshName()
    {
        if (item != null)
        {
            hoverBehavior.Name = item.itemSO.itemName;
        }
        else//null item, reset it all!
        {
            hoverBehavior.Name = "";
            hoverBehavior.Prefix = "";
            return;
        }

        /*if (item.itemSO.itemType != "tongs" && player.hasTongs && UI_ItemSlotController.IsTongable(item, player.equippedHandItem) && player.equippedHandItem.containedItem == null || player.hasTongs && item.itemSO.isReheatable)
        {
            hoverBehavior.Prefix = "RMB: Grab ";
        }*/

        if (item.itemSO.isEatable)
        {
            hoverBehavior.Prefix = "RMB: Eat ";
        }
        else if (item.itemSO.isEquippable)
        {
            if (player.equippedHandItem == item)
            {
                hoverBehavior.Prefix = "RMB: Unequip ";
            }
            else
            {
                hoverBehavior.Prefix = "RMB: Equip ";
            }
        }
        else if (item.itemSO.isDeployable)
        {
            hoverBehavior.Prefix = "RMB: Deploy ";
        }
        else
        {
            hoverBehavior.Prefix = "";
        }
    }

    public void LoadItem()//always with held item
    {
        player.UseHeldItem();
        item.ammo++;
        player.inventory.RefreshInventory();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        slotController.SelectItemSlot(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slotController.selectedItemSlot == this)//so that we dont deselect a different slot
        {
            slotController.DeSelectItemSlot();
        }
    }
}
