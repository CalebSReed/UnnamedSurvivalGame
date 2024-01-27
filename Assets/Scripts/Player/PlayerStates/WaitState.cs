using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitState : PlayerState
{
    public WaitState(PlayerMain _player, PlayerStateMachine _playerStateMachine) : base(_player, _playerStateMachine)
    {

    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public override void EnterState()
    {
        base.EnterState();
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
