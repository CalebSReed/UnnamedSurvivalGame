﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create New Mob",menuName = "Mob")]
public class MobSO : ScriptableObject
{
    [Header("MOB INFO")]
    public string mobName;
    public string mobType;
    public Sprite mobSprite;
    public int maxHealth;
    public int damage;
    public float shardCharge;
    public int walkSpeed;
    public int runSpeed;
    public float hurtBoxRadius;
    public float hurtBoxYOffset;
    public bool heavyWeight;//has a poise meter instead of always getting stunned from heavy attacks
    public bool isVampire;
    public bool talks;
    public Action.ActionType getActionType;
    public List<ItemSO> actionRewards;
    public MobSO mobTransition;
    public MobSO winterVariant;
    public List<ItemSO> acceptableItems;
    [Space(20)]
    public MobAggroType.AggroType aggroType;//plugin modular AI 
    public MobMovementBase.MovementOption aggroStrategy;
    public bool isSpecialAttacker;
    public float attentionSpan;
    public IAttackAI attackAI;
    [Header("Animation")]
    public RuntimeAnimatorController anim;
    [Header("PREDATORS AND PREY")]
    public List<string> predators;
    public List<string> prey;
    [Space(20)]
    public float preyDetectionRadius;
    public float abandonRadius;
    public float combatRadius;
    [Space(20)]
    public float predatorDetectionRadius;
    public float escapeRadius;
    [Header("PARASITES")]
    public bool isParasite;
    public bool isRaidParasite;
    public bool isScouter;
    public float scoutingRadius;
    [Header("LOOT")]
    public List<ItemSO> lootTable;//add so we can make amount 
    public List<int> lootAmounts;
    public List<int> lootChances;//100 = 100%
}
