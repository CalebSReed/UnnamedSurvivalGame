using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_CraftMenu_Controller : MonoBehaviour
{
    [SerializeField]
    private GameObject uiMenu;
    private Inventory inventory;

    private void Start()
    {
        RefreshCraftingMenuRecipes();
    }

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
    }

    public void RefreshCraftingMenuRecipes()
    {
        Debug.Log("refreshing");
        foreach (Transform child in uiMenu.transform)
        {
            UI_Crafter uiCrafter = child.GetComponent<UI_Crafter>();

            Transform ingredient1Child = child.Find("Ingredient1");
            Transform ingredient2Child = child.Find("Ingredient2");
            Transform ingredient3Child = child.Find("Ingredient3");
            Transform rewardChild = child.Find("Reward");
            Image image1 = ingredient1Child.Find("Image").GetComponent<Image>();
            Image image2 = ingredient2Child.Find("Image").GetComponent<Image>();
            Image image3 = ingredient3Child.Find("Image").GetComponent<Image>();

            image1.color = new Color(.25f, .25f, .25f);
            image2.color = new Color(.25f, .25f, .25f);
            image3.color = new Color(.25f, .25f, .25f);

            Image rewardImage = rewardChild.Find("Image").GetComponent<Image>();
            bool ingredient1Found = false;
            bool ingredient2Found = false;
            bool ingredient3Found = false;

            if (inventory.GetItemTypeInInventory(uiCrafter.recipe.ingredient1))//if we have item type discover it
            {
                if (!uiCrafter.discovered)
                {
                    uiCrafter.Discovery(true);
                }

                if (inventory.GetItemAmount(uiCrafter.recipe.ingredient1) >= uiCrafter.recipe.ingredient1Cost)//if we have enough of item, do not gray it out
                {
                    image1.color = new Color(1f, 1f, 1f);
                    ingredient1Found = true;
                }
                else//gray out
                {
                    image1.color = new Color(.25f, .25f, .25f);
                }
            }

            if (uiCrafter.recipe.ingredient2 != null)
            {
                if (inventory.GetItemTypeInInventory(uiCrafter.recipe.ingredient2))
                {
                    if (!uiCrafter.discovered)
                    {
                        uiCrafter.Discovery(true);
                    }

                    if (inventory.GetItemAmount(uiCrafter.recipe.ingredient2) >= uiCrafter.recipe.ingredient2Cost)
                    {
                        image2.color = new Color(1f, 1f, 1f);
                        ingredient2Found = true;
                    }
                    else//gray out
                    {
                        image2.color = new Color(.25f, .25f, .25f);
                    }
                }
            }

            if (uiCrafter.recipe.ingredient3 != null)
            {
                if (inventory.GetItemTypeInInventory(uiCrafter.recipe.ingredient3))
                {
                    if (!uiCrafter.discovered)
                    {
                        uiCrafter.Discovery(true);
                    }

                    if (inventory.GetItemAmount(uiCrafter.recipe.ingredient3) >= uiCrafter.recipe.ingredient3Cost)
                    {
                        image3.color = new Color(1f, 1f, 1f);
                        ingredient3Found = true;
                    }
                    else//gray out
                    {
                        image3.color = new Color(.25f, .25f, .25f);
                    }
                }
            }

            if (uiCrafter.recipe.ingredient2Cost == 0)
            {
                ingredient2Found = true;
            }
            if (uiCrafter.recipe.ingredient3Cost == 0)
            {
                ingredient3Found = true;
            }

            if (ingredient1Found && ingredient2Found && ingredient3Found)
            {
                rewardImage.color = new Color(1f, 1f, 1f);
            }
            else
            {
                rewardImage.color = new Color(.25f, .25f, .25f);
            }
        }
    }
}
