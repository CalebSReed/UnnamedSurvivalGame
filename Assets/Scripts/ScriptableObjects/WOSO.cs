﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create New World Object", menuName = "World Object")]
public class WOSO : ScriptableObject
{
    [Header("OBJ INFO")]
    public string objName;
    public string objType;
    public int objID;
    public Sprite objSprite;
    public Action.ActionType objAction;
    public int maxHealth;//we use health for working with getting attacked (only targetable by non-players)
    public float maxUses;//we use uses for working with items.
    public bool isPlayerMade = false;
    public bool isParasiteMade = false;
    public int basePoints;
    public bool isCollidable = true;
    public bool doesDamage;
    public int damage;
    public bool isTemporary;
    public float lifeTime;
    [Header("TRANSITIONS")]
    public bool willTransition;
    public WOSO[] objTransitions;
    public WOSO winterVariant;
    [Header("INTERACTABILITY")]
    public bool isInteractable;
    public bool isContainer;
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
    public int baseTemp;
    [Header("BURNING")]
    public bool glows;
    public bool burns;
    public int lightRadius;
    public int temperatureBurn;
    public int temperatureRadius;
    [Header("LOOT")]
    public List<ItemSO> lootTable;
    public List<int> lootAmounts;
    public List<int> lootChances;//100 = 100%
    [Header("WALLS AND FLOORS")]
    public string objectPrefab;
    public bool isHWall;//horizontal
    public bool isVWall;//vertical
    public bool isCWall;//center
    public bool isDoor;
    public bool isFloor;
    public bool isMirrored;
    [Header("MISC")]
    public ItemSO seed;
}
