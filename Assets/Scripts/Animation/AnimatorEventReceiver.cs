using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class AnimatorEventReceiver : MonoBehaviour
{
    public event Action<AnimationEvent> eventInvoked;
    [SerializeField] private PlayerMain player;

    public void OnAnimationEvent(AnimationEvent animationEvent)
    {
        eventInvoked?.Invoke(animationEvent);
    }

    public void OnHitSwingEvent(AnimationEvent animEvent)
    {
        player.swingingState.HitEnemies(1, DamageType.Light, player.atkRange);
    }

    public void OnBeginSwingMovement()
    {
        player.swingingState.isMoving = true;
    }

    public void OnEndSwingMovement()
    {
        player.swingingState.isMoving = false;
    }

    public void OnHeavySwingEvent(AnimationEvent animEvent)
    {
        player.swingingState.HitEnemies(3, DamageType.Heavy, player.atkRange * 1.5f);
    }

    public void OnSwingEndEvent(AnimationEvent animEvent)
    {
        player.swingAnimator.Rebind();
        player.swingingState.CheckToSwingAgain();
    }

    public void OnDodgeAnimStart()
    {
        player.StateMachine.ChangeState(player.rollingState);
    }

    public void OnDisableControls()
    {
        player.StateMachine.ChangeState(player.waitingState);
    }

    public void OnEnableControls()
    {
        player.StateMachine.ChangeState(player.defaultState);

        if (player.doAction == Action.ActionType.Shoot || player.doAction == Action.ActionType.Throw)
        {
            player.StateMachine.ChangeState(player.aimingState);
        }
        if (player.isHoldingItem)
        {
            player.StateMachine.ChangeState(player.holdingItemState);
        }
    }

    public void OnDodgeBegin(AnimationEvent animEvent)
    {
        player.hpManager.isInvincible = true;
    }

    public void OnDodgeEnd(AnimationEvent animEvent)
    {
        player.hpManager.isInvincible = false;
    }

    public void OnParryBegin()
    {
        player.hpManager.isParrying = true;
    }

    public void OnParryEnd()
    {
        player.hpManager.isParrying = false;
    }

    public void PlaySound(AnimationEvent animEvent)
    {
        player.audio.Play($"{animEvent.stringParameter}", transform.position);
    }

    public void PlayRandomSound(AnimationEvent animEvent)
    {
        int rand = UnityEngine.Random.Range(1, animEvent.intParameter+1);
        player.audio.Play($"{animEvent.stringParameter}{rand}", transform.position);
    }
}
