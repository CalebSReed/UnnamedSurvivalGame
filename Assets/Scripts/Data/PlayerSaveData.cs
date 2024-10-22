﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSaveData
{
    public Vector3 playerPos;
    public float health;
    public float hunger;
    public int difficulty;
    public bool inAdrenalineMode;
    public bool inSlowMode;
    public float adrenalineProgress;
    public float adrenalineCountdown;
    public bool inEther;
    public bool shardReady;
    public float shardProgress;

    public Dictionary<int, string> playerInvTypes = new Dictionary<int, string>();
    public Dictionary<int, int> playerInvAmounts = new Dictionary<int, int>();
    public Dictionary<int, int> playerInvDurabilities = new Dictionary<int, int>();
    public Dictionary<int, int> playerInvAmmo = new Dictionary<int, int>();
    public Dictionary<int, string[]> playerInvContainedTypes = new Dictionary<int, string[]>();

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

    public MobSaveData mobRide;
}
