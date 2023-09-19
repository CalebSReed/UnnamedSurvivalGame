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
    public List<ItemSO> lootTable;//add so we can make amount 
    public List<int> lootAmounts;
    public List<int> lootChances;//100 = 100%
}
