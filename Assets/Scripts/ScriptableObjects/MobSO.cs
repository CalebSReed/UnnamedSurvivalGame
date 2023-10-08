using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create New Mob",menuName = "Mob")]
public class MobSO : ScriptableObject
{
    [Header("MOB INFO")]
    public string mobType;
    public Sprite mobSprite;
    public int maxHealth;
    public int damage;
    public int speed;
    public MobAggroType.AggroType aggroType;//plugin modular AI 
    public bool isSpecialAttacker;
    public float attentionSpan;
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
    public bool isScouter;
    public float scoutingRadius;
    [Header("LOOT")]
    public List<ItemSO> lootTable;//add so we can make amount 
    public List<int> lootAmounts;
    public List<int> lootChances;//100 = 100%
}
