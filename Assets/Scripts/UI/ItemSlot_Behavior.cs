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
                if (item.isEquippable())//if item is consumable, eatable, equipabble, etc...
                {
                    inventory.RemoveItemBySlot(itemSlotNumber);
                    txt.text = "";
                }
                else if (item.isEatable())
                {
                    if (item.amount == 1)
                    {
                        txt.text = "";
                    }
                    inventory.SubtractItem(item, itemSlotNumber);
                }
                else if (item.isDeployable())
                {
                    inventory.RemoveItemBySlot(itemSlotNumber);
                    txt.text = "";
                }
                player.UseItem(item);
            }
            else if (player.isHoldingItem)
            {
                if (item.NeedsAmmo() && item.ammo <= 0)
                {
                    player.CombineHandItem(item, player.heldItem);//ammo
                }
                else if (player.heldItem.GetDoableAction() == item.GetDoableAction() && item.GetActionReward() != Item.ItemType.Null)
                {
                    Debug.Log("CUTTING");
                    if (player.heldItem.NeedsAmmo() && player.heldItem.ammo > 0)//if needs ammo, check if has ammo to craft with
                    {
                        CombineItem();
                    }
                    else if (player.heldItem.NeedsAmmo() && player.heldItem.ammo <= 0)//if we dont have enough ammo to craft
                    {
                        Debug.LogError("NEEDS AMMO");
                    }
                    else//if we dont need ammo, attempt to craft
                    {
                        CombineItem();
                    }
                    
                }
            }
        }
    }

    private void CombineItem()
    {
        if (player.heldItem.NeedsAmmo())
        {
            player.heldItem.ammo--;
        }
        player.UseHeldItem();
        item.amount--;
        RealItem.SpawnRealItem(player.transform.position, new Item { itemType = item.GetActionReward(), amount = 1 });
        if (item.amount <= 0)
        {
            inventory.RemoveItemBySlot(itemSlotNumber);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!player.isHoldingItem)
        {
            if (item.isDeployable())
            {
                txt.text = $"RMB: Deploy {item.itemType}";
            }
            else if (item.isEatable())
            {
                txt.text = $"RMB: Eat {item.itemType}";
            }
            else if (item.isEquippable())
            {
                txt.text = $"RMB: Equip {item.itemType}";
            }
        }
        else if (player.isHoldingItem)
        {
            if (player.heldItem.GetDoableAction() == item.GetDoableAction() && item.GetActionReward() != Item.ItemType.Null)
            {
                txt.text = $"RMB: {player.heldItem.GetDoableAction()} {item.itemType}";
            }
            else if (item.NeedsAmmo() && player.heldItem.itemType == item.ValidAmmo())
            {
                txt.text = $"RMB: Load {item.itemType} with {player.heldItem.itemType}";
            }
        }
        else
        {
            txt.text = item.itemType.ToString();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        txt.text = "";
    }
}
