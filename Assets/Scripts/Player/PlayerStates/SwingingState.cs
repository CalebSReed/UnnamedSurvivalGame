using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingState : PlayerState
{
    private InteractArgs interactArgs = new InteractArgs();

    public SwingingState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {

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

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public void CheckToSwingAgain()
    {
        if (player.playerInput.PlayerDefault.InteractButton.ReadValue<float>() == 1)
        {
            player.meleeAnimator.Play("Melee");
        }
        else
        {
            playerStateMachine.ChangeState(player.defaultState);
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
            if (_enemy.isTrigger && _enemy.transform.parent.gameObject != player.gameObject)//has to be trigger
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
