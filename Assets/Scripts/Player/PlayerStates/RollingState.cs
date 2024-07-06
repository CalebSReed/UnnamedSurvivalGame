using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingState : PlayerState
{
    public RollingState(PlayerMain _player, PlayerStateMachine _playerStateMachine) : base(_player, _playerStateMachine)
    {
    }

    Vector3 movement;
    Vector3 direction;

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public override void EnterState()
    {
        base.EnterState();

        movement = player.playerInput.PlayerDefault.Movement.ReadValue<Vector2>();

        Vector3 _forward = player.cam.transform.forward;//get camera's front and right angles
        Vector3 _right = player.cam.transform.right;

        Vector3 _forwardCameraRelative = movement.y * _forward;//multiply by movement (angle * 1 or * 0 or in between if using controller)
        Vector3 _rightCameraRelative = movement.x * _right;

        Vector3 newDirection = _forwardCameraRelative + _rightCameraRelative;//add forward and right values 

        direction = new Vector3(newDirection.x, 0, newDirection.z);//set Y to zero because everything should stay on Y:0        
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

        player.rb.MovePosition(player.rb.position + direction.normalized * player.speed * player.speedMult / 1.5f * Time.fixedDeltaTime);
    }
}
