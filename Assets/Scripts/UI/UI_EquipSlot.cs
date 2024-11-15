﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_EquipSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private PlayerMain player;
    public Image itemSpr;
    public Light bodyLight;
    public SpriteRenderer bodySprite;
    public SpriteRenderer sideBodySprite;
    public SpriteRenderer backBodySprite;
    [SerializeField] private Hoverable hoverBehavior;
    public Item currentItem { get; private set; }
    public TextMeshProUGUI itemDataText;
    [SerializeField] public Item.EquipType slotEquipType;

    [SerializeField] private SpriteRenderer handSpr;
    [SerializeField] private SpriteRenderer headSpr;
    [SerializeField] private SpriteRenderer headSideSpr;
    [SerializeField] private SpriteRenderer headBackSpr;
    [SerializeField] private SpriteRenderer chestSpr;
    [SerializeField] private SpriteRenderer chestSideSpr;
    [SerializeField] private SpriteRenderer chestBackSpr;
    [SerializeField] private SpriteRenderer legSpr;
    [SerializeField] private SpriteRenderer footSpr;

    [SerializeField] private Image outline;

    private bool decreasingDurability;

    private void Start()
    {
        itemDataText.SetText("");
        //hoverTxt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();

        hoverBehavior.SpecialCase = true;
        hoverBehavior.specialCaseModifier.AddListener(CheckCurrentItem);

        SetBodyLight();
        SetBodySprite();
    }

    private void CheckCurrentItem()
    {
        if (currentItem != null)
        {
            hoverBehavior.Prefix = "RMB: Unequip ";
            hoverBehavior.Name = currentItem.itemSO.itemName;
        }
        else
        {
            Debug.Log("No item!");
            hoverBehavior.Prefix = "";
            hoverBehavior.Name = "";
        }
    }

    private void SetBodySprite()
    {
        switch (slotEquipType)
        {
            case Item.EquipType.HandGear:
                bodySprite = handSpr;
                break;
            case Item.EquipType.HeadGear:
                bodySprite = headSpr;
                sideBodySprite = headSideSpr;
                backBodySprite = headBackSpr;
                break;
            case Item.EquipType.ChestGear:
                bodySprite = chestSpr;
                sideBodySprite = chestSideSpr;
                backBodySprite = chestBackSpr;
                break;
            case Item.EquipType.LegGear:
                bodySprite = legSpr;
                break;
            case Item.EquipType.FootGear:
                bodySprite = footSpr;
                break;
        }
    }

    private void SetBodyLight()
    {
        switch (slotEquipType)
        {
            case Item.EquipType.HandGear:
                bodyLight = player.light2D;
                break;
            case Item.EquipType.HeadGear:
                bodyLight = player.headLight;
                break;
            case Item.EquipType.ChestGear:
                break;
            case Item.EquipType.LegGear:
                break;
            case Item.EquipType.FootGear:
                break;
        }
    }

    public void SetItem(Item _item)
    {
        UpdateSprite(_item.itemSO.itemSprite);
        bodySprite.sprite = _item.itemSO.itemSprite;
        if (sideBodySprite != null)
        {
            sideBodySprite.sprite = _item.itemSO.itemSprite;
        }
        if (backBodySprite != null)
        {
            backBodySprite.sprite = _item.itemSO.itemSprite;
        }
        itemSpr.color = new Color(1f, 1f, 1f, 1f);
        currentItem = _item;

        if (currentItem.itemSO.equipType == Item.EquipType.HandGear)
        {
            outline.color = Color.red;
        }

        UpdateDurability();
        if (currentItem.itemSO.doActionType == Action.ActionType.Till)
        {
            player.StateMachine.ChangeState(player.tillingState);
        }
        if (_item.itemSO.doActionType == Action.ActionType.Burn || _item.itemSO.insulationValue > 0 || _item.itemSO.rainProtectionValue > 0)
        {
            if (!decreasingDurability)
            {
                StartCoroutine(DecreaseItemUsesOverTime());
                decreasingDurability = true;
            }
        }
        else
        {
            decreasingDurability = false;
            StopAllCoroutines();
        }
    }

    public void RemoveItem()
    {
        if (slotEquipType == Item.EquipType.HandGear && currentItem.itemSO.doActionType == Action.ActionType.Till || slotEquipType == Item.EquipType.HandGear && currentItem.itemSO.doActionType == Action.ActionType.Shoot || slotEquipType == Item.EquipType.HandGear && currentItem.itemSO.doActionType == Action.ActionType.Throw)
        {
            player.StateMachine.ChangeState(player.defaultState);
        }
        itemSpr.color = new Color(0f, 0f, 0f, 0f);
        bodySprite.sprite = null;
        if (sideBodySprite != null)
        {
            sideBodySprite.sprite = null;
        }
        if (backBodySprite != null)
        {
            backBodySprite.sprite = null;
        }
        itemDataText.SetText("");
        currentItem = null;
        outline.color = Color.black;
        UpdateSlotBool(false);

        StopAllCoroutines();
        decreasingDurability = false;
    }

    public void UpdateSprite(Sprite spr)
    {
        itemSpr.sprite = spr;

        if (currentItem != null && currentItem.ammo > 0)
        {
            itemSpr.sprite = currentItem.itemSO.loadedSprite;
        }
    }

    public void UpdateSlotBool(bool isEquipped)
    {
        switch (slotEquipType)
        {
            case Item.EquipType.HandGear:
                player.isHandItemEquipped = isEquipped;
                break;
            case Item.EquipType.HeadGear:
                player.isHeadItemEquipped = isEquipped;
                break;
            case Item.EquipType.ChestGear:
                player.isChestItemEquipped = isEquipped;
                break;
            case Item.EquipType.LegGear:
                player.isLeggingItemEquipped = isEquipped;
                break;
            case Item.EquipType.FootGear:
                player.isFootItemEquipped = isEquipped;
                break;
        }
    }

    public void UpdateDurability()
    {
        if (currentItem.uses <= 0)
        {
            BreakItem();
            player.inventory.RefreshEmptySlots();
            return;
        }
        int newDurability;
        if (currentItem.amount > 1 && currentItem.itemSO.maxUses <= 1)
        {
            newDurability = currentItem.amount;
        }
        else
        {
            newDurability = Mathf.RoundToInt((float)currentItem.uses / currentItem.itemSO.maxUses * 100);//100 / 200 = .5 * 100 = 50%
        }
        itemDataText.SetText($"{newDurability}%");
        player.inventory.RefreshInventory();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (player.StateMachine.currentPlayerState == player.deployState)
        {
            Debug.Log("Bye");
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            /*if (player.isHoldingItem && currentItem == null && player.heldItem.equipType == slotEquipType)//equip
            {
                var item = player.heldItem;
                player.heldItem = null;
                player.StopHoldingItem();
                player.EquipItem(item, this);
            }
            else if (player.isHoldingItem && currentItem != null && player.heldItem.equipType == slotEquipType)//swap
            {
                Item _tempItem = currentItem;
                RemoveItem();
                player.EquipItem(player.heldItem, this);
                player.heldItem = null;
                player.StopHoldingItem();
                player.HoldItem(_tempItem);
            }
            else if (currentItem != null && !player.isHoldingItem)//unequip and hold
            {
                player.HoldItem(currentItem);
                player.UnequipItem(this, false);
            }*/
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Debug.Log("Middle click");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (player.isHoldingItem)
            {
                player.LoadHandItem(player.equippedHandItem, player.heldItem);
            }
            else
            {
                player.UnequipItem(this);
            }
        }
    }

    private void BreakItem()
    {
        //currentItem = null;
        Debug.Log("Equipped item broke");
        if (currentItem.itemSO.needsAmmo)
        {
            player.inventory.AddItem(new Item { itemSO = currentItem.itemSO.validAmmo, amount = currentItem.ammo}, player.transform.position, false);
        }
        player.UnequipItem(this, false);
    }

    private IEnumerator DecreaseItemUsesOverTime()
    {
        if (currentItem != null)
        {
            if (currentItem.itemSO.doActionType == Action.ActionType.Burn)
            {
                bodyLight.intensity = 100;
            }

            if (WeatherManager.Instance.isRaining)
            {
                currentItem.uses -= 2;

            }
            else
            {
                currentItem.uses--;
            }
            UpdateDurability();
        }
        else
        {
            Debug.Log("Equipped item is null cant decrease uses!");
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(DecreaseItemUsesOverTime());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }
}