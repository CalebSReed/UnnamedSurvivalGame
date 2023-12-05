using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingState : PlayerState
{
    public AimingState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {

    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public override void EnterState()
    {
        base.EnterState();

        player.InteractEvent.AddListener(ShootProjectile);
    }

    public override void ExitState()
    {
        base.ExitState();

        player.InteractEvent.RemoveListener(ShootProjectile);
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

    public void ShootProjectile()
    {
        if (player.doAction == Action.ActionType.Throw)
        {
            Throw();
            playerStateMachine.ChangeState(player.defaultState);
        }
        else if (player.doAction == Action.ActionType.Shoot && player.equippedHandItem.ammo > 0)
        {
            Shoot();
        }
    }

    public void Throw()
    {
        Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());
        RaycastHit rayHit;
        Physics.Raycast(ray, out rayHit);

        var _projectile = player.SpawnProjectile();
        _projectile.transform.position = new Vector3(_projectile.transform.position.x, _projectile.transform.position.y + 1, _projectile.transform.position.z);
        var vel = _projectile.transform.right * 100;
        _projectile.GetComponent<Rigidbody>().velocity = vel;
        _projectile.GetComponent<ProjectileManager>().SetProjectile(player.equippedHandItem, player.transform.position, player.gameObject, vel, true);
        player.equippedHandItem = null;
        player.UnequipItem(player.handSlot, false);
        player.doAction = 0;
        player.aimingSprite.sprite = null;
    }

    public void Shoot()
    {
        player.equippedHandItem.ammo--;
        player.UseItemDurability();
        player.UpdateEquippedItem(player.equippedHandItem, player.handSlot);
        var _projectile = player.SpawnProjectile();
        var vel = _projectile.transform.right * 100;
        vel.y = 0;
        _projectile.GetComponent<Rigidbody>().velocity = vel;
        _projectile.GetComponent<ProjectileManager>().SetProjectile(new Item { itemSO = player.equippedHandItem.itemSO.validAmmo, amount = 1 }, player.transform.position, player.gameObject, vel, false);
    }
}
