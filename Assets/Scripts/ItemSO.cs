using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class ItemSO : ScriptableObject
{
    public GameObject prefab;
    public string itemType;
    public Sprite itemSprite;
    public bool isStackable = true;
    public int maxStackSize = 0;
    public bool isEquippable = false;
    public int maxUses = 0;
    public int damage = 0;
    public bool isDeployable = false;
    public WorldObject.worldObjectType deployObject;
    public Action.ActionType actionType;
    public bool canStoreItems = false;
    //public ItemSO[] validStorableItems;
    //public ItemSO[] storedItemReward;
    //public ItemSO[] actionReward;
    public bool needsToBeHot = false;
    public bool needsAmmo = false;
    //public ItemSO validAmmo;
    public int maxAmmo = 0;
    public Sprite loadedSprite;
    public Sprite loadedHandSprite;
    public Sprite aimingSprite;
    public bool isAmmo = false;
    public bool isSmeltable = false;
    public int smeltValue = 0;
    public int requiredSmeltingTime = 0;
    //public ItemSO smeltReward;
    public bool isFuel = false;
    public int fuelValue = 0;
    public int temperatureBurnValue = 0;
    public bool isEatable = false;
    public int[] restorationValues;
    public bool isCookable = false;
    //public ItemSO cookingReward;
    public bool isBowl = false;
}
