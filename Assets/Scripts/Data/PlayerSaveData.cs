using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSaveData
{
    public Vector3 playerPos;
    public float health;
    public int hunger;
    public int difficulty;

    public Dictionary<int, string> playerInvTypes = new Dictionary<int, string>();
    public Dictionary<int, int> playerInvAmounts = new Dictionary<int, int>();
    public Dictionary<int, int> playerInvDurabilities = new Dictionary<int, int>();
    public Dictionary<int, int> playerInvAmmo = new Dictionary<int, int>();

    public string handItemType;
    public int handItemAmount;
    public int handItemUses;
    public int handItemAmmo;

    public string headItemType;
    public int headItemAmount;
    public int headItemUses;
    public int headItemAmmo;

    public string chestItemType;
    public int chestItemAmount;
    public int chestItemUses;
    public int chestItemAmmo;

    public string legsItemType;
    public int legsItemAmount;
    public int legsItemUses;
    public int legsItemAmmo;

    public string feetItemType;
    public int feetItemAmount;
    public int feetItemUses;
    public int feetItemAmmo;

}
