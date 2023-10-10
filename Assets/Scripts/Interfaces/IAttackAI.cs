using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackAI
{
    MobMovementBase mobMovement { get; set; }
    float atkRadius { get; set; }
    GameObject target { get; set; }
    bool attacking { get; set; }
    RealMob realMob { get; set; }
    void StartCombat(object sender, CombatArgs e);
}
