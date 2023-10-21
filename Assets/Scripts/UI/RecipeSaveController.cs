using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class RecipeSaveController : MonoBehaviour
{
    public static RecipeSaveController Instance { get; private set; }
    public static DiscoverySaveData saveData = new DiscoverySaveData();
    public string recipeDiscoverySaveFileName;
    public string recipeCraftedSaveFileName;
    void Awake()
    {
        Instance = this;
        recipeDiscoverySaveFileName = Application.persistentDataPath + "/SaveFiles/RecipeDiscoveriesSave.json";
        recipeCraftedSaveFileName = Application.persistentDataPath + "/SaveFiles/RecipeCraftedSave.json";
    }

    public void SaveRecipes()
    {
        saveData.RecipeDiscoverySaves.Clear();
        saveData.RecipeCraftedSaves.Clear();
        int childIndex;
        for (childIndex = 0; childIndex < 5; childIndex++)
        {
            //Debug.Log(CalebUtils.FindChildrenWithTag(transform.GetChild(childIndex), "Recipe"));
            foreach (GameObject recipe in CalebUtils.FindChildrenWithTag(transform.GetChild(childIndex).GetChild(0), "Recipe"))
            {
                saveData.RecipeDiscoverySaves.Add(recipe.GetComponent<RecipeSlot>().recipe.name, recipe.GetComponent<RecipeSlot>().isDiscovered);
                saveData.RecipeCraftedSaves.Add(recipe.GetComponent<RecipeSlot>().recipe.name, recipe.GetComponent<RecipeSlot>().wasCrafted);
            }
        }
        var discoveryListJson = JsonConvert.SerializeObject(saveData.RecipeDiscoverySaves, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(recipeDiscoverySaveFileName, discoveryListJson);

        var craftedJson = JsonConvert.SerializeObject(saveData.RecipeCraftedSaves, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(recipeCraftedSaveFileName, craftedJson);
    }

    public void LoadRecipes()
    {
        if (File.Exists(recipeDiscoverySaveFileName))
        {
            var discoveryFile = File.ReadAllText(recipeDiscoverySaveFileName);
            var recipeDiscoveryListJson = JsonConvert.DeserializeObject<Dictionary<string, bool>>(discoveryFile);

            var craftedFile = File.ReadAllText(recipeCraftedSaveFileName);
            var craftedJson = JsonConvert.DeserializeObject<Dictionary<string, bool>>(craftedFile);
            saveData.RecipeDiscoverySaves = recipeDiscoveryListJson;
            saveData.RecipeCraftedSaves = craftedJson;

            int childIndex;
            for (childIndex = 0; childIndex < 5; childIndex++)
            {
                foreach (GameObject recipe in CalebUtils.FindChildrenWithTag(transform.GetChild(childIndex).GetChild(0), "Recipe"))
                {
                    var recipeSlot = recipe.GetComponent<RecipeSlot>();
                    recipeSlot.isDiscovered = saveData.RecipeDiscoverySaves[recipeSlot.recipe.name];
                    recipeSlot.wasCrafted = saveData.RecipeCraftedSaves[recipeSlot.recipe.name];
                    if (recipeSlot.isDiscovered && recipeSlot.gameObject.activeSelf)
                    {
                        recipeSlot.DiscoverRecipe();
                    }
                }
            }
        }
        else
        {
            Debug.LogError("No Recipe Save Found!");
        }
    }
}
