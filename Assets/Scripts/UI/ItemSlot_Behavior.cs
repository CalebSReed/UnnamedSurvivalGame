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
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
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
