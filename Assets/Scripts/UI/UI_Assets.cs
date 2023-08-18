using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Assets : MonoBehaviour
{
    public static UI_Assets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Sprite recipeWholeBackground;
    public Sprite recipeSlotBackground;
    public Sprite unknownRecipeBackground;
    public Sprite unknownRecipeSlot;

    /*public Sprite GetSprite()
    {
        switch (string)
        {
            default:
            case "background" :  return recipeWholeBackground;
            case "unknownBackground" : return unknownRecipeBackground;
        }
    }*/

}
