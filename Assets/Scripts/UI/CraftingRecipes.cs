using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Recipe")]
public class CraftingRecipes : ScriptableObject
{

    public new string name;
    [TextArea(10, 10)]
    public string description;

    [Space(10)]
    [Searchable]
    public ItemSO ingredient1;
    public int ingredient1Cost;
    [Space(10)]
    [Searchable]
    public ItemSO ingredient2;
    public int ingredient2Cost;
    [Space(10)]
    [Searchable]
    public ItemSO ingredient3;
    public int ingredient3Cost;
    [Space(10)]

    [Searchable]
    public ItemSO reward;
    [Space(10)]

    public int rewardAmount;


    //search foreach item inventory for first ingredient, then search for second, delete amount needed, restack them blah blah blah
    //add item crafted to inventory ezpz
    //maybe add some sort of interaction UI like mouse minigames??
    //store recipes as itemtype, item.amount, item (reward) idk sumn like that



}
