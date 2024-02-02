using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParasiteFactionData
{
    public Vector3 PlayerBase { get; set; }
    public Vector3 ParasiteBase { get; set; }
    public bool PlayerBaseExists { get; set; }
    public bool ParasiteBaseExists { get; set; }
    public int ParasiteTechLevel { get; set; }

    public bool checkingPlayerLocation;

    public float maxRaidHealth;

    public bool isRaidInProgress;

    public int raidCooldown = 0;

    public int raidDifficultyMult = 0;

    public int scouterDifficultyMult = 0;

    public int basePoints;
}
