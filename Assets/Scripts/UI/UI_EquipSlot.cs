using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_EquipSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private PlayerMain player;
    public Image itemSpr;
    public Item currentItem { get; private set; }
    public TextMeshProUGUI itemDataText;
    private TextMeshProUGUI hoverTxt;
    [SerializeField] private Item.EquipType slotEquipType;

    private void Start()
    {
        itemDataText.SetText("");
        hoverTxt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();
    }

    public void SetItem(Item _item)
    {
        UpdateSprite(_item.itemSO.itemSprite);
        itemSpr.color = new Color(1f, 1f, 1f, 1f);
        currentItem = _item;
        UpdateDurability();
        if (currentItem.itemSO.doActionType == Action.ActionType.Till)
        {
            player.StateMachine.ChangeState(player.tillingState);
        }
        if (_item.itemSO.doActionType == Action.ActionType.Burn || _item.itemSO.insulationValue > 0 || _item.itemSO.rainProtectionValue > 0)
        {
            StartCoroutine(DecreaseItemUsesOverTime());
        }
        else
        {
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
        itemDataText.SetText("");
        currentItem = null;


        StopAllCoroutines();
    }

    public void UpdateSprite(Sprite spr)
    {
        itemSpr.sprite = spr;
    }

    public void UpdateDurability()
    {
        if (currentItem.uses <= 0)
        {
            BreakItem();
            return;
        }
        int newDurability = Mathf.RoundToInt((float)currentItem.uses / currentItem.itemSO.maxUses * 100);//100 / 200 = .5 * 100 = 50%
        itemDataText.SetText($"{newDurability}%");
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
            if (player.isHoldingItem && currentItem == null && player.heldItem.equipType == slotEquipType)//equip
            {
                var item = player.heldItem;
                player.heldItem = null;
                player.StopHoldingItem();
                player.EquipItem(item);
            }
            else if (player.isHoldingItem && currentItem != null && player.heldItem.equipType == slotEquipType)//swap
            {
                Item _tempItem = currentItem;
                RemoveItem();
                player.EquipItem(player.heldItem);
                player.heldItem = null;
                player.StopHoldingItem();
                player.HoldItem(_tempItem);
            }
            else if (currentItem != null && !player.isHoldingItem)//unequip and hold
            {
                player.HoldItem(currentItem);
                player.UnequipItem(this, false);
            }
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

    public void ResetHoverText()
    {
        if (currentItem != null)
        {
            if (player.heldItem != null)
            {
                if (currentItem.itemSO.needsAmmo && currentItem.ammo == 0 && player.heldItem.itemSO.isAmmo)
                {
                    hoverTxt.text = $"RMB: Load {currentItem.itemSO.itemType} with {player.heldItem.itemSO.itemType}";
                }
                else
                {
                    hoverTxt.text = "RMB: Unequip";
                }
            }
            else
            {
                hoverTxt.text = "RMB: Unequip";
            }
        }
        else
        {
            hoverTxt.text = "";
        }
    }

    private void BreakItem()
    {
        //currentItem = null;
        Debug.Log("Equipped item broke");
        player.UnequipItem(this, false);
    }

    private IEnumerator DecreaseItemUsesOverTime()
    {
        if (currentItem != null)
        {
            currentItem.uses--;
            UpdateDurability();
            if (currentItem.uses <= 0)
            {
                BreakItem();
            }
        }
        else
        {
            Debug.LogError("Equipped item is null cant decrease uses!");
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(DecreaseItemUsesOverTime());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ResetHoverText();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverTxt.text = "";
    }
}