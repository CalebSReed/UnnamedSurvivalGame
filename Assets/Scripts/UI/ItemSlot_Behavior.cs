using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlot_Behavior : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Item item;
    public int itemSlotNumber;
    private TextMeshProUGUI txt;

    public Inventory inventory;
    public UI_Inventory uiInventory;

    [SerializeField] private PlayerMain player;

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
                    //item = new Item { itemSO = player.heldItem.itemSO, amount = player.heldItem.amount, ammo = player.heldItem.ammo, uses = player.heldItem.uses };
                    //Debug.Log(item);
                    inventory.GetItemList().SetValue(player.heldItem, itemSlotNumber);
                    player.heldItem = null;
                    player.StopHoldingItem();
                    uiInventory.RefreshInventoryItems();
                }
                return;
            }

            if (!player.isHoldingItem && !player.deployMode)
            {
                inventory.RemoveItemBySlot(itemSlotNumber);
                player.HoldItem(item);

            }
            else if (!player.isHoldingItem && item.itemSO.isDeployable && !player.deployMode)//deploy item with left click. I dont think we need this, item sorting is more important
            {

                /*inventory.RemoveItemBySlot(itemSlotNumber);
                txt.text = "";
                player.UseItem(item);*/
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
                            player.amountTxt.text = player.heldItem.amount.ToString();
                        }
                        else
                        {
                            player.amountTxt.text = "";
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
                    player.UpdateHeldItemStats(player.heldItem);
                    /*if (player.heldItem.itemSO.isDeployable)//if deployable
                    {
                        player.UseItem(player.heldItem);
                    }*/
                    //player.pointerImage.sprite = player.heldItem.itemSO.itemSprite;
                    uiInventory.RefreshInventoryItems();
                }


            }
            
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
            Debug.Log("Middle click");
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (item == null)
            {
                Debug.Log("Item Is Null But I Still Live Here");
                return;
            }
            if (!player.isHoldingItem)
            {
                if (player.isHandItemEquipped && !item.itemSO.isEquippable && !item.itemSO.isEatable && item.itemSO.actionReward.Length != 0)//if has item equipped and itemslot item is not equippable and actions are the same
                {
                    if (player.equippedHandItem.itemSO.doActionType == item.itemSO.getActionType1 && item.itemSO.actionReward.Length != 0 || player.equippedHandItem.itemSO.doActionType == item.itemSO.getActionType2 && item.itemSO.actionReward2.Length != 0)
                    {
                        CombineItem();
                        return;
                    }
                }
                else if (item.itemSO.isEquippable)//if item is consumable, eatable, equipabble, etc...
                {
                    inventory.RemoveItemBySlot(itemSlotNumber);
                    txt.text = "";
                }
                else if (item.itemSO.isDeployable)
                {
                    inventory.RemoveItemBySlot(itemSlotNumber);
                    txt.text = "";
                }
                if (item.itemSO.isEatable)
                {
                    if (item.amount == 1)
                    {
                        txt.text = "";
                    }
                    if (item.itemSO.isPlate)
                    {
                        RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 1 }, false);
                    }
                    if (item.itemSO.isBowl)
                    {
                        RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 }, false);
                    }
                    inventory.SubtractItem(item, itemSlotNumber);
                }
                player.UseItem(item);
            }
            else if (player.isHoldingItem)
            {
                if (item.itemSO.needsAmmo && item.ammo < item.itemSO.maxAmmo)
                {
                    player.CombineHandItem(item, player.heldItem);//ammo
                }
                else if (player.heldItem.itemSO.doActionType == item.itemSO.getActionType1 && item.itemSO.actionReward.Length != 0 || player.heldItem.itemSO.doActionType == item.itemSO.getActionType2 && item.itemSO.actionReward2.Length != 0)
                {
                    if (player.heldItem.itemSO.needsAmmo && player.heldItem.ammo > 0)//if needs ammo, check if has ammo to craft with
                    {
                        CombineItem();
                    }
                    else if (player.heldItem.itemSO.needsAmmo && player.heldItem.ammo <= 0)//if we dont have enough ammo to craft
                    {
                        Debug.LogError("NEEDS AMMO");
                    }
                    else//if we dont need ammo, attempt to craft
                    {
                        CombineItem();
                    }
                }
                else if (item.itemSO.canStoreItems)//if this can store an item, and if held item can be stored in this item
                {
                    int i = 0;
                    foreach (ItemSO _itemType in item.itemSO.validStorableItems)//wait are we checking if the base instance equals a brand new instance??? i hope this works lol WAIT i fixed it I THINK!!!
                    {
                        if (_itemType.itemType == player.heldItem.itemSO.itemType)//change to check their itemtypes
                        {
                            StoreItem(i);
                            break;
                        }
                        i++;
                    }
                }
            }
        }
    }

    private void CombineItem()//i think we always enter here able to craft
    {
        bool isStackable = item.itemSO.isStackable;
        if (player.isHoldingItem)//action with held item
        {
            Item tempItem = new Item { itemSO = player.heldItem.itemSO };
            if (player.heldItem.itemSO.needsAmmo)
            {
                player.heldItem.ammo--;
                if (player.heldItem.ammo <= 0)
                {
                    player.UpdateHeldItemStats(player.heldItem);
                }
            }
            if (!item.itemSO.isWall)//we shouldnt punish players for wanting to make cleaner looking walls
            {
                player.UseHeldItem();
            }
            item.amount--;

            if (tempItem.itemSO.doActionType == item.itemSO.getActionType1 && item.itemSO.actionReward.Length != 0)
            {
                int i = 0;
                foreach (ItemSO _itemType in item.itemSO.actionReward)
                {
                    if (isStackable || item.itemSO.actionReward.Length > 1)
                    {
                        RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.actionReward[i], amount = 1 }, false);
                    }
                    else if (!isStackable && item.itemSO.actionReward.Length == 1)
                    {
                        inventory.GetItemList()[itemSlotNumber] = new Item { itemSO = item.itemSO.actionReward[i], amount = 1, equipType = item.itemSO.actionReward[i].equipType, uses = item.itemSO.actionReward[i].maxUses, ammo = 0 };
                        this.item = inventory.GetItemList()[itemSlotNumber];
                        uiInventory.RefreshInventoryItems();
                    }
                    i++;
                }
            }

            if (tempItem.itemSO.doActionType == item.itemSO.getActionType2 && item.itemSO.actionReward2.Length != 0)
            {
                int i = 0;
                foreach (ItemSO _itemType in item.itemSO.actionReward2)
                {
                    if (isStackable || item.itemSO.actionReward2.Length > 1)
                    {
                        RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.actionReward2[i], amount = 1 }, false);
                    }
                    else if (!isStackable && item.itemSO.actionReward2.Length == 1)
                    {
                        inventory.GetItemList()[itemSlotNumber] = new Item { itemSO = item.itemSO.actionReward2[i], amount = 1, equipType = item.itemSO.actionReward2[i].equipType, uses = item.itemSO.actionReward2[i].maxUses, ammo = 0 };
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
        }
        else
        {
            Item tempItem = new Item { itemSO = player.equippedHandItem.itemSO };
            if (player.equippedHandItem.itemSO.needsAmmo)//action with equipped item
            {
                player.equippedHandItem.ammo--;
            }
            if (!item.itemSO.isWall)//we shouldnt punish players for wanting to make cleaner looking walls
            {
                player.UseItemDurability();
            }
            item.amount--;

            if (tempItem.itemSO.doActionType == item.itemSO.getActionType1 && item.itemSO.actionReward.Length != 0)
            {
                int i = 0;
                foreach (ItemSO _itemType in item.itemSO.actionReward)
                {
                    if (isStackable || item.itemSO.actionReward.Length > 1)
                    {
                        RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.actionReward[i], amount = 1 }, false);
                    }
                    else if (!isStackable && item.itemSO.actionReward.Length == 1)
                    {
                        inventory.GetItemList()[itemSlotNumber] = new Item { itemSO = item.itemSO.actionReward[i], amount = 1, equipType = item.itemSO.actionReward[i].equipType, uses = item.itemSO.actionReward[i].maxUses, ammo = 0 };
                        this.item = inventory.GetItemList()[itemSlotNumber];
                        uiInventory.RefreshInventoryItems();
                    }
                    i++;
                }
            }

            if (tempItem.itemSO.doActionType == item.itemSO.getActionType2 && item.itemSO.actionReward2.Length != 0)
            {
                int i = 0;
                foreach (ItemSO _itemType in item.itemSO.actionReward2)
                {
                    if (isStackable || item.itemSO.actionReward2.Length > 1)
                    {
                        RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.actionReward2[i], amount = 1 }, false);
                    }
                    else if (!isStackable && item.itemSO.actionReward2.Length == 1)
                    {
                        inventory.GetItemList()[itemSlotNumber] = new Item { itemSO = item.itemSO.actionReward2[i], amount = 1, equipType = item.itemSO.actionReward2[i].equipType, uses = item.itemSO.actionReward2[i].maxUses, ammo = 0 };
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
        }
        player.AnimateActionUse();
        int randVal = Random.Range(1, 4);
        player.audio.Play($"Chop{randVal}", gameObject);
    }

    private void StoreItem(int _reward)
    {
        bool isStackable = item.itemSO.isStackable;
        if (item.itemSO.isBowl && player.heldItem.itemSO.isBowl)
        {
            Debug.Log("ADD BOWL");
            RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 }, false);
        }
        player.UseHeldItem();

        if (isStackable)
        {
            item.amount--;
            RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.storedItemReward[_reward], amount = 1 }, false);
            if (item.amount <= 0)
            {
                inventory.RemoveItemBySlot(itemSlotNumber);
            }
        }
        else
        {
            item = new Item { itemSO = item.itemSO.storedItemReward[_reward], amount = 1, equipType = item.itemSO.storedItemReward[_reward].equipType, uses = item.itemSO.storedItemReward[_reward].maxUses, ammo = 0 };
            inventory.GetItemList()[itemSlotNumber] = item;
            uiInventory.RefreshInventoryItems();
        }


    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
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
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        txt.text = "";
    }
}
