using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UI_Crafter : MonoBehaviour
{
    public CraftingRecipes subRecipe1;
    public CraftingRecipes subRecipe2;
    public CraftingRecipes subRecipe3;

    public GameObject subButton1;
    public GameObject subButton2;
    public GameObject subButton3;

    public CraftingRecipes recipe { get; set; }
    //private UI_Assets uiAssets;
    [SerializeField] private UI_Inventory uiInv;

    private Inventory inventory;

    private Transform ingredient1;
    private Transform ingredient2;
    private Transform ingredient3;
    private Transform reward;
    private Transform background;
    //public bool discovered = false;

    [SerializeField]
    Crafter crafter;

    [SerializeField] private Button button;

    Image ingredient1Image;
    Image ingredient2Image;
    Image ingredient3Image;
    Image rewardImage;

    Image backgroundImage;

    TextMeshProUGUI ingredient1Text;
    TextMeshProUGUI ingredient2Text;
    TextMeshProUGUI ingredient3Text;
    TextMeshProUGUI rewardText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;

    GameObject selectedRecipe;
    GameObject highlightedRecipe;
    private bool recipeLocked;

    private void Start()
    {
        inventory = uiInv.inventory;
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
        backgroundImage.sprite = UI_Assets.Instance.recipeWholeBackground;

        ingredient1Text = ingredient1.Find("Text").GetComponent<TextMeshProUGUI>();
        ingredient2Text = ingredient2.Find("Text").GetComponent<TextMeshProUGUI>();
        ingredient3Text = ingredient3.Find("Text").GetComponent<TextMeshProUGUI>();
        rewardText = reward.Find("Text").GetComponent<TextMeshProUGUI>();

        ingredient1.gameObject.SetActive(false);
        ingredient2.gameObject.SetActive(false);
        ingredient3.gameObject.SetActive(false);
        reward.gameObject.SetActive(false);
        button.gameObject.SetActive(false);
    }

    public void CancelButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ResetCraftingData();
        }
    }

    public void SetHighlightedRecipe(GameObject newRecipe, bool Override = false)
    {
        if (recipeLocked && !Override)
        {
            return;
        }

        UnHighlightRecipe();

        highlightedRecipe = newRecipe;
        highlightedRecipe.transform.Find("Background").GetComponent<Image>().color = new Color(.75f, .75f, .75f);
    }

    public void SelectRecipe(GameObject newRecipe)
    {
        DeselectRecipe(false);
        recipeLocked = true;
        selectedRecipe = newRecipe;
        SetHighlightedRecipe(newRecipe, true);
        selectedRecipe.transform.localScale = new Vector3(.7f, .7f, .7f);
        selectedRecipe.transform.Find("Background").GetComponent<Image>().color = new Color(1, .5f, .5f);
    }

    private void UnHighlightRecipe()
    {
        if (highlightedRecipe != null)
        {
            highlightedRecipe.transform.Find("Background").GetComponent<Image>().color = Color.black;
            highlightedRecipe = null;
        }
    }

    private void DeselectRecipe(bool setLock = true)
    {
        if (setLock)
        {
            recipeLocked = false;
        }
        UnHighlightRecipe();
        if (selectedRecipe != null)
        {
            selectedRecipe.transform.localScale = new Vector3(.5f, .5f, 1);
            selectedRecipe.transform.Find("Background").GetComponent<Image>().color = Color.black;
            selectedRecipe = null;
            ResetCraftingData();
        }
    }

    public void ResetCraftingData()//called from tab clicking as well
    {
        recipeLocked = false;
        DeselectRecipe();
        recipe = null;
        button.gameObject.SetActive(false);
        ingredient1.gameObject.SetActive(false);
        ingredient2.gameObject.SetActive(false);
        ingredient3.gameObject.SetActive(false);
        ingredient1Image.sprite = null;
        ingredient2Image.sprite = null;
        ingredient3Image.sprite = null;
        reward.gameObject.SetActive(false);
        rewardImage.sprite = null;
        ingredient1Text.text = "";
        ingredient2Text.text = "";
        ingredient3Text.text = "";
        nameText.text = "";
        descText.text = "";
    }

    public void GetCraftingData(bool ing1Found, bool ing2Found, bool ing3Found, bool ing1EnoughFound, bool ing2EnoughFound, bool ing3EnoughFound, ItemSO ing1, ItemSO ing2, ItemSO ing3, CraftingRecipes _recipe, GameObject recipeSlot)
    {
        if (recipeLocked && recipeSlot != selectedRecipe)
        {
            return;
        }
        recipe = _recipe;
        button.gameObject.SetActive(true);
        ingredient1.gameObject.SetActive(true);
        ingredient1Image.sprite = ing1.itemSprite;

        //sub recipes
        if (ing1.itemRecipe != null)
        {
            subButton1.SetActive(true);
            subRecipe1 = ing1.itemRecipe;
        }
        else
        {
            subButton1.SetActive(false);
            subRecipe1 = null;
        }

        if (ing2 != null && ing2.itemRecipe != null)
        {
            subButton2.SetActive(true);
            subRecipe2 = ing2.itemRecipe;
        }
        else
        {
            subButton2.SetActive(false);
            subRecipe2 = null;
        }

        if (ing3 != null && ing3.itemRecipe != null)
        {
            subButton3.SetActive(true);
            subRecipe3 = ing3.itemRecipe;
        }
        else
        {
            subButton3.SetActive(false);
            subRecipe3 = null;
        }

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
            ingredient2.gameObject.SetActive(true);
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
            ingredient3.gameObject.SetActive(true);
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

        reward.gameObject.SetActive(true);

        if (ing1EnoughFound && ing2EnoughFound && ing3EnoughFound)
        {
            rewardImage.color = new Color(1, 1, 1);
        }
        else
        {
            rewardImage.color = new Color(.25f, .25f, .25f);
        }

        SetText(recipe);
    }

    private void SetText(CraftingRecipes recipe)
    {


        //Discovery(crafter.inventory.GetItemTypeInInventory(recipe.ingredient1));
        //Discovery(crafter.inventory.GetItemTypeInInventory(recipe.ingredient2));

        nameText.text = $"{recipe.name}";
        descText.text = $"{recipe.description}";

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
        //Debug.Log("lets craft!");
        crafter.Craft(recipe.ingredient1, recipe.ingredient1Cost, recipe.ingredient2, recipe.ingredient2Cost, recipe.ingredient3, recipe.ingredient3Cost, new Item { itemSO = recipe.reward, amount = recipe.rewardAmount});
    }

    public void StartCraftingSubRecipe1()
    {
        if (subRecipe1 != null)
        {
            crafter.Craft(subRecipe1.ingredient1, subRecipe1.ingredient1Cost, subRecipe1.ingredient2, subRecipe1.ingredient2Cost, subRecipe1.ingredient3, subRecipe1.ingredient3Cost, new Item { itemSO = subRecipe1.reward, amount = subRecipe1.rewardAmount });
        }
    }

    public void StartCraftingSubRecipe2()
    {
        if (subRecipe2 != null)
        {
            crafter.Craft(subRecipe2.ingredient1, subRecipe2.ingredient1Cost, subRecipe2.ingredient2, subRecipe2.ingredient2Cost, subRecipe2.ingredient3, subRecipe2.ingredient3Cost, new Item { itemSO = subRecipe2.reward, amount = subRecipe2.rewardAmount });
        }
    }

    public void StartCraftingSubRecipe3()
    {
        if (subRecipe3 != null)
        {
            crafter.Craft(subRecipe3.ingredient1, subRecipe3.ingredient1Cost, subRecipe3.ingredient2, subRecipe3.ingredient2Cost, subRecipe3.ingredient3, subRecipe3.ingredient3Cost, new Item { itemSO = subRecipe3.reward, amount = subRecipe3.rewardAmount });
        }
    }
}
