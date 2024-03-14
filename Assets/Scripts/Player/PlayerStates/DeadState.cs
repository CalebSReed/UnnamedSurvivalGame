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

        player.body.GetChild(0).GetChild(0).localScale = Vector3.zero;
        player.body.GetChild(0).GetChild(1).localScale = Vector3.zero;
        player.body.GetChild(0).GetChild(2).localScale = Vector3.zero;
        player.playerAnimator.SetBool("isWalking", false);
        player.playerSideAnimator.SetBool("isWalking", false);
        player.playerBackAnimator.SetBool("isWalking", false);
    }

    public override void ExitState()
    {
        base.ExitState();

        player.body.GetChild(0).GetChild(0).localScale = Vector3.one;
        player.body.GetChild(0).GetChild(1).localScale = Vector3.one;
        player.body.GetChild(0).GetChild(2).localScale = Vector3.one;
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
