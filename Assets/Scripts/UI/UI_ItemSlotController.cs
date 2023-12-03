using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_ItemSlotController : MonoBehaviour
{
    public ItemSlot_Behavior selectedItemSlot { get; private set; }
    [SerializeField] private PlayerMain player;

    public void OnUseButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed && selectedItemSlot != null && selectedItemSlot.item != null)
        {
            if (player.isHoldingItem)
            {
                if (IsCombinable1(player.heldItem, selectedItemSlot.item))
                {
                    selectedItemSlot.CombineItem(player.heldItem, 1);
                    player.UpdateHeldItemStats();
                }
                else if (selectedItemSlot.item.itemSO.needsAmmo && player.heldItem.itemSO == selectedItemSlot.item.itemSO.validAmmo)//load item
                {
                    selectedItemSlot.LoadItem();
                }
            }
            else
            {
                if (player.playerInput.PlayerDefault.SpecialModifier.ReadValue<int>() == 1)//if holding shift
                {
                    player.DropItem(selectedItemSlot.item);
                    player.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
                    return;
                }
                else if (player.isHandItemEquipped && IsCombinable1(player.equippedHandItem, selectedItemSlot.item))
                {
                    selectedItemSlot.CombineItem(player.equippedHandItem, 1);
                    player.UpdateEquippedItem(player.equippedHandItem, player.handSlot);
                }
                else if (player.isHandItemEquipped && IsCombinable2(player.equippedHandItem, selectedItemSlot.item))
                {
                    selectedItemSlot.CombineItem(player.equippedHandItem, 2);
                    player.UpdateEquippedItem(player.equippedHandItem, player.handSlot);
                }
                else if (selectedItemSlot.item.itemSO.isEatable || selectedItemSlot.item.itemSO.isEquippable || selectedItemSlot.item.itemSO.isDeployable)
                {
                    UseSelectedItemSlot();
                }
            }

        }
    }

    private bool IsCombinable1(Item item1, Item item2)
    {
        if (item1.itemSO.needsAmmo && item1.ammo <= 0)//dont craft if u need ammo to craft
        {
            return false;
        }
        if (item1.itemSO.doActionType == item2.itemSO.getActionType1)
        {
            return true;
        }
        return false;
    }

    private bool IsCombinable2(Item item1, Item item2)
    {
        if (item1.itemSO.needsAmmo && item1.ammo <= 0)
        {
            return false;
        }
        if (item1.itemSO.doActionType == item2.itemSO.getActionType2)
        {
            return true;
        }
        return false;
    }

    public void UseSelectedItemSlot()
    {
        player.UseItem(selectedItemSlot.item);
        if (!selectedItemSlot.item.itemSO.isEatable)
        {
            player.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
        }
        else
        {
            player.inventory.SubtractItem(selectedItemSlot.item, selectedItemSlot.itemSlotNumber);
        }
    }

    public void SelectItemSlot(ItemSlot_Behavior itemSlot)
    {
        selectedItemSlot = itemSlot;
    }

    public void DeSelectItemSlot()
    {
        selectedItemSlot = null;
    }
}
