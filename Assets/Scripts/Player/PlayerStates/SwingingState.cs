using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingState : PlayerState
{
    public InteractArgs interactArgs = new InteractArgs();
    private float oldSpeed;
    public bool isMoving;
    public Vector3 dir;


    public SwingingState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {
        interactArgs.playerSender = player;
    }
    public override void EnterState()
    {
        base.EnterState();

        player.speed = 4;
        GetSwingDirection();
        player.origin.transform.parent.GetComponent<BillBoardBehavior>().isRotating = false;
    }

    public override void ExitState()
    {
        base.ExitState();

        player.speed = player.normalSpeed;
        player.origin.transform.parent.GetComponent<BillBoardBehavior>().isRotating = true;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        player.defaultState.ReadMovement();

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        player.defaultState.DoMovement();

        if (isMoving)
        {
            //player.rb.MovePosition(player.rb.position + dir.normalized * player.speed * Time.fixedDeltaTime);
        }
    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public void GetSwingDirection()
    {
        Vector3 newDir = player.origin.position;
        dir = player.originPivot.position - newDir;
        newDir.y = 0;
    }

    public void CheckToSwingAgain()
    {
        if (player.playerInput.PlayerDefault.InteractButton.ReadValue<float>() == 1 && playerStateMachine.currentPlayerState == this)
        {
            if (player.equippedHandItem != null && player.equippedHandItem.itemSO.doActionType == Action.ActionType.Melee)
            {
                if (player.playerInput.PlayerDefault.SecondSpecialModifier.ReadValue<float>() == 1f)
                {
                    player.swingAnimator.Play("StrongSwing");
                }
                else
                {
                    player.swingAnimator.Play("WeakSwing");
                }
            }
            else if (player.equippedHandItem != null)
            {
                player.swingAnimator.Play("Work");
            }
            else
            {
                player.meleeAnimator.Play("Melee");
            }
        }
        else if (player.doAction == Action.ActionType.Shoot || player.doAction == Action.ActionType.Throw)
        {
            playerStateMachine.ChangeState(player.aimingState);
        }
        else if (playerStateMachine.currentPlayerState == this)
        {
            playerStateMachine.ChangeState(player.defaultState);//Just in case you pick up an equippable item
        }
    }

    public void HitEnemies(int multiplier, DamageType dmgType, float radius)
    {
        Collider[] _hitEnemies = Physics.OverlapSphere(player.originPivot.position, radius);
        interactArgs.playerSender = player;

        foreach (Collider _enemy in _hitEnemies)
        {
            if (player.equippedHandItem != null)
            {
                interactArgs.workEffectiveness = player.equippedHandItem.itemSO.actionEfficiency;
            }
            interactArgs.actionType = player.doAction;

            if (_enemy.isTrigger && _enemy.transform.parent != null && _enemy.transform.parent.gameObject != player.gameObject)//has to be trigger and not self
            {
                interactArgs.hitTrigger = true;
                if (_enemy.GetComponentInParent<Interactable>() != null)//if interactable, interact
                {
                    _enemy.GetComponentInParent<Interactable>().OnInteract(interactArgs);
                }
                else if (_enemy.GetComponentInParent<HealthManager>() != null && _enemy.GetComponentInParent<RealWorldObject>() == null)//or if damageable, damage (unless is object)
                {
                    if (player.equippedHandItem != null)
                    {
                        if (_enemy.transform.root.GetComponent<PlayerMain>() != null && GameManager.Instance.pvpEnabled)//is other player
                        {
                            Debug.Log($"{player.equippedHandItem.itemSO.damage} is dmg from weapon");
                            _enemy.transform.root.GetComponent<PlayerMain>().TakeDamageFromOtherPlayerRPC(player.equippedHandItem.itemSO.damage * multiplier, (int)dmgType, player.playerId);
                            player.UseEquippedItemDurability();
                            continue;
                        }
                        else if (_enemy.transform.root.GetComponent<PlayerMain>() != null && !GameManager.Instance.pvpEnabled)
                        {
                            continue;
                        }
                        _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.equippedHandItem.itemSO.damage * multiplier, player.transform.tag, player.gameObject, dmgType);
                        player.UseEquippedItemDurability();
                    }
                    else
                    {
                        if (_enemy.transform.root.GetComponent<PlayerMain>() != null && GameManager.Instance.pvpEnabled)//is other player
                        {
                            Debug.Log("doing base atk dmg to player");
                            _enemy.transform.root.GetComponent<PlayerMain>().TakeDamageFromOtherPlayerRPC(player.baseAtkDmg * multiplier, (int)dmgType, player.playerId);
                            continue;
                        }
                        else if (_enemy.transform.root.GetComponent<PlayerMain>() != null && !GameManager.Instance.pvpEnabled)
                        {
                            continue;
                        }

                        _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.baseAtkDmg * multiplier, player.transform.tag, player.gameObject, dmgType);
                    }
                }
            }
            else if (!_enemy.isTrigger && _enemy.gameObject != player.gameObject)//if not trigger, we should only hit world objects
            {
                interactArgs.hitTrigger = false;
                if (_enemy.GetComponent<Interactable>() != null)//if interactable, interact
                {
                    _enemy.GetComponent<Interactable>().OnInteract(interactArgs);
                }
            }
        }
    }
}
