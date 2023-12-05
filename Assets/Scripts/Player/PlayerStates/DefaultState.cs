using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DefaultState : PlayerState
{
    private Vector3 movement;

    public DefaultState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {

    }

    public override void EnterState()
    {
        base.EnterState();

        player.InteractEvent.AddListener(SwingHand);
    }

    public override void ExitState()
    {
        base.ExitState();

        player.InteractEvent.RemoveListener(SwingHand);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        ReadMovement();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        DoMovement();
    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public void ReadMovement()
    {
        movement = player.playerInput.PlayerDefault.Movement.ReadValue<Vector2>();

        if (movement.x < 0)
        {
            player.body.localScale = new Vector3(-1, 1, 1);
        }
        else if (movement.x > 0)
        {
            player.body.localScale = new Vector3(1, 1, 1);
        }
    }

    public void DoMovement()
    {
        //movement relative to camera rotation
        Vector3 _forward = player.cam.transform.forward;//get camera's front and right angles
        Vector3 _right = player.cam.transform.right;

        Vector3 _forwardCameraRelative = movement.y * _forward;//multiply by movement (angle * 1 or * 0 or in between if using controller)
        Vector3 _rightCameraRelative = movement.x * _right;

        Vector3 newDirection = _forwardCameraRelative + _rightCameraRelative;//add forward and right values 

        newDirection = new Vector3(newDirection.x, 0, newDirection.z);//set Y to zero because everything should stay on Y:0
        player.rb.MovePosition(player.rb.position + newDirection.normalized * player.speed * Time.fixedDeltaTime);//move the rigid body
    }

    private void SwingHand()//wait these needs to double as attack and work in one function
    {
        if (!player.isHoldingItem)
        {
            player.meleeAnimator.Play("Melee");
            playerStateMachine.ChangeState(player.swingingState);
        }
    }
}
