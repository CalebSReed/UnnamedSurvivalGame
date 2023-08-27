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

    [SerializeField] private PlayerMain player;

    private void Awake()
    {
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!player.isHoldingItem)
                inventory.RemoveItemBySlot(itemSlotNumber);
            player.HoldItem(item);
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
            Debug.Log("Middle click");
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (!player.isHoldingItem)
            {
                if (item.itemSO.isEquippable)//if item is consumable, eatable, equipabble, etc...
                {
                    inventory.RemoveItemBySlot(itemSlotNumber);
                    txt.text = "";
                }
                else if (item.itemSO.isEatable)
                {
                    if (item.amount == 1)
                    {
                        txt.text = "";
                    }
                    if (item.itemSO.isPlate)
                    {
                        RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = ItemObjectArray.Instance.ClayPlate, amount = 1 }, false);
                    }
                    if (item.itemSO.isBowl)
                    {
                        RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = ItemObjectArray.Instance.ClayBowl, amount = 1 }, false);
                    }
                    inventory.SubtractItem(item, itemSlotNumber);
                }
                else if (item.itemSO.isDeployable)
                {
                    inventory.RemoveItemBySlot(itemSlotNumber);
                    txt.text = "";
                }
                player.UseItem(item);
            }
            else if (player.isHoldingItem)
            {
                if (item.itemSO.needsAmmo && item.ammo <= 0)
                {
                    player.CombineHandItem(item, player.heldItem);//ammo
                }
                else if (player.heldItem.itemSO.actionType == item.itemSO.actionType && item.itemSO.actionReward.Length != 0)
                {
                    Debug.Log("CUTTING");
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

    private void CombineItem()
    {
        if (player.heldItem.itemSO.needsAmmo)
        {
            player.heldItem.ammo--;
        }
        player.UseHeldItem();
        item.amount--;

        int i = 0;
        foreach (ItemSO _itemType in item.itemSO.actionReward)
        {
            RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.actionReward[i], amount = 1 }, false);
            i++;
        }

        if (item.amount <= 0)
        {
            inventory.RemoveItemBySlot(itemSlotNumber);
        }
    }

    private void StoreItem(int _reward)
    {
        if (item.itemSO.isBowl && player.heldItem.itemSO.isBowl)
        {
            Debug.Log("ADD BOWL");
            RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = ItemObjectArray.Instance.ClayBowl, amount = 1 }, false);
        }
        item.amount--;
        player.UseHeldItem();
        RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.storedItemReward[_reward], amount = 1 }, false);
        if (item.amount <= 0)
        {
            player.inventory.RemoveItemBySlot(itemSlotNumber);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!player.isHoldingItem)
        {
            if (item.itemSO.isDeployable)
            {
                txt.text = $"RMB: Deploy {item.itemSO.itemType}";
            }
            else if (item.itemSO.isEatable)
            {
                txt.text = $"RMB: Eat {item.itemSO.itemType}";
            }
            else if (item.itemSO.isEquippable)
            {
                txt.text = $"RMB: Equip {item.itemSO.itemType}";
            }
            else
            {
                txt.text = item.itemSO.itemType.ToString();
            }
        }
        else if (player.isHoldingItem)
        {
            if (player.heldItem.itemSO.actionType == item.itemSO.actionType && item.itemSO.actionReward.Length != 0)
            {
                txt.text = $"RMB: {player.heldItem.itemSO.actionType} {item.itemSO.itemType}";
            }
            else if (item.itemSO.needsAmmo && player.heldItem.itemSO.itemType == item.itemSO.validAmmo.itemType)//ohhhh fixed it??
            {
                txt.text = $"RMB: Load {item.itemSO.itemType} with {player.heldItem.itemSO.itemType}";
            }
            else if (item.itemSO.canStoreItems)//if this can store an item, and if held item can be stored in this item
            {
                int i = 0;
                foreach (ItemSO _itemType in item.itemSO.validStorableItems)
                {
                    if (_itemType.itemType == player.heldItem.itemSO.itemType)
                    {
                        txt.text = $"RMB: Put {player.heldItem.itemSO.itemType} in {item.itemSO.itemType}";
                        break;
                    }
                    i++;
                }
            }
        }
        else
        {
            txt.text = item.itemSO.itemType.ToString();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        txt.text = "";
    }
}
