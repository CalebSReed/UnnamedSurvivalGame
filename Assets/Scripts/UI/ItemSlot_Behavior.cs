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
                }
                else if (item.isEatable())
                {
                    inventory.SubtractItem(item, itemSlotNumber);
                }
                else if (item.isDeployable())
                {
                    inventory.RemoveItemBySlot(itemSlotNumber);
                }
                player.UseItem(item);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        txt.text = item.itemType.ToString();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        txt.text = "";
    }
}
