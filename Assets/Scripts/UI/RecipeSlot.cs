using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecipeSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Crafter crafter;
    public CraftingRecipes recipe;

    [SerializeField] private UI_Crafter uiCrafter;

    private Inventory inventory = null;
    public UI_Inventory uiInv;

    private Transform reward;
    private Transform background;

    bool ingredient1Found = false;
    bool ingredient2Found = false;
    bool ingredient3Found = false;
    bool ingredient1IsEnough = false;
    bool ingredient2IsEnough = false;
    bool ingredient3IsEnough = false;


    private bool isDiscovered;

    Image rewardImage;
    Image backgroundImage;//unknown recipe sprite

    public void Start()
    {
        uiInv = GameObject.FindGameObjectWithTag("UI_Inventory").GetComponent<UI_Inventory>();
        uiInv.CheckDiscovery += OnItemCollected;

        background = transform.Find("Unknown");
        reward = transform.Find("Reward");

        rewardImage = reward.Find("Image").GetComponent<Image>();
        rewardImage.sprite = recipe.reward.itemSprite;
        backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = UI_Assets.Instance.unknownRecipeSlot;
        inventory = uiInv.inventory;
        reward.gameObject.SetActive(false);
    }

    private void CheckDiscovery()
    {
        ingredient1IsEnough = false;
        ingredient2IsEnough = false;
        ingredient3IsEnough = false;

        Debug.Log("refreshing");
        if (inventory.GetItemTypeInInventory(recipe.ingredient1))//if we have item type discover it
        {

            if (inventory.GetItemAmount(recipe.ingredient1) >= recipe.ingredient1Cost)//if we have enough of item, do not gray it out
            {
                ingredient1IsEnough = true;
                ingredient1Found = true;
            }
            else if (inventory.GetItemAmount(recipe.ingredient1) > 0 && recipe.ingredient1Cost > 0)
            {
                ingredient1Found = true;
            }
        }

        if (recipe.ingredient2 != null)
        {
            if (inventory.GetItemTypeInInventory(recipe.ingredient2))
            {
                if (inventory.GetItemAmount(recipe.ingredient2) >= recipe.ingredient2Cost)//if we have enough of item, do not gray it out
                {
                    ingredient2IsEnough = true;
                    ingredient2Found = true;
                }
                else if (inventory.GetItemAmount(recipe.ingredient2) > 0 && recipe.ingredient2Cost > 0)
                {
                    ingredient2Found = true;
                }
            }
        }
        else if (recipe.ingredient2Cost == 0)
        {
            ingredient2IsEnough = true;
            //ingredient2Found = true;
        }

        if (recipe.ingredient3 != null)
        {
            if (inventory.GetItemTypeInInventory(recipe.ingredient3))
            {
                if (inventory.GetItemAmount(recipe.ingredient3) >= recipe.ingredient3Cost)//if we have enough of item, do not gray it out
                {
                    ingredient3IsEnough = true;
                    ingredient3Found = true;
                }
                else if (inventory.GetItemAmount(recipe.ingredient3) > 0 && recipe.ingredient3Cost > 0)
                {
                    ingredient3Found = true;
                }
            }
        }
        else if (recipe.ingredient3Cost == 0)
        {
            ingredient3IsEnough = true;
            //ingredient3Found = true;
        }
        if (ingredient1IsEnough && ingredient2IsEnough && ingredient3IsEnough)
        {
            rewardImage.color = new Color(1, 1, 1);
        }
        else
        {
            rewardImage.color = new Color(.25f, .25f, .25f);
        }

        if (!isDiscovered)
        {
            if (ingredient1Found || ingredient2Found || ingredient3Found)
            {
                backgroundImage.sprite = null;
                reward.gameObject.SetActive(true);
                isDiscovered = true;
            }
        }
        Debug.Log($"ing1 found: {ingredient1Found} ing2 found {ingredient2Found}, ing 3 found {ingredient3Found}, 1 {ingredient1IsEnough}, 2 {ingredient2IsEnough}, 3 {ingredient3IsEnough}");
    }

    public void OnItemCollected(object sender, EventArgs e)
    {
        CheckDiscovery();//maybe a small waiting period will help
        if (uiCrafter.recipe != null)
        {
            if (uiCrafter.recipe.description == recipe.description)//this would present a weird bug where having the same desc would have two items sending crafting data... 
            {
                Debug.LogError("GO GO GO!!!");
                SendCraftingData();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount >= 2)
        {
            StartCrafting();
        } 
        else if (eventData.clickCount == 1 && reward.gameObject.activeSelf == true)
        {
            Debug.Log("Send that data, boys!");
            SendCraftingData();
        }
    }

    private void SendCraftingData()
    {
        uiCrafter.GetCraftingData(ingredient1Found, ingredient2Found, ingredient3Found, ingredient1IsEnough, ingredient2IsEnough, ingredient3IsEnough, recipe.ingredient1, recipe.ingredient2, recipe.ingredient3, recipe);
    }

    public void StartCrafting()
    {
        Debug.Log("we gonna craft?");
        if (isDiscovered)
        {
            Debug.Log("lets craft!");
            crafter.Craft(recipe.ingredient1, recipe.ingredient1Cost, recipe.ingredient2, recipe.ingredient2Cost, recipe.ingredient3, recipe.ingredient3Cost, new Item { itemSO = recipe.reward, amount = recipe.rewardAmount });
        }
    }
}
