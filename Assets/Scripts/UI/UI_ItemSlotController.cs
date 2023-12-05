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
        if (player.StateMachine.currentPlayerState == player.deployState)
        {
            Debug.Log("bye");
            return;
        }
            if (context.performed && selectedItemSlot != null && selectedItemSlot.item != null)
        {
            if (player.isHoldingItem)
            {
                if (IsCombinable1(player.heldItem, selectedItemSlot.item))
                {
                    selectedItemSlot.CombineItem(player.heldItem, 1);
                    player.UpdateHeldItemStats();
                }
                else if (IsCombinable2(player.heldItem, selectedItemSlot.item))
                {
                    selectedItemSlot.CombineItem(player.heldItem, 2);
                    player.UpdateHeldItemStats();
                }
                else if (selectedItemSlot.item.itemSO.needsAmmo && player.heldItem.itemSO == selectedItemSlot.item.itemSO.validAmmo)//load item
                {
                    selectedItemSlot.LoadItem();
                }
                else if (IsStorable(player.heldItem, selectedItemSlot.item))
                {
                    selectedItemSlot.StoreItem(player.heldItem);
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

    private bool IsStorable(Item item1, Item item2)//item 1 is held item or equipped
    {
        foreach(ItemSO item in item2.itemSO.validStorableItems)
        {
            if (item1.itemSO == item)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsCombinable1(Item item1, Item item2)
    {
        if (item1.itemSO.doActionType == 0)//default action isnt a real action
        {
            return false;
        }
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
        if (item1.itemSO.doActionType == 0)//default action isnt a real action
        {
            return false;
        }
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
            if (selectedItemSlot.isChestSlot)
            {
                Debug.Log("Chest slot used");
                var uiInv = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().chestUI.GetComponent<UI_Inventory>();
                uiInv.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
                return;
            }
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
