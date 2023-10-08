using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackAI
{
    float atkRadius { get; set; }
    GameObject target { get; set; }
    bool attacking { get; set; }
    void StartCombat(object sender, CombatArgs e);
}
