using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState currentPlayerState { get; private set; }

    public void StartState(PlayerState _state)
    {
        currentPlayerState = _state;
        currentPlayerState.EnterState();
    }

    public void ChangeState(PlayerState _state)
    {
        currentPlayerState.ExitState();
        currentPlayerState = _state;
        currentPlayerState.EnterState();
    }
}
