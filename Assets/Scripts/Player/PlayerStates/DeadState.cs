using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : PlayerState
{
    public DeadState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {
    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public override void EnterState()
    {
        base.EnterState();

        player.body.GetChild(0).GetComponent<SpriteRenderer>().color = new Vector4(0, 0, 0, 0);
    }

    public override void ExitState()
    {
        base.ExitState();

        player.body.GetChild(0).GetComponent<SpriteRenderer>().color = new Vector4(1, 1, 1, 1);
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
