using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DefaultState : PlayerState
{
    public DefaultState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {

    }

    private Vector3 movement;
    private InteractArgs interactArgs = new InteractArgs();

    public override void EnterState()
    {
        base.EnterState();
        player.InteractEvent.AddListener(SwingHand);
    }

    public override void ExitState()
    {
        base.ExitState();
        player.InteractEvent.RemoveListener(SwingHand);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        movement = player.playerInput.PlayerDefault.Movement.ReadValue<Vector2>();

        if (movement.x < 0)
        {
            player.body.localScale = new Vector3(-1, 1, 1);
            player.isMirrored = true;
        }
        else if (movement.x > 0)
        {
            player.body.localScale = new Vector3(1, 1, 1);
            player.isMirrored = false;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        CheckIfMoving();

        //HoverText.transform.position = Input.mousePosition;
        //HoverText.transform.position = new Vector3(HoverText.transform.position.x + 15, HoverText.transform.position.y - 15, HoverText.transform.position.z);

        Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.Movement.ReadValue<Vector2>());//this might cause bugs calling in physics update
        RaycastHit rayHit;
        Physics.Raycast(ray, out rayHit);

        Vector3 newPos = rayHit.point;
        newPos.y = 0;
        player.pointer.transform.position = newPos;

        /*if (main.deployMode && !Input.GetKey(KeyCode.LeftControl) && !main.itemToDeploy.itemSO.isWall)// if holding left control will NOT snap to a grid
        {
            Vector3 currentPos = rayHit.point;
            currentPos.y = 0;
            main.deploySprite.transform.localPosition = Vector3.forward;
            main.deploySprite.transform.position = new Vector3(Mathf.Round(currentPos.x / 6.25f) * 6.25f, 0, Mathf.Round(currentPos.z / 6.25f) * 6.25f);//these dont actually place where they SHOULD!!!
        }
        else if (main.deployMode && main.itemToDeploy.itemSO.isWall)//if is a wall, always snap >:(
        {
            Vector3 currentPos = rayHit.point;
            currentPos.y = 0;
            main.deploySprite.transform.localPosition = Vector3.forward;
            main.deploySprite.transform.position = new Vector3(Mathf.Round(currentPos.x / 6.25f) * 6.25f, 0, Mathf.Round(currentPos.z / 6.25f) * 6.25f);//fix this shid bruh

            if (main.itemToDeploy.itemSO.deployObject.isHWall)
            {
                main.deploySprite.transform.position = new Vector3(main.deploySprite.transform.position.x, 0, main.deploySprite.transform.position.z + 2);
            }

        }
        else if (main.deployMode && !main.itemToDeploy.itemSO.isWall && Input.GetKey(KeyCode.LeftControl))
        {
            main.deploySprite.transform.localPosition = Vector3.forward;
            main.pointer.transform.position = newPos;
        }

        if (main.doAction == Action.ActionType.Till && !main.deployMode)
        {
            main.deploySprite.color = new Color(.5f, 1f, 1f, .5f);
            main.deploySprite.sprite = WosoArray.Instance.SearchWOSOList("Tilled Row").objSprite;
            Vector3 currentPos = rayHit.point;
            main.deploySprite.transform.position = new Vector3(Mathf.Round(currentPos.x / 6.25f) * 6.25f, 0, Mathf.Round(currentPos.z / 6.25f) * 6.25f);
        }*/

        //movement relative to camera rotation
        Vector3 _forward = player.cam.transform.forward;//get camera's front and right angles
        Vector3 _right = player.cam.transform.right;

        Vector3 _forwardCameraRelative = movement.y * _forward;//multiply by movement (angle * 1 or * 0 or in between if using controller)
        Vector3 _rightCameraRelative = movement.x * _right;

        Vector3 newDirection = _forwardCameraRelative + _rightCameraRelative;//add forward and right values 

        newDirection = new Vector3(newDirection.x, 0, newDirection.z);//set Y to zero because everything should stay on Y:0

        player.rb.MovePosition(player.rb.position + newDirection.normalized * player.speed * Time.fixedDeltaTime);//move the rigid body


        if (player.playerInput.PlayerDefault.Movement.ReadValue<Vector2>().x != 0 || player.playerInput.PlayerDefault.Movement.ReadValue<Vector2>().y != 0)
        {
            //onMoved?.Invoke(this, EventArgs.Empty);
            /*target = Vector3.zero;//this the dumbest shit ive ever seen lol
            main.goingToItem = false;
            main.doingAction = false;
            main.animateWorking = false;
            main.playerAnimator.SetBool("isDeploying", false);
            main.isDeploying = false;
            main.goingToItem = false;
            main.goingToCollect = false;
            main.givingItem = false;
            main.goingToLight = false;
            main.attachingItem = false;
            main.tillMode = false;*/
        }
    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }
    private void CheckIfMoving()
    {
        if (player.transform.hasChanged)
        {
            player.playerAnimator.SetBool("isWalking", true);
            player.transform.hasChanged = false;
        }
        else
        {
            player.playerAnimator.SetBool("isWalking", false);
        }
    }

    private void SwingHand()//wait these needs to double as attack and work in one function
    {
        Debug.Log("Swingin!");
        if (player.equippedHandItem != null)
        {
            interactArgs.workEffectiveness = player.equippedHandItem.itemSO.actionEfficiency;

        }
        Collider[] _hitEnemies = Physics.OverlapSphere(player.originPivot.position, player.atkRange);
        
        foreach (Collider _enemy in _hitEnemies)
        {
            if (_enemy.isTrigger && _enemy.transform.parent.gameObject != player.gameObject)//has to be trigger
            {
                if (_enemy.GetComponentInParent<Interactable>() != null)//if interactable, interact
                {
                    _enemy.GetComponentInParent<Interactable>().OnInteract(interactArgs);
                    if (player.equippedHandItem != null)
                    {
                        player.UseItemDurability();//check if interact event should use up durability of a tool
                    }
                }
                else if (_enemy.GetComponentInParent<HealthManager>() != null && _enemy.GetComponentInParent<RealWorldObject>() == null)//or if damageable, damage (unless is object)
                {
                    if (player.equippedHandItem != null)
                    {
                        _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.equippedHandItem.itemSO.damage, player.transform.tag, player.gameObject);
                    }
                    else
                    {
                        _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.baseAtkDmg, player.transform.tag, player.gameObject);
                    }
                }
            }
        }
    }



    /*   template
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
    */
}
