using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_Inventory : MonoBehaviour
{
    private Inventory inventory;
    private Transform itemSlotContainer;
    private Transform itemSlot;
    [SerializeField] private UI_CraftMenu_Controller uiCrafter;

    private void Awake()
    {
        itemSlotContainer = transform.Find("ItemSlotContainer");
        itemSlot = itemSlotContainer.Find("ItemSlot");
    }

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;

        inventory.OnItemListChanged += Inventory_OnItemListChanged;
        RefreshInventoryItems();
    }

    private void Inventory_OnItemListChanged(object sender, System.EventArgs e)
    {
        RefreshInventoryItems();
        uiCrafter.RefreshCraftingMenuRecipes();
    }

    public void RefreshInventoryItems()
    {
        foreach (Transform child in itemSlotContainer)
        {
            if (child == itemSlot) continue;
            Destroy(child.gameObject);
        }

        int x = 0;
        int y = 0;
        float itemSlotSize = 120f;

        int i = 0;

        foreach (Item item in inventory.GetItemList())
        {
            RectTransform itemSlotRectTransform = Instantiate(itemSlot, itemSlotContainer).GetComponent<RectTransform>();
            ItemSlot_Behavior itemsSlotBehavior = itemSlotRectTransform.GetComponent<ItemSlot_Behavior>();

            itemsSlotBehavior.inventory = inventory;
            itemsSlotBehavior.itemSlotNumber = i;
            itemsSlotBehavior.item = item;

            itemSlotRectTransform.gameObject.SetActive(true);
            itemSlotRectTransform.anchoredPosition = new Vector2(x * itemSlotSize, y * itemSlotSize);
            Image image = itemSlotRectTransform.Find("Image").GetComponent<Image>();
            image.sprite = item.itemSO.itemSprite;
            if (item.ammo > 0)
            {
                image.sprite = item.GetLoadedSprite();
            }
            image.color = new Color(1f, 1f, 1f, 1f);
            TextMeshProUGUI uiText = itemSlotRectTransform.Find("Amount Display").GetComponent<TextMeshProUGUI>();
            if (item.amount > 1 && item.itemSO.isStackable)
            {
                uiText.SetText(item.amount.ToString());
            }
            else
            {
                uiText.SetText("");
            }
            if (!item.itemSO.isStackable && item.uses > 0)
            {
                uiText.SetText(item.uses.ToString());
            }
            else if (!item.itemSO.isStackable && item.uses <= 0)
            {
                uiText.SetText("");
            }
            x++;
            if (x > 7)
            {
                x = 0;
                y--;
            }

            if (item.amount == 0)
            {
                //inventory.RemoveItemBySlot(i);
                Debug.LogError("EMPTY ITEM");
            }

            i++;
        }
    }

}
