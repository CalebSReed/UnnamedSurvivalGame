using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobPassiveNeutralAI : MonoBehaviour//will flee when attacked
{
    private MobMovementBase mobMovement;

    private void Awake()
    {
        mobMovement = GetComponent<MobMovementBase>();
        GetComponent<HealthManager>().OnDamageTaken += FleeOnAttacked;
    }

    private void FleeOnAttacked(object sender, DamageArgs e)
    {
        mobMovement.target = e.senderObject;
        mobMovement.SwitchMovement(MobMovementBase.MovementOption.MoveAway);
    }
}
