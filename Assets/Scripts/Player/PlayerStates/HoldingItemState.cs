using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldingItemState : PlayerState
{
    public HoldingItemState(PlayerMain player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public override void EnterState()
    {
        base.EnterState();

        player.FireEvent.AddListener(TryToGiveItem);
        player.CancelEvent.AddListener(UnHoldItem);
    }

    public override void ExitState()
    {
        base.ExitState();

        player.FireEvent.RemoveListener(TryToGiveItem);
        player.CancelEvent.RemoveListener(UnHoldItem);
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

    public void TryToGiveItem()
    {
        Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());
        RaycastHit[] rayHitList = Physics.RaycastAll(ray);
        foreach (RaycastHit rayHit in rayHitList)
        {
            if (rayHit.collider.isTrigger && rayHit.collider.GetComponentInParent<RealWorldObject>() != null && Vector3.Distance(rayHit.transform.position, player.transform.position) <= player.collectRange)
            {
                rayHit.collider.GetComponentInParent<RealWorldObject>().ReceiveItem();
                return;
            }
            else if (rayHit.collider.isTrigger && rayHit.collider.GetComponentInParent<RealMob>() && Vector3.Distance(rayHit.transform.position, player.transform.position) <= player.collectRange)
            {
                rayHit.collider.GetComponentInParent<RealMob>().ReceiveItem();
                return;
            }
        }
    }

    public void UnHoldItem()
    {
        player.StopHoldingItem();
    }
}
