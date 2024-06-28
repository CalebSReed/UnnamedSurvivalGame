using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingState : PlayerState
{
    public WaitingState(PlayerMain _player, PlayerStateMachine _playerStateMachine) : base(_player, _playerStateMachine)
    {
    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public override void EnterState()
    {
        base.EnterState();
        player.swingingState.isMoving = false;
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
