using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Crafter : MonoBehaviour
{

    public CraftingRecipes recipe;
    //private UI_Assets uiAssets;


    private Transform ingredient1;
    private Transform ingredient2;
    private Transform ingredient3;
    private Transform reward;
    private Transform background;
    public bool discovered = false;

    [SerializeField]
    Crafter crafter;

    private void Start()
    {
        background = transform.Find("Background");
        ingredient1 = transform.Find("Ingredient1");
        ingredient2 = transform.Find("Ingredient2");
        ingredient3 = transform.Find("Ingredient3");
        reward = transform.Find("Reward");
        Image ingredient1Image = ingredient1.Find("Image").GetComponent<Image>();
        Image ingredient2Image = ingredient2.Find("Image").GetComponent<Image>();
        Image ingredient3Image = ingredient3.Find("Image").GetComponent<Image>();
        Image rewardImage = reward.Find("Image").GetComponent<Image>();

        Image backgroundImage = background.GetComponent<Image>();

        backgroundImage.sprite = UI_Assets.Instance.unknownRecipeBackground;

        ingredient1.gameObject.SetActive(false);
        ingredient2.gameObject.SetActive(false);
        ingredient3.gameObject.SetActive(false);
        reward.gameObject.SetActive(false);

        TextMeshProUGUI ingredient1Text = ingredient1.Find("Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ingredient2Text = ingredient2.Find("Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ingredient3Text = ingredient3.Find("Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI rewardText = reward.Find("Text").GetComponent<TextMeshProUGUI>();

        //Discovery(crafter.inventory.GetItemTypeInInventory(recipe.ingredient1));
        //Discovery(crafter.inventory.GetItemTypeInInventory(recipe.ingredient2));

        if (recipe.ingredient1Cost > 1)
        {
            ingredient1Text.SetText(recipe.ingredient1Cost.ToString());
        }
        else
        {
            ingredient1Text.SetText("");
        }
        if (recipe.ingredient2Cost > 1)
        {
            ingredient2Text.SetText(recipe.ingredient2Cost.ToString());
        }
        else
        {
            ingredient2Text.SetText("");
        }

        if (recipe.ingredient3Cost > 1)
        {
            ingredient3Text.SetText(recipe.ingredient3Cost.ToString());
        }
        else
        {
            ingredient3Text.SetText("");
        }

        if (recipe.rewardAmount > 1)
        {
            rewardText.SetText(recipe.rewardAmount.ToString());
        }
        else
        {
            rewardText.SetText("");
        }

        Item tempItem = new Item { itemSO = recipe.ingredient1, amount = recipe.ingredient1Cost };
        ingredient1Image.sprite = tempItem.itemSO.itemSprite;

        if (recipe.ingredient2 != null)
        {
            tempItem = new Item { itemSO = recipe.ingredient2, amount = recipe.ingredient2Cost };
            ingredient2Image.sprite = tempItem.itemSO.itemSprite;
        }

        if (recipe.ingredient3 != null)
        {
            tempItem = new Item { itemSO = recipe.ingredient3, amount = recipe.ingredient3Cost };
            ingredient3Image.sprite = tempItem.itemSO.itemSprite;
        }
        tempItem = new Item { itemSO = recipe.reward, amount = recipe.rewardAmount };
        rewardImage.sprite = tempItem.itemSO.itemSprite;

    }

    public void Discovery(bool isDiscovered)
    {
        if (isDiscovered)
        {
            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = UI_Assets.Instance.recipeWholeBackground;

            ingredient1.gameObject.SetActive(true);
            if (recipe.ingredient2 != null)
            {
                ingredient2.gameObject.SetActive(true);
            }
            if (recipe.ingredient3 != null)
            {
                ingredient3.gameObject.SetActive(true);
            }
            reward.gameObject.SetActive(true);
            discovered = true;
        }
    }

    public void StartCrafting()
    {
        Debug.Log("we gonna craft?");
        if (discovered)
        {
            Debug.Log("lets craft!");
            crafter.Craft(recipe.ingredient1, recipe.ingredient1Cost, recipe.ingredient2, recipe.ingredient2Cost, recipe.ingredient3, recipe.ingredient3Cost, new Item { itemSO = recipe.reward, amount = recipe.rewardAmount});
        }
    }

}
