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
    private AudioManager audio;

    public Inventory inventory;

    [SerializeField] private PlayerMain player;

    private void Awake()
    {
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
        audio = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!player.isHoldingItem && !item.itemSO.isDeployable)
            {
                inventory.RemoveItemBySlot(itemSlotNumber);
                player.HoldItem(item);

            }
            else if (item.itemSO.isDeployable)
            {

                inventory.RemoveItemBySlot(itemSlotNumber);
                txt.text = "";
                player.UseItem(item);
            }
            
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
            Debug.Log("Middle click");
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (!player.isHoldingItem)
            {
                if (player.isItemEquipped && !item.itemSO.isEquippable && !item.itemSO.isEatable)//if has item equipped and itemslot item is not equippable and actions are the same
                {
                    if (player.equippedHandItem.itemSO.actionType == item.itemSO.actionType && item.itemSO.actionReward.Length != 0 || player.equippedHandItem.itemSO.actionType == item.itemSO.actionType2 && item.itemSO.actionReward2.Length != 0)
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
                if (item.itemSO.needsAmmo && item.ammo <= 0)
                {
                    player.CombineHandItem(item, player.heldItem);//ammo
                }
                else if (player.heldItem.itemSO.actionType == item.itemSO.actionType && item.itemSO.actionReward.Length != 0 || player.heldItem.itemSO.actionType == item.itemSO.actionType2 && item.itemSO.actionReward2.Length != 0)
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
        if (player.isHoldingItem)
        {
            if (player.heldItem.itemSO.needsAmmo)
            {
                player.heldItem.ammo--;
            }
            if (!item.itemSO.isWall)//we shouldnt punish players for wanting to make cleaner looking walls
            {
                player.UseHeldItem();
            }
            item.amount--;

            if (player.heldItem.itemSO.actionType == item.itemSO.actionType && item.itemSO.actionReward.Length != 0)
            {
                int i = 0;
                foreach (ItemSO _itemType in item.itemSO.actionReward)
                {
                    RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.actionReward[i], amount = 1 }, false);
                    i++;
                }
            }

            if (player.heldItem.itemSO.actionType == item.itemSO.actionType2 && item.itemSO.actionReward2.Length != 0)
            {
                int i = 0;
                foreach (ItemSO _itemType in item.itemSO.actionReward)
                {
                    RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.actionReward2[i], amount = 1 }, false);
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
            if (player.equippedHandItem.itemSO.needsAmmo)
            {
                player.equippedHandItem.ammo--;
            }
            if (!item.itemSO.isWall)//we shouldnt punish players for wanting to make cleaner looking walls
            {
                player.UseItemDurability();
            }
            item.amount--;

            if (player.equippedHandItem.itemSO.actionType == item.itemSO.actionType && item.itemSO.actionReward.Length != 0)
            {
                int i = 0;
                foreach (ItemSO _itemType in item.itemSO.actionReward)
                {
                    RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.actionReward[i], amount = 1 }, false);
                    i++;
                }
            }

            if (player.equippedHandItem.itemSO.actionType == item.itemSO.actionType2 && item.itemSO.actionReward2.Length != 0)
            {
                int i = 0;
                foreach (ItemSO _itemType in item.itemSO.actionReward)
                {
                    RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = item.itemSO.actionReward2[i], amount = 1 }, false);
                    i++;
                }
            }

            if (item.amount <= 0)
            {
                inventory.RemoveItemBySlot(itemSlotNumber);
            }
            int randVal = Random.Range(1, 4);
            audio.Play($"Chop{randVal}");
        }

    }

    private void StoreItem(int _reward)
    {
        if (item.itemSO.isBowl && player.heldItem.itemSO.isBowl)
        {
            Debug.Log("ADD BOWL");
            RealItem.SpawnRealItem(player.transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 }, false);
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
                txt.text = $"RMB / LMB: Deploy {item.itemSO.itemName}";
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
            if (player.isItemEquipped)//at bottom as a new if statement because of it being a special case where two options can be correct
            {
                if (player.equippedHandItem.itemSO.actionType == item.itemSO.actionType && item.itemSO.actionReward.Length != 0 || player.equippedHandItem.itemSO.actionType == item.itemSO.actionType2 && item.itemSO.actionReward2.Length != 0)
                {
                    if (item.itemSO.isDeployable)
                    {
                        txt.text = $"LMB: Deploy {item.itemSO.itemName}, RMB: {player.equippedHandItem.itemSO.actionType} {item.itemSO.itemName}";
                    }
                    else
                    {
                        txt.text = $"RMB: {player.equippedHandItem.itemSO.actionType} {item.itemSO.itemName}";
                    }
                }
            }
        }
        else if (player.isHoldingItem)
        {
            if (player.heldItem.itemSO.actionType == item.itemSO.actionType && item.itemSO.actionReward.Length != 0 || player.heldItem.itemSO.actionType == item.itemSO.actionType2 && item.itemSO.actionReward2.Length != 0)
            {
                txt.text = $"RMB: {player.heldItem.itemSO.actionType} {item.itemSO.itemName}";
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

    public void OnPointerExit(PointerEventData eventData)
    {
        txt.text = "";
    }
}
