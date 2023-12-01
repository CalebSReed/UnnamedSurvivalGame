using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    //Player states:
    /*
     * Idle?
     * Deploy
     * Attacking
     * Busy??? Or eating, using, etc?
     * Riding (for future use riding vehicles and animals)
     * Aiming..?
     * Interacting? / Working? (swinging your tool at objects)
     */
    protected PlayerMain player;
    protected PlayerStateMachine playerStateMachine;

    public PlayerState(PlayerMain _player, PlayerStateMachine _playerStateMachine)
    {
        player = _player;
        playerStateMachine = _playerStateMachine;
    }

    public virtual void EnterState()
    {

    }

    public virtual void ExitState()
    {

    }

    public virtual void FrameUpdate()
    {
        
    }

    public virtual void PhysicsUpdate()
    {

    }

    public virtual void AnimationTriggerEvent()
    {

    }

}
