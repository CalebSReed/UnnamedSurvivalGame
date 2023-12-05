using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerMain player;
    public PlayerState currentPlayerState { get; private set; }

    private void Awake()
    {
        player = GetComponent<PlayerMain>();
    }

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
