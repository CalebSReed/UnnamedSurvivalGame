using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployState : PlayerState
{
    public Item deployItem;

    public DeployState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {

    }

    public override void EnterState()
    {
        base.EnterState();

        player.deploySprite.color = new Color(.5f, 1f, 1f, .5f);
        //pointerImage.transform.localScale = new Vector3(1f, 1f, 1f);
        player.deploySprite.sprite = deployItem.itemSO.itemSprite;//change to object sprite because items will have diff sprites blah blah blah
        player.deploySprite.gameObject.SetActive(true);
        player.deployOutlineSprite.gameObject.SetActive(true);

        player.CancelEvent.AddListener(ExitDeploy);
        player.InteractEvent.AddListener(DeployObject);
    }

    public override void ExitState()
    {
        base.ExitState();

        if (deployItem.amount > 0)
        {
            player.inventory.AddItem(deployItem, player.transform.position);
        }

        deployItem = null;
        player.deploySprite.sprite = null;
        player.CancelEvent.RemoveListener(ExitDeploy);
        player.InteractEvent.RemoveListener(DeployObject);
        player.deploySprite.gameObject.SetActive(false);
        player.deployOutlineSprite.gameObject.SetActive(false);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        player.defaultState.ReadMovement();
        SetDeploySpritePosition();
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

    private void SetDeploySpritePosition()
    {
        Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());//this might cause bugs calling in physics update
        RaycastHit[] rayHits = Physics.RaycastAll(ray, Mathf.Infinity, GameManager.Instance.tileMask);

        Debug.Log(rayHits.Length);  

        foreach(var rayHit in rayHits)
        {
            if (rayHit.collider.CompareTag("Tile"))
            {
                RaycastHit correctRayHit = rayHit;

                Vector3 currentPos = correctRayHit.point;
                currentPos.y = 0;
                player.deploySprite.transform.position = currentPos;

                if (player.playerInput.PlayerDefault.DeployModifier.ReadValue<float>() == 0 || deployItem.itemSO.isWall)//is wall or not holdin ctrl
                {
                    player.deploySprite.transform.localPosition = Vector3.forward;
                    player.deploySprite.transform.position = new Vector3(Mathf.Round(currentPos.x / 6.25f) * 6.25f, 0, Mathf.Round(currentPos.z / 6.25f) * 6.25f);//these dont actually place where they SHOULD!!!
                }
                else if (player.playerInput.PlayerDefault.DeployModifier.ReadValue<float>() == 1)//isnt wall but holdin ctrl  else might actually work im too lazy to test
                {
                    player.deploySprite.transform.localPosition = currentPos;
                }
                return;
            }
        }
        Debug.LogError("No valid ray hits!");
    }

    private void ExitDeploy()
    {
        playerStateMachine.ChangeState(player.defaultState);
    }

    private void DeployObject()
    {
        Vector3 newPos = player.deploySprite.transform.position;
        newPos.y = 0;
        if (deployItem.itemSO.isWall || player.playerInput.PlayerDefault.DeployModifier.ReadValue<float>() == 0)
        {
            newPos = new Vector3(Mathf.Round(newPos.x / 6.25f) * 6.25f, 0, Mathf.Round(newPos.z / 6.25f) * 6.25f);
        }
        RealWorldObject obj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = deployItem.itemSO.deployObject });

        if (obj.woso.isDoor)
        {
            obj.transform.rotation = player.cam.transform.rotation;
        }

        deployItem.amount--;

        if (obj.woso.isCWall || obj.woso.isDoor)//only placing wall should destroy
        {
            CheckIfTouchingWall(obj.transform.position, obj.gameObject);
        }

        if (deployItem.amount <= 0)
        {
            if (player.isHandItemEquipped && player.equippedHandItem.itemSO.doActionType == Action.ActionType.Shoot || player.isHandItemEquipped && player.equippedHandItem.itemSO.doActionType == Action.ActionType.Throw)//OVERRIDE!!
            {
                playerStateMachine.ChangeState(player.aimingState);
            }
            else if (player.isHandItemEquipped && player.equippedHandItem.itemSO.doActionType == Action.ActionType.Till)
            {
                playerStateMachine.ChangeState(player.tillingState);
            }
            else
            {
                playerStateMachine.ChangeState(player.defaultState);
            }
        }
    }

    private void CheckIfTouchingWall(Vector3 pos, GameObject self)
    {
        var objects = Physics.BoxCastAll(pos, Vector3.one, Vector3.up);
        foreach(var obj in objects)
        {
            if (obj.collider.GetComponent<RealWorldObject>() != null && obj.collider.GetComponent<RealWorldObject>().woso.isCWall && obj.collider.gameObject != self || obj.collider.GetComponent<RealWorldObject>() != null && obj.collider.GetComponent<RealWorldObject>().woso.isDoor && obj.collider.gameObject != self)
            {
                obj.collider.GetComponent<RealWorldObject>().Break();
            }
        }
    }
}
