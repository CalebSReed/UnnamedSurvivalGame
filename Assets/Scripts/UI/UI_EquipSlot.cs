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
        UpdateDurability(_item.uses);
    }

    public void RemoveItem()
    {
        itemSpr.color = new Color(0f, 0f, 0f, 0f);
        itemDataText.SetText("");
        currentItem = null;
    }

    public void UpdateSprite(Sprite spr)
    {
        itemSpr.sprite = spr;
    }

    public void UpdateDurability(int _durability)
    {
        int newDurability = Mathf.RoundToInt((float)_durability / currentItem.itemSO.maxUses * 100);//100 / 200 = .5 * 100 = 50%
        itemDataText.SetText($"{newDurability}%");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Debug.Log("Left click");
        else if (eventData.button == PointerEventData.InputButton.Middle)
            Debug.Log("Middle click");
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (player.isHoldingItem)
            {
                //Debug.LogError("COMBINING");
                player.CombineHandItem(player.equippedHandItem, player.heldItem);
            }
            else
            {
                //Debug.LogError("UNEQUIPPING");
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        player.hoveringOverSlot = true;
        ResetHoverText();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        player.hoveringOverSlot = false;
        hoverTxt.text = "";
    }
}