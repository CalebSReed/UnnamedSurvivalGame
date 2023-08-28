using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Crafter : MonoBehaviour
{

    private CraftingRecipes recipe;
    //private UI_Assets uiAssets;


    private Transform ingredient1;
    private Transform ingredient2;
    private Transform ingredient3;
    private Transform reward;
    private Transform background;
    public bool discovered = false;

    [SerializeField]
    Crafter crafter;

    Image ingredient1Image;
    Image ingredient2Image;
    Image ingredient3Image;
    Image rewardImage;

    Image backgroundImage;

    TextMeshProUGUI ingredient1Text;
    TextMeshProUGUI ingredient2Text;
    TextMeshProUGUI ingredient3Text;
    TextMeshProUGUI rewardText;

    private void Start()
    {
        //poo
    }

    public void GetCraftingData(bool ing1Found, bool ing2Found, bool ing3Found, bool ing1EnoughFound, bool ing2EnoughFound, bool ing3EnoughFound, ItemSO ing1, ItemSO ing2, ItemSO ing3, CraftingRecipes _recipe)
    {
        recipe = _recipe;

        ingredient1.gameObject.SetActive(ing1Found);//if false wont turn on
        ingredient1Image.sprite = ing1.itemSprite;

        if (ing1EnoughFound)
        {
            ingredient1Image.color = new Color(1,1,1);
        }
        else
        {
            ingredient1Image.color = new Color(.25f, .25f, .25f);
        }

        if (ing2 != null)
        {
            ingredient2.gameObject.SetActive(ing2Found);//if false wont turn on
            ingredient2Image.sprite = ing2.itemSprite;
            if (ing2EnoughFound)
            {
                ingredient2Image.color = new Color(1, 1, 1);
            }
            else
            {
                ingredient2Image.color = new Color(.25f, .25f, .25f);
            }
        }
        else
        {
            ingredient2.gameObject.SetActive(false);
        }

        if (ing3 != null)
        {
            ingredient3.gameObject.SetActive(ing3Found);//if false wont turn on
            ingredient3Image.sprite = ing3.itemSprite;
            if (ing3EnoughFound)
            {
                ingredient3Image.color = new Color(1, 1, 1);
            }
            else
            {
                ingredient3Image.color = new Color(.25f, .25f, .25f);
            }
        }
        else
        {
            ingredient3.gameObject.SetActive(false);
        }

        SetText(recipe);
    }

    private void SetText(CraftingRecipes recipe)
    {
        background = transform.Find("Background");
        ingredient1 = transform.Find("Ingredient1");
        ingredient2 = transform.Find("Ingredient2");
        ingredient3 = transform.Find("Ingredient3");
        reward = transform.Find("Reward");
        ingredient1Image = ingredient1.Find("Image").GetComponent<Image>();
        ingredient2Image = ingredient2.Find("Image").GetComponent<Image>();
        ingredient3Image = ingredient3.Find("Image").GetComponent<Image>();
        rewardImage = reward.Find("Image").GetComponent<Image>();

        backgroundImage = background.GetComponent<Image>();

        backgroundImage.sprite = UI_Assets.Instance.unknownRecipeBackground;

        ingredient1.gameObject.SetActive(false);
        ingredient2.gameObject.SetActive(false);
        ingredient3.gameObject.SetActive(false);
        reward.gameObject.SetActive(false);

        ingredient1Text = ingredient1.Find("Text").GetComponent<TextMeshProUGUI>();
        ingredient2Text = ingredient2.Find("Text").GetComponent<TextMeshProUGUI>();
        ingredient3Text = ingredient3.Find("Text").GetComponent<TextMeshProUGUI>();
        rewardText = reward.Find("Text").GetComponent<TextMeshProUGUI>();

        //Discovery(crafter.inventory.GetItemTypeInInventory(recipe.ingredient1));
        //Discovery(crafter.inventory.GetItemTypeInInventory(recipe.ingredient2));

        if (recipe.ingredient1Cost > 0)
        {
            ingredient1Text.SetText(recipe.ingredient1Cost.ToString());
        }
        else
        {
            ingredient1Text.SetText("");
        }
        if (recipe.ingredient2Cost > 0)
        {
            ingredient2Text.SetText(recipe.ingredient2Cost.ToString());
        }
        else
        {
            ingredient2Text.SetText("");
        }

        if (recipe.ingredient3Cost > 0)
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

        ingredient1Image.sprite = recipe.ingredient1.itemSprite;

        if (recipe.ingredient2 != null)
        {
            ingredient2Image.sprite = recipe.ingredient2.itemSprite;
        }

        if (recipe.ingredient3 != null)
        {
            ingredient3Image.sprite = recipe.ingredient3.itemSprite;
        }
        rewardImage.sprite = recipe.reward.itemSprite;
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
