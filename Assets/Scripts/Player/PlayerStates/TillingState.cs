using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TillingState : PlayerState
{
    public TillingState(PlayerMain _player, PlayerStateMachine _playerStateMachine) : base(_player, _playerStateMachine)
    {

    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public override void EnterState()
    {
        base.EnterState();

        player.SpecialInteractEvent.AddListener(Till);
        player.deploySprite.gameObject.SetActive(true);
    }

    public override void ExitState()
    {
        base.ExitState();

        player.SpecialInteractEvent.RemoveListener(Till);
        UnTill();
        player.deploySprite.gameObject.SetActive(false);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        player.defaultState.ReadMovement();
        ShowTillPosition();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        player.defaultState.DoMovement();
    }

    private void Till()
    {
        Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());
        RaycastHit rayHit;
        Physics.Raycast(ray, out rayHit);
        Vector3 newPos = rayHit.point;
        newPos.y = 0;
        newPos = new Vector3(Mathf.Round(newPos.x / 6.25f) * 6.25f, 0, Mathf.Round(newPos.z / 6.25f) * 6.25f);
        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Tilled Row") });
        player.deploySprite.sprite = null;
        player.deploySprite.color = new Color(1, 1, 1, 0);
        player.UseEquippedItemDurability();
    }

    private void ShowTillPosition()
    {
        Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());//this might cause bugs calling in physics update
        RaycastHit rayHit;
        Physics.Raycast(ray, out rayHit);
        Vector3 currentPos = rayHit.point;
        player.deploySprite.color = new Color(.5f, 1f, 1f, .5f);
        player.deploySprite.sprite = WosoArray.Instance.SearchWOSOList("Tilled Row").objSprite;
        player.deploySprite.transform.position = new Vector3(Mathf.Round(currentPos.x / 6.25f) * 6.25f, 0, Mathf.Round(currentPos.z / 6.25f) * 6.25f);
    }

    private void UnTill()
    {
        player.deploySprite.color = new Color(1, 1, 1, 0);
        player.deploySprite.transform.position = Vector3.zero;
    }
}
