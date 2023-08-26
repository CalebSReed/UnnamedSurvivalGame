using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using TMPro;

public class UI_HandSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public TextMeshProUGUI textMeshProUGUI;
    public PlayerMain player;
    private TextMeshProUGUI txt;
    public Item item;

    private void Start()
    {
        textMeshProUGUI.SetText("");
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
    }

    public void SetItem(Item _item, int _durability)
    {
        UpdateSprite(_item.itemSO.itemSprite);
        image.color = new Color(1f, 1f, 1f, 1f);
        UpdateDurability(_durability);
        item = _item;
    }

    public void RemoveItem()
    {
        image.color = new Color(0f, 0f, 0f, 0f);
        textMeshProUGUI.SetText("");
        item = null;
    }

    public void UpdateSprite(Sprite spr)
    {
        image.sprite = spr;
    }

    public void UpdateDurability(int _durability)
    {
        textMeshProUGUI.SetText($"{_durability}");
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
                player.UnequipItem();
            }
        }
    }

    public void ResetHoverText()
    {
        if (item != null)
        {
            if (player.heldItem != null)
            {
                if (item.itemSO.needsAmmo && item.ammo == 0 && player.heldItem.itemSO.isAmmo)
                {
                    txt.text = $"Load {item.itemSO.itemType} with {player.heldItem.itemSO.itemType}";
                }
                else
                {
                    txt.text = "Unequip";
                }
            }
            else
            {
                txt.text = "Unequip";
            }
        }
        else
        {
            txt.text = "";
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
        txt.text = "";
    }
}
