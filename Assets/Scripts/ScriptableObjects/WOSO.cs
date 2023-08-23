using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create New World Object", menuName = "World Object")]
public class WOSO : ScriptableObject
{
    [Header("OBJ INFO")]
    public string objType;
    public Sprite objSprite;
    public Action.ActionType objAction;
    public float maxUses;
    [Header("TRANSITIONS")]
    public bool willTransition;
    public WOSO objTransition;
    [Header("INTERACTABILITY")]
    public bool isInteractable;
    public bool hasAttachments;
    public ItemSO[] itemAttachments;
    //public WOSO[] attachmentWOSO;
    [Header("LISTS")]
    public ItemSO[] acceptableFuels;
    public ItemSO[] acceptableSmeltItems;
    [Header("SMELTING")]
    public int maxTemp;
    public int minTemp;
    public int maxFuel;
    [Header("BURNING")]
    public bool burns;
    public int lightRadius;
    [Header("LOOT")]
    public List<ItemSO> lootTable;
    public List<int> lootAmounts;
    public List<int> lootChances;//100 = 100%
}
