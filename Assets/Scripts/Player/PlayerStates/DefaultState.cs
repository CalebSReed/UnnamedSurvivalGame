using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DefaultState : PlayerState
{
    private Vector3 movement;
    public bool hideBody;

    public DefaultState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {

    }

    public override void EnterState()
    {
        if (!player.IsOwner && GameManager.Instance.multiplayerEnabled)
        {
            return;
        }

        base.EnterState();

        player.InteractEvent.AddListener(SwingHand);

        player.SpecialInteractEvent.AddListener(SpecialUse);

        /*if (player.doAction == Action.ActionType.Shoot || player.doAction == Action.ActionType.Throw)
        {
            playerStateMachine.ChangeState(player.aimingState);
        }*/

        if (player.doAction == Action.ActionType.Till)
        {
            playerStateMachine.ChangeState(player.tillingState);
        }
    }

    public override void ExitState()
    {
        if (!player.IsOwner && GameManager.Instance.multiplayerEnabled)
        {
            return;
        }

        base.ExitState();

        player.InteractEvent.RemoveListener(SwingHand);

        player.SpecialInteractEvent.RemoveListener(SpecialUse);
    }

    public override void FrameUpdate()
    {
        if (!player.IsOwner && GameManager.Instance.multiplayerEnabled)
        {
            return;
        }

        base.FrameUpdate();

        ReadMovement();
        ChooseDirectionSprite();
    }

    public override void PhysicsUpdate()
    {
        if (!player.IsOwner && GameManager.Instance.multiplayerEnabled)
        {
            return;
        }

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

        if ( movement.x != 0 || movement.y != 0)
        {
            player.playerAnimator.SetBool("isWalking", true);
            player.playerSideAnimator.SetBool("isWalking", true);
            player.playerBackAnimator.SetBool("isWalking", true);
        }
        else
        {
            player.playerAnimator.SetBool("isWalking", false);
            player.playerSideAnimator.SetBool("isWalking", false);
            player.playerBackAnimator.SetBool("isWalking", false);
        }
    }

    public void DoMovement(bool rotateMovement = true)//only move forward player's rotation. 
    {
        //movement relative to camera rotation
        Vector3 _forward = player.cam.transform.forward;//get camera's front and right angles
        Vector3 _right = player.cam.transform.right;

        Vector3 _forwardCameraRelative = movement.y * _forward;//multiply by movement (angle * 1 or * 0 or in between if using controller)
        Vector3 _rightCameraRelative = movement.x * _right;
        
        //Debug.Log($"forward: {_forwardCameraRelative}, right: {_rightCameraRelative}");

        Vector3 newDirection = _forwardCameraRelative + _rightCameraRelative;//add forward and right values 

        if (player.IsLocalPlayer && rotateMovement)
        {
            LookTowardsMovement(newDirection.x, newDirection.z);
        }

        newDirection.Normalize();
        newDirection = player.rb.position + newDirection * player.speed * player.speedMult * Time.fixedDeltaTime;
        newDirection.y = FindGroundLevel(newDirection);
        player.rb.MovePosition(newDirection);//move the rigid body
    }

    private void LookTowardsMovement(float x, float y)
    {
        if (x != 0 || y != 0)
        {
            Vector3 lookAtRotation = new Vector3(x, 0, y);
            Quaternion newRot = Quaternion.LookRotation(lookAtRotation, Vector3.up);
            player.bodyHolder.rotation = newRot;
        }
    }

    public void ChooseDirectionSprite(bool flip = true, bool hideBody = false)
    {
        float angle = Vector3.SignedAngle(player.bodyHolder.forward, SceneReferences.Instance.mainCamBehavior.rotRef.forward, Vector3.up);

        if (Mathf.Abs(angle) == 180 || Mathf.Abs(angle) == 0)//If we are running perfectly straight with the camera, DONT FLIP!!!!!!
        {
            player.body.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            if (angle > 0 && flip)//idk how i got it backwards but ok
            {
                player.body.localScale = new Vector3(-1, 1, 1);
            }
            else if (angle < 0)
            {
                player.body.localScale = new Vector3(1, 1, 1);
            }
        }


        if (hideBody)
        {
            player.playerBackAnimator.transform.localScale = Vector3.zero;
            player.playerSideAnimator.transform.localScale = Vector3.zero;
            player.playerAnimator.transform.localScale = Vector3.zero;
        }
        else// 0-44 is back, 45-134 is side 135-180 is front
        {
            if (Mathf.Abs(angle) < 45)
            {
                player.playerBackAnimator.transform.localScale = Vector3.one;
                player.playerSideAnimator.transform.localScale = Vector3.zero;
                player.playerAnimator.transform.localScale = Vector3.zero;
            }
            else if (Mathf.Abs(angle) >= 45 && Mathf.Abs(angle) < 135)//If we take off the = sign we'll actually retain the original direction before turning diagonal!! pretty cool huh? Change to that if u want to ever.
            {
                player.playerSideAnimator.transform.localScale = Vector3.one;
                player.playerAnimator.transform.localScale = Vector3.zero;
                player.playerBackAnimator.transform.localScale = Vector3.zero;
            }
            else if (Mathf.Abs(angle) >= 135)
            {
                player.playerAnimator.transform.localScale = Vector3.one;
                player.playerBackAnimator.transform.localScale = Vector3.zero;
                player.playerSideAnimator.transform.localScale = Vector3.zero;
            }
        }

    }

    private void SwingHand()//wait these needs to double as attack and work in one function
    {
        if (!player.isHoldingItem)
        {
            if (player.equippedHandItem != null && player.equippedHandItem.itemSO.doActionType == Action.ActionType.Melee)
            {
                if (player.playerInput.PlayerDefault.SecondSpecialModifier.ReadValue<float>() == 1f)
                {
                    //player.swingAnimator.Play("StrongSwing", 0, 0f);
                }
                else
                {
                    player.swingAnimator.Play("SwingLFull", 0, 0f);
                }
                //player.playerAnimator.Play("Swing", 1);   
            }
            else if (player.equippedHandItem != null)
            {
                //player.swingAnimator.Play("Work", 0, 0f);
                player.swingAnimator.Play("SwingLFull", 0, 0f);
            }
            else
            {
                player.meleeAnimator.Play("Melee", 0, 0f);
            }
            playerStateMachine.ChangeState(player.swingingState);
        }
    }

    private void SpecialUse()
    {
        if (player.playerInput.PlayerDefault.SecondSpecialModifier.ReadValue<float>() == 1f && player.isHandItemEquipped)
        {
            player.swingAnimator.Play("Parry", 0, 0f);
            return;
        }
        if (player.hasTongs && player.isHandItemEquipped && player.equippedHandItem.heldItem != null)
        {
            if (GameManager.Instance.isServer)
            {
                var _item = RealItem.SpawnRealItem(player.transform.position, player.equippedHandItem.heldItem);
                CalebUtils.RandomDirForceNoYAxis3D(_item.GetComponent<Rigidbody>(), 5f);
            }
            else
            {
                int[] containedTypes = null;
                int[] containedAmounts = null;
                if (player.equippedHandItem.heldItem.itemSO.canStoreItems)
                {
                    containedTypes = new int[player.equippedHandItem.heldItem.itemSO.maxStorageSpace];
                    containedAmounts = new int[player.equippedHandItem.heldItem.itemSO.maxStorageSpace];
                    for (int i = 0; i < containedTypes.Length; i++)
                    {
                        if (player.equippedHandItem.heldItem.containedItems[i] != null)
                        {
                            containedTypes[i] = player.equippedHandItem.heldItem.containedItems[i].itemSO.itemID;
                            containedAmounts[i] = 1;
                        }
                        else
                        {
                            containedTypes[i] = -1;
                        }
                    }
                }

                ClientHelper.Instance.AskToSpawnItemSpecificRPC(player.transform.position, true, false, player.equippedHandItem.heldItem.itemSO.itemType, 1, 0, 0, 0, player.equippedHandItem.heldItem.isHot, player.equippedHandItem.heldItem.remainingTime, containedTypes, containedAmounts, null, true);
            }

            player.equippedHandItem.heldItem = null;
            player.RemoveContainedItem();
        }
    }

    private float FindGroundLevel(Vector3 newPos)//we NEED the NEXT frame position because move calls only happen the NEXT frame!! 
    {
        float groundLevel = 0f;
        var y0pos = player.transform.position;
        y0pos.y = 0;
        var y250pos = player.transform.position;
        y250pos.y = 250;
        var y500pos = player.transform.position;//just in case players pvp and trigger ether twice
        y500pos.y = 500;
        if (Vector3.Distance(player.transform.position, y0pos) < Vector3.Distance(player.transform.position, y250pos))//if 0 is closer to 250
        {
            groundLevel = 0f;
        }
        else if (Vector3.Distance(player.transform.position, y250pos) < Vector3.Distance(player.transform.position, y500pos))//if false, then if 250 is closer than 500
        {
            groundLevel = 250f;
        }
        else
        {
            groundLevel = 500f;
        }
        player.transform.position = new Vector3(newPos.x, groundLevel, newPos.z);
        return groundLevel;
    }
}
