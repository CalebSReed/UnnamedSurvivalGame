﻿using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class ItemSO : ScriptableObject
{
    [Header("ITEM")]
    public string itemType;
    public Sprite itemSprite;
    [Space(20)]
    [Header("STACKING")]
    public bool isStackable = true;
    public int maxStackSize = 0;
    [Space(20)]
    [Header("EQUIPMENT")]
    public Action.ActionType actionType;
    public ItemSO[] actionReward;
    public bool isEquippable = false;
    public int maxUses = 0;
    public int damage = 0;
    [Space(20)]
    [Header("FOOD")]
    public bool isEatable = false;
    public int[] restorationValues;
    public bool isCookable = false;
    public ItemSO cookingReward;
    [Space(20)]
    [Header("SMELTING")]
    public bool isSmeltable = false;
    public int smeltValue = 0;
    public int requiredSmeltingTime = 0;
    public ItemSO smeltReward;
    public bool isFuel = false;
    public int fuelValue = 0;
    public int temperatureBurnValue = 0;
    public bool needsToBeHot = false;
    [Space(20)]
    [Header("DEPLOYING")]
    public bool isDeployable = false;
    public WOSO deployObject;
    [Space(20)]
    [Header("AMMO")]
    public bool needsAmmo = false;
    public ItemSO validAmmo;
    public int maxAmmo = 0;
    public Sprite loadedSprite;
    public Sprite loadedHandSprite;
    public Sprite aimingSprite;
    public bool isAmmo = false;
    [Space(20)]
    [Header("STORAGE")]
    public bool canStoreItems = false;
    public ItemSO[] validStorableItems;
    public ItemSO[] storedItemReward;
    [Space(20)]
    [Header("MISC")]
    public bool isBowl = false;
}