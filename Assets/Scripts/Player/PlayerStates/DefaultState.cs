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

        player.SpecialInteractEvent.AddListener(SpecialUse);

        /*if (player.doAction == Action.ActionType.Shoot || player.doAction == Action.ActionType.Throw)
        {
            playerStateMachine.ChangeState(player.aimingState);
        }*/
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
        movement = player.playerInput.PlayerDefault.Movement.ReadValue<Vector2>();//Rotate player where they are moving

        if (movement.x < 0)
        {
            player.body.localScale = new Vector3(-1, 1, 1);
        }
        else if (movement.x > 0)
        {
            player.body.localScale = new Vector3(1, 1, 1);
        }

        if (movement.x != 0 || movement.y != 0)
        {
            player.playerAnimator.SetBool("isWalking", true);
        }
        else
        {
            player.playerAnimator.SetBool("isWalking", false);
        }
    }

    public void DoMovement()//only move forward player's rotation. 
    {
        //movement relative to camera rotation
        Vector3 _forward = player.cam.transform.forward;//get camera's front and right angles
        Vector3 _right = player.cam.transform.right;

        Vector3 _forwardCameraRelative = movement.y * _forward;//multiply by movement (angle * 1 or * 0 or in between if using controller)
        Vector3 _rightCameraRelative = movement.x * _right;

        Vector3 newDirection = _forwardCameraRelative + _rightCameraRelative;//add forward and right values 

        newDirection = new Vector3(newDirection.x, 0, newDirection.z);//set Y to zero because everything should stay on Y:0
        player.rb.MovePosition(player.rb.position + newDirection.normalized * player.speed * player.speedMult * Time.fixedDeltaTime);//move the rigid body
    }

    private void SwingHand()//wait these needs to double as attack and work in one function
    {
        if (!player.isHoldingItem)
        {
            player.meleeAnimator.Play("Melee");
            playerStateMachine.ChangeState(player.swingingState);
        }
    }

    private void SpecialUse()
    {
        if (player.hasTongs && player.isHandItemEquipped && player.equippedHandItem.containedItem != null)
        {
            var _item = RealItem.SpawnRealItem(player.transform.position, player.equippedHandItem.containedItem);
            CalebUtils.RandomDirForceNoYAxis3D(_item.GetComponent<Rigidbody>(), 5f);
            player.equippedHandItem.containedItem = null;
            player.RemoveContainedItem();
        }
    }
}
