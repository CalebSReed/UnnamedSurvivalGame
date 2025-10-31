using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class AnimatorEventReceiver : MonoBehaviour
{
    public event Action<AnimationEvent> eventInvoked;
    [SerializeField] private PlayerMain player;

    private void Start()
    {
        player = transform.root.GetComponent<PlayerMain>();
        /*if (GameManager.Instance.localPlayer != null)
        {
            player = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
        }
        else
        {
            //GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;
        }*/
    }

    public void TestHidePlayerModel(AnimationEvent animEvent)
    {
        player.defaultState.hideBody = true;

        player.swingAnimator.SetBool("ReadyToHit", false);
        player.swingAnimator.SetBool("Hit", false);
    }

    public void TestUnHidePlayerModel(AnimationEvent animEvent)
    {
        player.defaultState.hideBody = false;
    }

    public void AimWithCamera(AnimationEvent animEvent)
    {
        player.bodyHolder.eulerAngles = new Vector3(0, player.mainCam.GetComponent<Camera_Behavior>().rotRef.eulerAngles.y, 0);
        player.origin.rotation = player.bodyHolder.rotation;
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        //player = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
    }

    public void OnAnimationEvent(AnimationEvent animationEvent)
    {
        eventInvoked?.Invoke(animationEvent);
    }

    public void OnHitSwingEvent(AnimationEvent animEvent)
    {
        if (player.IsLocalPlayer)
        {
            player.swingingState.HitEnemies(1, DamageType.Light, player.atkRange);
        }
    }

    public void OnFirstHit(AnimationEvent animEvent)
    {
        if (player.IsLocalPlayer)
        {
            player.swingingState.HitEnemies(1, DamageType.Light, player.atkRange, true);
        }
    }

    public void OnBeginSwingMovement()
    {
        player.swingingState.isMoving = true;
        player.swingingState.GetSwingDirection();
    }

    public void OnEndSwingMovement()
    {
        player.swingingState.isMoving = false;
    }

    public void StaySwingingState(AnimationEvent animEvent)
    {
        if (player.StateMachine.currentPlayerState != player.swingingState)
        {
            player.StateMachine.ChangeState(player.swingingState);
        }
    }

    public void OnHeavySwingEvent(AnimationEvent animEvent)
    {
        if (player.IsLocalPlayer)
        {
            player.swingingState.HitEnemies(3, DamageType.Heavy, player.atkRange * 1.5f);
        }
    }

    public void OnSwingEndEvent(AnimationEvent animEvent)
    {
        //player.swingAnimator.Rebind();
        player.swingingState.CheckToSwingAgain();
        //Debug.Log($"ending swing from {gameObject}");
    }

    public void OnPerfectHitWindowStart(AnimationEvent animEvent)
    {
        player.swingingState.perfectHit = true;
    }

    public void OnPerfectHitWindowEnd(AnimationEvent animEvent)
    {
        player.swingingState.perfectHit = false;
    }

    public void DontFlip(AnimationEvent animEvent)
    {
        player.swingAnimator.SetBool("DontFlip", true);
    }

    public void Flip(AnimationEvent animEvent)
    {
        player.swingAnimator.SetBool("DontFlip", false);
    }

    public void OnBuildPower(AnimationEvent animEvent)
    {
        player.swingingState.comboPower = 0f;
        player.swingingState.buildingPower = true;
        player.swingAnimator.SetBool("ReadyToHit", true);
    }

    public void OnStopBuildingPower(AnimationEvent animEvent)
    {
        player.swingingState.buildingPower = false;
    }

    public void OnDodgeAnimStart()
    {
        player.StateMachine.ChangeState(player.rollingState);
    }

    public void OnDisableControls()
    {
        //Debug.Log($"Disabling player ID: {player}");
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
