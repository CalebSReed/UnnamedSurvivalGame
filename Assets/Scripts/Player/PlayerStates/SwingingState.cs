using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingState : PlayerState
{
    private InteractArgs interactArgs = new InteractArgs();
    private float oldSpeed;


    public SwingingState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {

    }
    public override void EnterState()
    {
        base.EnterState();

        oldSpeed = player.speed;
        player.speed = 4;
    }

    public override void ExitState()
    {
        base.ExitState();

        player.speed = oldSpeed;
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
    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public void CheckToSwingAgain()
    {
        if (player.playerInput.PlayerDefault.InteractButton.ReadValue<float>() == 1 && playerStateMachine.currentPlayerState == this)
        {
            player.meleeAnimator.Play("Melee");
        }
        else if (playerStateMachine.currentPlayerState == this)
        {
            playerStateMachine.ChangeState(player.defaultState);//Just in case you pick up an equippable item
        }
    }

    public void HitObjects()
    {
        if (player.equippedHandItem != null)
        {
            interactArgs.workEffectiveness = player.equippedHandItem.itemSO.actionEfficiency;
        }
        interactArgs.actionType = player.doAction;
        Collider[] _hitEnemies = Physics.OverlapSphere(player.originPivot.position, player.atkRange);

        foreach (Collider _enemy in _hitEnemies)
        {
            if (_enemy.isTrigger && _enemy.transform.parent != null && _enemy.transform.parent.gameObject != player.gameObject)//has to be trigger
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
                        _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.equippedHandItem.itemSO.damage, player.transform.tag, player.gameObject);
                        player.UseItemDurability();
                    }
                    else
                    {
                        _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.baseAtkDmg, player.transform.tag, player.gameObject);
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
