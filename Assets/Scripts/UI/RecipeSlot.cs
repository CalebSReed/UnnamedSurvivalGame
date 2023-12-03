using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecipeSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField]
    private Crafter crafter;
    public CraftingRecipes recipe;

    private PlayerMain player;

    [SerializeField] private UI_Crafter uiCrafter;

    private Inventory inventory = null;
    public UI_Inventory uiInv;

    private Transform reward;
    private Transform background;
    private Transform newRecipeNotification;
    private Transform outline;

    bool ingredient1Found = false;
    bool ingredient2Found = false;
    bool ingredient3Found = false;
    bool ingredient1IsEnough = false;
    bool ingredient2IsEnough = false;
    bool ingredient3IsEnough = false;

    public bool wasCrafted { get; set; }
    public bool isDiscovered { get; set; }

    Image rewardImage;
    Image backgroundImage;//unknown recipe sprite

    public void Start()
    {
        uiInv = GameObject.FindGameObjectWithTag("UI_Inventory").GetComponent<UI_Inventory>();
        uiInv.CheckDiscovery += OnItemCollected;

        background = transform.Find("Unknown");
        reward = transform.Find("Reward");
        newRecipeNotification = transform.Find("NewRecipeNotification");
        outline = transform.Find("Background");

        rewardImage = reward.Find("Image").GetComponent<Image>();
        rewardImage.sprite = recipe.reward.itemSprite;
        backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = UI_Assets.Instance.unknownRecipeSlot;
        inventory = uiInv.inventory;
        reward.gameObject.SetActive(false);
        newRecipeNotification.gameObject.SetActive(false);
        crafter.onCrafted += OnRecipeCrafted;
    }

    private void CheckDiscovery()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();
        if (player.freeCrafting)
        {
            background.gameObject.SetActive(false);
            reward.gameObject.SetActive(true);
            newRecipeNotification.gameObject.SetActive(false);
            isDiscovered = true;
        }

        if (!reward.gameObject.activeSelf && isDiscovered)
        {
            DiscoverRecipe();
        }

        if (wasCrafted && newRecipeNotification.gameObject.activeSelf)
        {
            DisableCraftingNotification();
        }

        ingredient1IsEnough = false;
        ingredient2IsEnough = false;
        ingredient3IsEnough = false;

        //Debug.Log("refreshing");
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
        if (ingredient1IsEnough && ingredient2IsEnough && ingredient3IsEnough || player.freeCrafting)
        {
            rewardImage.color = new Color(1, 1, 1);
        }
        else
        {
            rewardImage.color = new Color(.25f, .25f, .25f);
        }

        if (!isDiscovered)
        {
            if (inventory.GetItemTypeInInventory(recipe.baseItem) || inventory.GetItemTypeInInventory(recipe.reward))//no longer check if any are found, check to find the base element / item
            {
                DiscoverRecipe();
            }
        }
        //Debug.Log($"ing1 found: {ingredient1Found} ing2 found {ingredient2Found}, ing 3 found {ingredient3Found}, 1 {ingredient1IsEnough}, 2 {ingredient2IsEnough}, 3 {ingredient3IsEnough}");
    }

    public void OnItemCollected(object sender, EventArgs e)
    {
        CheckDiscovery();//maybe a small waiting period will help
        if (uiCrafter.recipe != null)
        {
            if (uiCrafter.recipe.description == recipe.description)//this would present a weird bug where having the same desc would have two items sending crafting data... 
            {
                //Debug.LogError("GO GO GO!!!");
                SendCraftingData();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDiscovered)
        {
            return;
        }
        if (eventData.clickCount >= 2 && eventData.button == PointerEventData.InputButton.Left)
        {
            StartCrafting();
        } 
        else if (eventData.clickCount == 1 && reward.gameObject.activeSelf == true && eventData.button == PointerEventData.InputButton.Left)
        {
            uiCrafter.SelectRecipe(gameObject);
            SendCraftingData();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDiscovered)
        {
            return;
        }
        uiCrafter.SetHighlightedRecipe(gameObject);
        SendCraftingData();
    }

    private void OnEnable()
    {
        if (isDiscovered)
        {
            DiscoverRecipe();
        }
    }

    private void SendCraftingData()
    {
        uiCrafter.GetCraftingData(ingredient1Found, ingredient2Found, ingredient3Found, ingredient1IsEnough, ingredient2IsEnough, ingredient3IsEnough, recipe.ingredient1, recipe.ingredient2, recipe.ingredient3, recipe, gameObject);
    }

    public void StartCrafting()
    {
        //Debug.Log("we gonna craft?");
        if (isDiscovered)
        {
            //Debug.Log("lets craft!");
            crafter.Craft(recipe.ingredient1, recipe.ingredient1Cost, recipe.ingredient2, recipe.ingredient2Cost, recipe.ingredient3, recipe.ingredient3Cost, new Item { itemSO = recipe.reward, amount = recipe.rewardAmount });
        }
    }

    public void DiscoverRecipe()
    {
        background = transform.Find("Unknown");//idk why i gotta do this again...
        reward = transform.Find("Reward");
        newRecipeNotification = transform.Find("NewRecipeNotification");
        if (background.gameObject != null)
        {
            background.gameObject.SetActive(false);
        }
        reward.gameObject.SetActive(true);
        if (!wasCrafted)
        {
            newRecipeNotification.gameObject.SetActive(true);
        }
        isDiscovered = true;
    }

    private void OnRecipeCrafted(object sender, CraftingArgs e)
    {
        if (e.rewardItem == recipe.reward)
        {
            wasCrafted = true;
            DisableCraftingNotification();
        }
    }

    public void DisableCraftingNotification()
    {
        newRecipeNotification.gameObject.SetActive(false);
    }

}
