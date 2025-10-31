using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AimingState : PlayerState
{
    public AimingState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {

    }

    public float chargingPower { get; private set; } = 0;
    public float maxCharge { get; private set; } = 1;

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    public override void EnterState()
    {
        base.EnterState();

        chargingPower = 0;
        player.FireEvent.AddListener(ShootProjectile);
    }

    public override void ExitState()
    {
        base.ExitState();

        player.speed = player.normalSpeed;

        Vector3 pos = player.meleeHand.transform.localPosition;
        pos.z = 2.5f;
        player.meleeHand.transform.localPosition = pos;

        player.FireEvent.RemoveListener(ShootProjectile);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        player.defaultState.ReadMovement();
        player.defaultState.ChooseDirectionSprite();
        CheckIfCharging();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        player.defaultState.DoMovement();
    }

    private void CheckIfCharging()
    {
        if (player.playerInput.PlayerDefault.SpecialInteract.ReadValue<float>() == 1f && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())//if holding RMB charge up
        {
            chargingPower += Time.deltaTime * 4;
            player.speed = player.normalSpeed / 2;
        }
        else if (chargingPower > 0)
        {
            chargingPower -= Time.deltaTime * 8;
            player.speed = player.normalSpeed;
        }
        if (chargingPower > maxCharge)
        {
            chargingPower = maxCharge;
        }
        Vector3 pos = player.meleeHand.transform.localPosition;
        pos.z = 2.5f - chargingPower * 2;
        player.meleeHand.transform.localPosition = pos;
    }

    public void ShootProjectile()
    {
        if (chargingPower < maxCharge && player.playerInput.PlayerDefault.SpecialInteract.ReadValue<float>() < 1f)
        {
            player.meleeAnimator.Play("Melee");
            playerStateMachine.ChangeState(player.swingingState);
            return;
        }
        else if (chargingPower < maxCharge)
        {
            return;
        }
        chargingPower = 0;
        if (player.doAction == Action.ActionType.Throw)
        {
            Throw();
            if (player.equippedHandItem == null)
            {
                playerStateMachine.ChangeState(player.defaultState);
            }
        }
        else if (player.doAction == Action.ActionType.Shoot && player.equippedHandItem.ammo > 0)
        {
            Shoot();
        }
    }

    public void Throw()
    {
        if (GameManager.Instance.isServer)
        {
            var _projectile = player.SpawnProjectile();
            _projectile.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z);
            var vel = _projectile.transform.right * 100;
            vel.y = player.transform.position.y;//need
            _projectile.GetComponent<Rigidbody>().velocity = vel;
            _projectile.GetComponent<ProjectileManager>().SetProjectile(player.equippedHandItem, player.transform.position, player.gameObject, vel, true, false, .5f);
            _projectile.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            ClientHelper.Instance.SpawnProjectileRPC(player.transform.position, player.meleeHand.transform.rotation, player.equippedHandItem.itemSO.itemType, player.equippedHandItem.uses, player.GetComponent<NetworkObject>().NetworkObjectId);
        }

        //player.equippedHandItem.amount--;
        var nextItem = player.inventory.FindFirstNonEquippedItem(player.equippedHandItem.itemSO.itemType);
        var nextItemIndex = player.inventory.FindFirstNonEquippedItemIndex(player.equippedHandItem.itemSO.itemType);

        player.equippedHandItem.amount--;
        player.UnequipItem(player.equippedHandItem.equipType, false);
        player.equippedHandItem = null;
        player.doAction = 0;
        player.aimingSprite.sprite = null;
        player.inventory.RefreshEmptySlots();

        if (nextItem != null)
        {
            player.EquipItem(nextItem);
        }
    }

    public void Shoot()
    {
        player.equippedHandItem.ammo--;
        player.UseEquippedItemDurability();
        if (player.equippedHandItem.uses > 0)
        {
            player.UpdateEquippedItem(player.equippedHandItem);
        }

        if (GameManager.Instance.isServer)
        {
            var _projectile = player.SpawnProjectile();
            _projectile.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z);
            var vel = _projectile.transform.right * 100;
            vel.y = player.transform.position.y;
            _projectile.GetComponent<Rigidbody>().velocity = vel;
            _projectile.GetComponent<ProjectileManager>().SetProjectile(new Item { itemSO = player.equippedHandItem.itemSO.validAmmo, amount = 1 }, player.transform.position, player.gameObject, vel, false, false, .5f);
            _projectile.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            ClientHelper.Instance.SpawnProjectileRPC(player.transform.position, player.meleeHand.transform.rotation, player.equippedHandItem.itemSO.validAmmo.itemType, player.equippedHandItem.uses, player.GetComponent<NetworkObject>().NetworkObjectId);
        }

    }
}
