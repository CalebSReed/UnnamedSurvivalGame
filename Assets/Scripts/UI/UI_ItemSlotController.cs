using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_ItemSlotController : MonoBehaviour
{
    public ItemSlot_Behavior selectedItemSlot { get; private set; }
    private PlayerMain player;
    [SerializeField] public UI_Inventory UI_chest;

    private void Start()
    {
        GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        player = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
    }

    public void OnSelectButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed && selectedItemSlot != null && player.StateMachine.currentPlayerState != player.swingingState && player.StateMachine.currentPlayerState != player.deadState && player.StateMachine.currentPlayerState != player.waitingState)
        {
            if (player.playerInput.PlayerDefault.SpecialModifier.ReadValue<float>() == 1 && UI_chest.obj != null && UI_chest.obj.IsContainerOpen() && selectedItemSlot.item != null)
            {
                if (selectedItemSlot.isChestSlot)
                {
                    player.inventory.AddItem(selectedItemSlot.item, player.transform.position, false);
                    UI_chest.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
                }
                else
                {
                    UI_chest.inventory.AddItem(selectedItemSlot.item, UI_chest.obj.transform.position, false);
                    if (selectedItemSlot.item == player.equippedHandItem)
                    {
                        player.UnequipItem(Item.EquipType.HandGear, false);
                    }
                    player.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
                }
            }
            else
            {
                selectedItemSlot.OnAcceptButtonPressed();
            }
        }
    }

    public void OnUseButtonDown(InputAction.CallbackContext context)
    {
        if (player.StateMachine.currentPlayerState == player.deployState || player.StateMachine.currentPlayerState == player.swingingState || player.StateMachine.currentPlayerState == player.deadState || player.StateMachine.currentPlayerState == player.waitingState)
        {
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
                /*else if (IsStorable(player.heldItem, selectedItemSlot.item))
                {
                    selectedItemSlot.StoreItem(player.heldItem);
                }*/
            }
            else
            {
                if (player.playerInput.PlayerDefault.SecondSpecialModifier.ReadValue<float>() == 1 && !selectedItemSlot.isContainedSlot)//if holding left control
                {
                    player.DropItem(selectedItemSlot.item);
                    if (selectedItemSlot.isChestSlot)
                    {
                        UI_chest.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
                    }
                    else
                    {
                        player.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
                    }
                    return;
                }
                else if (player.isHandItemEquipped && player.equippedHandItem.heldItem != null)
                {
                    if (selectedItemSlot.item != null && selectedItemSlot.item.itemSO == player.equippedHandItem.heldItem.itemSO && selectedItemSlot.item.amount < selectedItemSlot.item.itemSO.maxStackSize && !player.equippedHandItem.heldItem.isHot)
                    {
                        player.inventory.GetItemList()[selectedItemSlot.itemSlotNumber].amount++;
                        player.equippedHandItem.heldItem = null;
                        player.RemoveContainedItem();
                        player.inventory.RefreshInventory();
                    }
                }
                else if (player.hasTongs && IsTongable(selectedItemSlot.item, player.equippedHandItem) && player.equippedHandItem.heldItem == null || player.hasTongs && selectedItemSlot.item.itemSO.isReheatable)
                {
                    player.equippedHandItem.heldItem = Item.DupeItem(selectedItemSlot.item);
                    player.equippedHandItem.heldItem.amount = 1;
                    player.UpdateContainedItem(player.equippedHandItem.heldItem);
                    selectedItemSlot.SubtractItem();
                }
                else if (player.isHandItemEquipped && IsCombinable1(player.equippedHandItem, selectedItemSlot.item))
                {
                    selectedItemSlot.CombineItem(player.equippedHandItem, 1);
                    player.equipmentManager.UpdateDurability(player.equipmentManager.handItem);
                }
                else if (player.isHandItemEquipped && IsCombinable2(player.equippedHandItem, selectedItemSlot.item))
                {
                    selectedItemSlot.CombineItem(player.equippedHandItem, 2);
                    player.equipmentManager.UpdateDurability(player.equipmentManager.handItem);
                }
                else if (selectedItemSlot.item != null && selectedItemSlot.item.itemSO.canStoreItems)
                {
                    if (!selectedItemSlot.isChestSlot)//Sync these items in multiplayer LATER!!
                    {
                        selectedItemSlot.ToggleContainer();
                    }
                }
                else if (selectedItemSlot.item.itemSO.isEatable || selectedItemSlot.item.itemSO.isEquippable || selectedItemSlot.item.itemSO.isDeployable)
                {
                    UseSelectedItemSlot();
                }
            }

        }
        else if (context.performed && player.isHandItemEquipped && player.equippedHandItem.heldItem != null)
        {
            if (selectedItemSlot != null && selectedItemSlot.item == null && !player.equippedHandItem.heldItem.isHot)
            {
                player.inventory.GetItemList().SetValue(player.equippedHandItem.heldItem, selectedItemSlot.itemSlotNumber);
                player.inventory.RefreshInventory();
                player.equippedHandItem.heldItem = null;
                player.RemoveContainedItem();
            }
        }
    }

    public static bool IsTongable(Item item1, Item item2)
    {
        foreach (ItemSO item in item2.itemSO.validStorableItems)
        {
            if (item1.itemSO == item)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsStorable(Item item1, Item item2)//item 1 is held item or equipped most of the time
    {
        if (!item2.itemSO.canStoreItems)
        {
            return false;
        }

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
        if (item1.itemSO.needsToBeHot && !item1.isHot)
        {
            return false;
        }
        else if (item2.itemSO.needsToBeHot && !item2.isHot)
        {
            return false;
        }
        else if (item1.itemSO.doActionType == 0)//default action isnt a real action
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
        /*if (item1.itemSO.needsToBeHot && !item1.isHot)    //1st action is prob always hammer, 2nd is prob always secondary like chisel for file etc, make this specific later
        {
            return false;
        }
        else if (item2.itemSO.needsToBeHot && !item2.isHot)
        {
            return false;
        }*/
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
        if (selectedItemSlot.isChestSlot && selectedItemSlot.item.itemSO.equipType == Item.EquipType.HandGear)
        {
            if (!player.inventory.InventoryHasOpenSlot())
            {
                return;
            }
        }

        if (player.currentlyRiding || player.StateMachine.currentPlayerState == player.deadState)
        {
            return;
        }

        player.UseItem(selectedItemSlot.item);

        if (selectedItemSlot.item == null)
        {
            return;
        }

        if (!selectedItemSlot.item.itemSO.isEatable)
        {
            if (selectedItemSlot.isChestSlot)
            {
                Debug.Log("Chest slot used");

                if (selectedItemSlot.item.itemSO.equipType == Item.EquipType.HandGear)
                {
                    player.inventory.AddItem(selectedItemSlot.item, transform.position, false);
                }

                var uiInv = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().chestUI.GetComponent<UI_Inventory>();
                uiInv.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
                return;
            }
            if (selectedItemSlot.item.itemSO.equipType != Item.EquipType.HandGear)
            {
                player.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
            }
        }
        else
        {
            if (selectedItemSlot.isChestSlot)
            {
                Debug.Log("Chest slot used");
                var uiInv = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().chestUI.GetComponent<UI_Inventory>();
                uiInv.inventory.SubtractItem(selectedItemSlot.item, selectedItemSlot.itemSlotNumber);
                return;
            }
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
