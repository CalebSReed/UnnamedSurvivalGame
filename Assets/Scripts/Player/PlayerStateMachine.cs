using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerMain player;
    public PlayerState currentPlayerState { get; private set; }
    public PlayerState previousPlayerState { get; private set; }

    private void Awake()
    {
        player = GetComponent<PlayerMain>();
    }

    public void StartState(PlayerState _state)
    {
        currentPlayerState = _state;
        currentPlayerState.EnterState();
    }

    public void ChangeState(PlayerState _state, bool forceAlive = false)
    {
        if (currentPlayerState == player.deadState && !forceAlive || !player.IsOwner)
        {
            return;
        }

        currentPlayerState.ExitState();

        if (previousPlayerState != currentPlayerState)//Don't get into a loop of the same state lol
        {
            previousPlayerState = currentPlayerState;
        }

        if (previousPlayerState == null)
        {
            currentPlayerState = player.defaultState;
            currentPlayerState.EnterState();
            return;
        }

        currentPlayerState = _state;
        currentPlayerState.EnterState();
    }
}
