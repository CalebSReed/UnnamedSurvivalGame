using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_ItemSlotController : MonoBehaviour
{
    public ItemSlot_Behavior hoverItemSlot { get; private set; }
    public ItemSlot_Behavior selectedItemSlot { get; private set; }
    private PlayerMain player;
    [SerializeField] public UI_Inventory UI_chest;
    [SerializeField] bool isHotBar;
    public bool waitOneFrame;//for input shenanigans like use event calling after cancel event calling causing deployed objects to get immediately redeployed

    private void Start()
    {
        GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;
    }

    private void Update()
    {
        if (waitOneFrame)
        {
            waitOneFrame = false;
        }
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        player = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
    }

    public void OnSelectButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed && hoverItemSlot != null && player.StateMachine.currentPlayerState != player.swingingState && player.StateMachine.currentPlayerState != player.deadState && player.StateMachine.currentPlayerState != player.waitingState)
        {
            if (player.playerInput.PlayerDefault.SpecialModifier.ReadValue<float>() == 1 && UI_chest.obj != null && UI_chest.obj.IsContainerOpen() && hoverItemSlot.item != null)
            {
                if (hoverItemSlot.isChestSlot)
                {
                    player.inventory.AddItem(hoverItemSlot.item, player.transform.position, false);
                    UI_chest.inventory.RemoveItemBySlot(hoverItemSlot.itemSlotNumber);
                }
                else
                {
                    UI_chest.inventory.AddItem(hoverItemSlot.item, UI_chest.obj.transform.position, false);
                    if (hoverItemSlot.item == player.equippedHandItem)
                    {
                        player.UnequipItem(Item.EquipType.HandGear, false);
                    }
                    player.inventory.RemoveItemBySlot(hoverItemSlot.itemSlotNumber);
                }
            }
            else
            {
                hoverItemSlot.OnAcceptButtonPressed();
            }
        }
    }

    public void OnUseButtonDown(InputAction.CallbackContext context)
    {
        if (waitOneFrame || player.StateMachine.currentPlayerState == player.deployState || player.StateMachine.currentPlayerState == player.swingingState || player.StateMachine.currentPlayerState == player.deadState || player.StateMachine.currentPlayerState == player.waitingState)
        {
            return;
        }
        if (context.performed && hoverItemSlot != null && hoverItemSlot.item != null)
        {
            if (player.isHoldingItem)
            {
                if (IsCombinable1(player.heldItem, hoverItemSlot.item))
                {
                    hoverItemSlot.CombineItem(player.heldItem, 1);
                    player.UpdateHeldItemStats();
                }
                else if (IsCombinable2(player.heldItem, hoverItemSlot.item))
                {
                    hoverItemSlot.CombineItem(player.heldItem, 2);
                    player.UpdateHeldItemStats();
                }
                else if (hoverItemSlot.item.itemSO.needsAmmo && player.heldItem.itemSO == hoverItemSlot.item.itemSO.validAmmo)//load item
                {
                    hoverItemSlot.LoadItem();
                }
                /*else if (IsStorable(player.heldItem, selectedItemSlot.item))
                {
                    selectedItemSlot.StoreItem(player.heldItem);
                }*/
            }
            else
            {
                if (player.playerInput.PlayerDefault.SecondSpecialModifier.ReadValue<float>() == 1 && !hoverItemSlot.isContainedSlot)//if holding left control
                {
                    player.DropItem(hoverItemSlot.item);
                    if (hoverItemSlot.isChestSlot)
                    {
                        UI_chest.inventory.RemoveItemBySlot(hoverItemSlot.itemSlotNumber);
                    }
                    else
                    {
                        player.inventory.RemoveItemBySlot(hoverItemSlot.itemSlotNumber);
                    }
                    return;
                }
                else if (player.isHandItemEquipped && player.equippedHandItem.heldItem != null)
                {
                    if (hoverItemSlot.item != null && hoverItemSlot.item.itemSO == player.equippedHandItem.heldItem.itemSO && hoverItemSlot.item.amount < hoverItemSlot.item.itemSO.maxStackSize && !player.equippedHandItem.heldItem.isHot)
                    {
                        player.inventory.GetItemList()[hoverItemSlot.itemSlotNumber].amount++;
                        player.equippedHandItem.heldItem = null;
                        player.RemoveContainedItem();
                        player.inventory.RefreshInventory();
                    }
                }
                else if (player.hasTongs && IsTongable(hoverItemSlot.item, player.equippedHandItem) && player.equippedHandItem.heldItem == null || player.hasTongs && hoverItemSlot.item.itemSO.isReheatable)
                {
                    player.equippedHandItem.heldItem = Item.DupeItem(hoverItemSlot.item);
                    player.equippedHandItem.heldItem.amount = 1;
                    player.UpdateContainedItem(player.equippedHandItem.heldItem);
                    hoverItemSlot.SubtractItem();
                }
                else if (player.isHandItemEquipped && IsCombinable1(player.equippedHandItem, hoverItemSlot.item))
                {
                    hoverItemSlot.CombineItem(player.equippedHandItem, 1);
                    player.equipmentManager.UpdateDurability(player.equipmentManager.handItem);
                }
                else if (player.isHandItemEquipped && IsCombinable2(player.equippedHandItem, hoverItemSlot.item))
                {
                    hoverItemSlot.CombineItem(player.equippedHandItem, 2);
                    player.equipmentManager.UpdateDurability(player.equipmentManager.handItem);
                }
                else if (hoverItemSlot.item != null && hoverItemSlot.item.itemSO.canStoreItems)
                {
                    if (!hoverItemSlot.isChestSlot)//Sync these items in multiplayer LATER!!
                    {
                        hoverItemSlot.ToggleContainer();
                    }
                }
                else if (hoverItemSlot.item.itemSO.isEatable || hoverItemSlot.item.itemSO.isEquippable || hoverItemSlot.item.itemSO.isDeployable)
                {
                    UseHoveredItemSlot();
                }
            }

        }
        else if (context.performed && player.isHandItemEquipped && player.equippedHandItem.heldItem != null)
        {
            if (hoverItemSlot != null && hoverItemSlot.item == null && !player.equippedHandItem.heldItem.isHot)
            {
                player.inventory.GetItemList().SetValue(player.equippedHandItem.heldItem, hoverItemSlot.itemSlotNumber);
                player.inventory.RefreshInventory();
                player.equippedHandItem.heldItem = null;
                player.RemoveContainedItem();
            }
        }
        else if (context.performed && selectedItemSlot.item != null)
        {
            if (selectedItemSlot.item.itemSO.isEatable || selectedItemSlot.item.itemSO.isEquippable || selectedItemSlot.item.itemSO.isDeployable)
            {
                player.UseItem(selectedItemSlot.item);

                if (selectedItemSlot.item.itemSO.isEatable)
                {
                    selectedItemSlot.SubtractItem();
                }
                else if (selectedItemSlot.item.itemSO.equipType != Item.EquipType.HandGear)
                {
                    player.inventory.RemoveItemBySlot(selectedItemSlot.itemSlotNumber);
                }
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

    public void UseHoveredItemSlot()
    {
        if (hoverItemSlot.isChestSlot && hoverItemSlot.item.itemSO.equipType == Item.EquipType.HandGear)
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

        player.UseItem(hoverItemSlot.item);

        if (hoverItemSlot.item == null)
        {
            return;
        }

        if (!hoverItemSlot.item.itemSO.isEatable)
        {
            if (hoverItemSlot.isChestSlot)
            {
                Debug.Log("Chest slot used");

                if (hoverItemSlot.item.itemSO.equipType == Item.EquipType.HandGear)
                {
                    player.inventory.AddItem(hoverItemSlot.item, transform.position, false);
                }

                var uiInv = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().chestUI.GetComponent<UI_Inventory>();
                uiInv.inventory.RemoveItemBySlot(hoverItemSlot.itemSlotNumber);
                return;
            }
            if (hoverItemSlot.item.itemSO.equipType != Item.EquipType.HandGear)
            {
                player.inventory.RemoveItemBySlot(hoverItemSlot.itemSlotNumber);
            }
        }
        else
        {
            if (hoverItemSlot.isChestSlot)
            {
                Debug.Log("Chest slot used");
                var uiInv = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().chestUI.GetComponent<UI_Inventory>();
                uiInv.inventory.SubtractItem(hoverItemSlot.item, hoverItemSlot.itemSlotNumber);
                return;
            }
            player.inventory.SubtractItem(hoverItemSlot.item, hoverItemSlot.itemSlotNumber);
        }
    }

    public void SelectItemSlot(ItemSlot_Behavior itemSlot)
    {
        selectedItemSlot = itemSlot;

        if (player.isHandItemEquipped)
        {
            player.UnequipItem(Item.EquipType.HandGear, false);
        }
        if (selectedItemSlot.item != null && selectedItemSlot.item.itemSO.isEquippable && selectedItemSlot.item.equipType == Item.EquipType.HandGear)
        {
            player.EquipItem(selectedItemSlot.item);
        }
        player.uiInventory.RefreshInventoryItems();
    }

    public void HoverItemSlot(ItemSlot_Behavior itemSlot)
    {
        hoverItemSlot = itemSlot;
    }

    public void UnHoverItemSlot()
    {
        hoverItemSlot = null;
    }
}
