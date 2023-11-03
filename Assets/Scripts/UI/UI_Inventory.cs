using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_Inventory : MonoBehaviour
{
    public Inventory inventory;//public for the recipe slots k thx.
    private Transform itemSlotContainer;
    private Transform itemSlot;

    public event EventHandler CheckDiscovery;

    private RealWorldObject obj;

    [SerializeField] private UI_CraftMenu_Controller uiCrafter;
    int width = 15;

    private void Awake()
    {
        itemSlotContainer = transform.Find("ItemSlotContainer");
        itemSlot = itemSlotContainer.Find("ItemSlot");
    }

    public void SetInventory(Inventory inventory, int invWidth = 16, RealWorldObject _obj = null)
    {
        this.inventory = inventory;
        inventory.OnItemListChanged += Inventory_OnItemListChanged;
        width = invWidth-1;

        if (obj != null && _obj != null && obj.IsContainerOpen() && obj != _obj)//if we have an obj attached that is open, and we are assigning an obj and prev obj is not same obj, close first obj's container and then we should enable uiInv object again
        {
            //Debug.LogError("closing prev obj");
            obj.CloseContainer();
        }

        if (_obj != null)//if we pass in an obj, set that obj
        {
            obj = _obj;
        }
        RefreshInventoryItems();
    }

    private void Inventory_OnItemListChanged(object sender, System.EventArgs e)
    {
        RefreshInventoryItems();
        //uiCrafter.RefreshCraftingMenuRecipes();//invoke another event    
    }

    private void RefreshRecipeSlots()
    {
        CheckDiscovery?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshInventoryItems()
    {
        Invoke(nameof(RefreshRecipeSlots), .01f);

        foreach (Transform child in itemSlotContainer)
        {
            if (child == itemSlot) continue;
            Destroy(child.gameObject);
        }

        int x = 0;
        int y = 0;
        float itemSlotSize = 120f;

        int i = 0;

        for(int index = 0; index < inventory.GetItemList().Length; index++)//inv width changes when placing items... and also closing container wont call
        {
            //Debug.Log("I'm here!");
            RectTransform itemSlotRectTransform = Instantiate(itemSlot, itemSlotContainer).GetComponent<RectTransform>();
            ItemSlot_Behavior itemsSlotBehavior = itemSlotRectTransform.GetComponent<ItemSlot_Behavior>();

            itemsSlotBehavior.inventory = inventory;
            itemsSlotBehavior.uiInventory = this;
            itemsSlotBehavior.itemSlotNumber = i;
            if (inventory.GetItemList()[index] == null)//if item does not exist
            {
                itemsSlotBehavior.item = null;

                itemSlotRectTransform.gameObject.SetActive(true);
                itemSlotRectTransform.anchoredPosition = new Vector2(x * itemSlotSize, y * itemSlotSize);
                Image image = itemSlotRectTransform.Find("Image").GetComponent<Image>();

                TextMeshProUGUI uiText = itemSlotRectTransform.Find("Amount Display").GetComponent<TextMeshProUGUI>();
                uiText.text = "";
                x++;
                if (x > width)//was 7 now double cuz we need more inventory bruv
                {
                    x = 0;
                    y--;
                }
                //Debug.Log("This slot is empty :)");
                i++;
            }
            else//item exists
            {
                itemsSlotBehavior.item = inventory.GetItemList()[index];
                //Debug.Log(itemsSlotBehavior.item);
                Item item = inventory.GetItemList()[index];

                itemSlotRectTransform.gameObject.SetActive(true);
                itemSlotRectTransform.anchoredPosition = new Vector2(x * itemSlotSize, y * itemSlotSize);
                Image image = itemSlotRectTransform.Find("Image").GetComponent<Image>();
                image.sprite = item.itemSO.itemSprite;
                if (item.ammo > 0)
                {
                    image.sprite = item.itemSO.loadedSprite;
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
                    int newUses = Mathf.RoundToInt((float)item.uses / item.itemSO.maxUses * 100);
                    uiText.SetText($"{newUses}%");
                }
                else if (!item.itemSO.isStackable && item.uses <= 0)
                {
                    uiText.SetText("");
                }
                x++;
                if (x > width)//was 7 now double cuz we need more inventory bruv
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

}
