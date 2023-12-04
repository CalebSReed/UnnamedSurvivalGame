using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlot_Behavior : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Item item;
    public bool isMouseHoveringOver;
    public int itemSlotNumber;
    private TextMeshProUGUI txt;

    public Inventory inventory;
    public UI_Inventory uiInventory;

    [SerializeField] private PlayerMain player;
    [SerializeField] private UI_ItemSlotController slotController;

    private void Awake()
    {
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
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
                inventory.RemoveItemBySlot(itemSlotNumber);
                player.HoldItem(item);

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
                    tempItem = new Item { itemSO = item.itemSO, ammo = item.ammo, amount = item.amount, uses = item.uses, equipType = item.equipType};//not sure if this is required because pointers and such but whatevs.
                    item = player.heldItem;
                    inventory.GetItemList().SetValue(item, itemSlotNumber);
                    player.heldItem = tempItem;
                    player.UpdateHeldItemStats();

                    uiInventory.RefreshInventoryItems();
                }


            }
            
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
            Debug.Log("Middle click");
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            //
        }
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
            foreach (ItemSO _itemType in item.itemSO.actionReward)
            {
                if (isStackable || item.itemSO.actionReward.Length > 1)
                {
                    item.amount--;
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
            foreach (ItemSO _itemType in item.itemSO.actionReward2)
            {
                if (isStackable || item.itemSO.actionReward2.Length > 1)
                {
                    item.amount--;
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
        player.audio.Play($"Chop{randVal}", gameObject);
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

    public void LoadItem()//always with held item
    {
        player.UseHeldItem();
        item.ammo++;
        player.inventory.RefreshInventory();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        slotController.SelectItemSlot(this);

        /*if (item != null)
        {
            if (!player.isHoldingItem)
            {
                if (item.itemSO.isDeployable)
                {
                    txt.text = $"RMB: Deploy {item.itemSO.itemName}";
                }
                else if (item.itemSO.isEatable)
                {
                    txt.text = $"RMB: Eat {item.itemSO.itemName}";
                }
                else if (item.itemSO.isEquippable)
                {
                    txt.text = $"RMB: Equip {item.itemSO.itemName}";
                }
                else
                {
                    txt.text = item.itemSO.itemName.ToString();
                }
                if (player.isHandItemEquipped)//at bottom as a new if statement because of it being a special case where two options can be correct
                {
                    if (player.equippedHandItem.itemSO.doActionType == item.itemSO.getActionType1 && item.itemSO.actionReward.Length != 0 || player.equippedHandItem.itemSO.doActionType == item.itemSO.getActionType2 && item.itemSO.actionReward2.Length != 0)
                    {
                        if (item.itemSO.isDeployable)
                        {
                            txt.text = $"RMB: {player.equippedHandItem.itemSO.doActionType} {item.itemSO.itemName}";
                        }
                        else
                        {
                            txt.text = $"RMB: {player.equippedHandItem.itemSO.doActionType} {item.itemSO.itemName}";
                        }
                    }
                }
            }
            else if (player.isHoldingItem)
            {
                if (player.heldItem.itemSO.doActionType == item.itemSO.getActionType1 && item.itemSO.actionReward.Length != 0 || player.heldItem.itemSO.doActionType == item.itemSO.getActionType2 && item.itemSO.actionReward2.Length != 0)
                {
                    txt.text = $"RMB: {player.heldItem.itemSO.doActionType} {item.itemSO.itemName}";
                }
                else if (item.itemSO.needsAmmo && player.heldItem.itemSO.itemType == item.itemSO.validAmmo.itemType)//ohhhh fixed it??
                {
                    txt.text = $"RMB: Load {item.itemSO.itemName} with {player.heldItem.itemSO.itemName}";
                }
                else if (item.itemSO.canStoreItems)//if this can store an item, and if held item can be stored in this item
                {
                    int i = 0;
                    foreach (ItemSO _itemType in item.itemSO.validStorableItems)
                    {
                        if (_itemType.itemType == player.heldItem.itemSO.itemType)
                        {
                            txt.text = $"RMB: Put {player.heldItem.itemSO.itemName} in {item.itemSO.itemName}";
                            break;
                        }
                        i++;
                    }
                }
            }
            else
            {
                txt.text = item.itemSO.itemName.ToString();
            }
        }*/
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slotController.selectedItemSlot == this)//so that we dont deselect a different slot
        {
            slotController.DeSelectItemSlot();
        }
    }
}
