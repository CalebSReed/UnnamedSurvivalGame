using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobStateMachine
{
    public MobState currentMobState { get; set; }

    public void StartState(MobState _state)
    {
        currentMobState = _state;
        currentMobState.EnterState();
    }

    public void ChangeState(MobState _state)
    {
        currentMobState.ExitState();
        currentMobState = _state;
        currentMobState.EnterState();
    }
}
