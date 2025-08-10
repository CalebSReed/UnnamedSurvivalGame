using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingState : PlayerState
{
    public InteractArgs interactArgs = new InteractArgs();
    private float oldSpeed;
    public bool isMoving;
    public Vector3 dir;
    public bool perfectHit;
    public bool buildingPower;
    public float comboPower = 0f;


    public SwingingState(PlayerMain player, PlayerStateMachine _playerStateMachine) : base(player, _playerStateMachine)
    {
        interactArgs.playerSender = player;
    }
    public override void EnterState()
    {
        base.EnterState();

        player.speed = 4;
        player.origin.transform.parent.GetComponent<BillBoardBehavior>().isRotating = false;
        player.InteractEvent.AddListener(TrySwingAgain);
    }

    public override void ExitState()
    {
        base.ExitState();

        player.speed = player.normalSpeed;
        player.origin.transform.parent.GetComponent<BillBoardBehavior>().isRotating = true;
        player.InteractEvent.RemoveListener(TrySwingAgain);
        player.swingAnimator.SetBool("ForceStopCombo", false);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        player.defaultState.ReadMovement();

        if (buildingPower)
        {
            BuildPower();
        }
        else if (!buildingPower && comboPower > 0f)
        {
            LosePower();
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        //player.defaultState.DoMovement(false);

        player.defaultState.ChooseDirectionSprite(false, true);

        if (isMoving && player.doAction == Action.ActionType.Melee || player.doAction == Action.ActionType.Shoot || player.doAction == Action.ActionType.Throw)
        {
            player.rb.MovePosition(player.rb.position + dir.normalized * 3 * Time.fixedDeltaTime);
        }
    }

    public override void AnimationTriggerEvent()
    {
        base.AnimationTriggerEvent();
    }

    private void TrySwingAgain()
    {
        if (comboPower > 0)
        {
            player.swingAnimator.SetBool("Hit", true);
            /*flipped = !flipped;
            if (flipped)
            {
                player.swingAnimator.Play("SwingR");
            }
            else
            {
                player.swingAnimator.Play("SwingL");
            }*/
        }
        else
        {
            Debug.Log("nope!");
        }
    }

    private void BuildPower()
    {
        comboPower += Time.deltaTime * 2f;
    }

    private void LosePower()
    {
        comboPower -= Time.deltaTime * 2f;
    }

    public void GetSwingDirection()
    {
        Vector3 newDir = player.origin.position;
        dir = player.originPivot.position - newDir;
        newDir.y = 0;
    }

    public void CheckToSwingAgain()
    {
        /*if (player.playerInput.PlayerDefault.InteractButton.ReadValue<float>() == 1 && playerStateMachine.currentPlayerState == this)
        {
            if (player.equippedHandItem != null && player.equippedHandItem.itemSO.doActionType == Action.ActionType.Melee)
            {
                if (player.playerInput.PlayerDefault.SecondSpecialModifier.ReadValue<float>() == 1f)
                {
                    //player.swingAnimator.Play("StrongSwing", 0, 0f);
                }
                else
                {
                    //player.swingAnimator.Play("WeakSwing", 0, 0f);
                }
            }
            else if (player.equippedHandItem != null)
            {
                //player.swingAnimator.Play("Work", 0, 0f);
            }
            else
            {
                player.meleeAnimator.Play("Melee", 0, 0f);
            }
        }*/

        if (player.doAction == Action.ActionType.Shoot || player.doAction == Action.ActionType.Throw)
        {
            playerStateMachine.ChangeState(player.aimingState);
        }
        else if (playerStateMachine.currentPlayerState == this)
        {
            playerStateMachine.ChangeState(player.defaultState);//Just in case you pick up an equippable item
        }
    }

    public void HitEnemies(int multiplier, DamageType dmgType, float radius, bool firstHit = false)
    {
        Collider[] _hitEnemies = Physics.OverlapSphere(player.originPivot.position, radius);
        interactArgs.playerSender = player;

        foreach (Collider _enemy in _hitEnemies)
        {
            if (player.equippedHandItem != null)
            {
                if (perfectHit)
                {
                    interactArgs.workEffectiveness = player.equippedHandItem.itemSO.actionEfficiency * 2f;
                }
                else if (firstHit)
                {
                    interactArgs.workEffectiveness = player.equippedHandItem.itemSO.actionEfficiency;
                }
                else
                {
                    interactArgs.workEffectiveness = player.equippedHandItem.itemSO.actionEfficiency * comboPower;
                }
            }
            interactArgs.actionType = player.doAction;

            if (_enemy.isTrigger && _enemy.transform.parent != null && _enemy.transform.parent.gameObject != player.gameObject)//has to be trigger and not self
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
                        if (_enemy.transform.root.GetComponent<PlayerMain>() != null && GameManager.Instance.pvpEnabled)//is other player
                        {
                            Debug.Log($"{player.equippedHandItem.itemSO.damage} is dmg from weapon");

                            if (perfectHit)
                            {
                                _enemy.transform.root.GetComponent<PlayerMain>().TakeDamageFromOtherPlayerRPC(player.equippedHandItem.itemSO.damage * 2f * multiplier, (int)dmgType, player.playerId.Value);
                                int rand = UnityEngine.Random.Range(1, 4);
                                player.audio.Play($"PerfectHit{rand}", player.transform.position, player.gameObject, true);
                            }
                            else if (firstHit)
                            {
                                _enemy.transform.root.GetComponent<PlayerMain>().TakeDamageFromOtherPlayerRPC(player.equippedHandItem.itemSO.damage * multiplier, (int)dmgType, player.playerId.Value);
                            }
                            else
                            {
                                _enemy.transform.root.GetComponent<PlayerMain>().TakeDamageFromOtherPlayerRPC(player.equippedHandItem.itemSO.damage * comboPower * multiplier, (int)dmgType, player.playerId.Value);
                            }

                            player.UseEquippedItemDurability();
                            continue;
                        }
                        else if (_enemy.transform.root.GetComponent<PlayerMain>() != null && !GameManager.Instance.pvpEnabled)
                        {
                            continue;
                        }

                        if (perfectHit)
                        {
                            _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.equippedHandItem.itemSO.damage * 2f * multiplier, player.transform.tag, player.gameObject, dmgType);
                            //Debug.Log($"combo: {comboPower} perfect: {perfectHit} dmg: {player.equippedHandItem.itemSO.damage * 1.5f * multiplier}");
                            int rand = UnityEngine.Random.Range(1, 4);
                            player.audio.Play($"PerfectHit{rand}", player.transform.position, player.gameObject, true);
                        }
                        else if (firstHit)
                        {
                            _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.equippedHandItem.itemSO.damage * multiplier, player.transform.tag, player.gameObject, dmgType);
                            //Debug.Log($"combo: {comboPower} perfect: {perfectHit} dmg: {player.equippedHandItem.itemSO.damage * multiplier}");
                        }
                        else
                        {
                            _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.equippedHandItem.itemSO.damage * comboPower * multiplier, player.transform.tag, player.gameObject, dmgType);
                            //Debug.Log($"combo: {comboPower} perfect: {perfectHit} dmg: {player.equippedHandItem.itemSO.damage * comboPower * multiplier}");
                        }

                        player.UseEquippedItemDurability();
                    }
                    else
                    {
                        if (_enemy.transform.root.GetComponent<PlayerMain>() != null && GameManager.Instance.pvpEnabled)//is other player
                        {
                            //Debug.Log("doing base atk dmg to player");
                            _enemy.transform.root.GetComponent<PlayerMain>().TakeDamageFromOtherPlayerRPC(player.baseAtkDmg * multiplier, (int)dmgType, player.playerId.Value);
                            continue;
                        }
                        else if (_enemy.transform.root.GetComponent<PlayerMain>() != null && !GameManager.Instance.pvpEnabled)
                        {
                            continue;
                        }


                        _enemy.GetComponentInParent<HealthManager>().TakeDamage(player.baseAtkDmg * multiplier, player.transform.tag, player.gameObject, dmgType);
                    }
                }
            }
            else if (!_enemy.isTrigger && _enemy.gameObject != player.gameObject)//if not trigger, we should only hit world objects
            {
                interactArgs.hitTrigger = false;
                if (_enemy.GetComponent<Interactable>() != null)//if interactable, interact
                {
                    _enemy.GetComponent<Interactable>().OnInteract(interactArgs);
                    Debug.Log($"combo: {comboPower} perfect: {perfectHit} work: {interactArgs.workEffectiveness}");
                }
            }
        }
    }
}
