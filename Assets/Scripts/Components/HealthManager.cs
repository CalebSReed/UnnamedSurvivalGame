using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
    public bool isInvincible;
    public bool adrenalineInvincible;
    public bool isParrying;

    public float currentArmor { get; private set; }
    public DamageArgs dmgArgs = new DamageArgs();
    public EventHandler<DamageArgs> OnDamageTaken;
    public EventHandler<DamageArgs> OnDeath;
    public EventHandler OnHealed;

    public void SetHealth(int _val)
    {
        maxHealth = _val;
        currentHealth = _val;
    }

    public void SetCurrentHealth(int val)
    {
        currentHealth = val;
    }

    public void TakeDamage(float _dmg, string _dmgSenderTag, GameObject _senderObj, DamageType _dmgType = 0)
    {
        if (isInvincible || adrenalineInvincible)
        {
            return;
        }

        dmgArgs.damageSenderTag = _dmgSenderTag;
        dmgArgs.senderObject = _senderObj;
        dmgArgs.damageAmount = _dmg;
        dmgArgs.dmgType = _dmgType;
        if (GetComponent<PlayerMain>() != null)
        {
            if (GetComponent<PlayerMain>().godMode)
            {
                return;
            }
        }

        if (currentArmor != 0)
        {
            float _newDmg = _dmg - _dmg / 100f * currentArmor; //50 - 50 / 100 (.5) * armor (5) so 2.5... 50 - 2.5 = 47; // 2.5 is 5% of 50. so we are taking out percentage of armor!
            currentHealth -= _newDmg;
        }
        else
        {
            currentHealth -= _dmg;
        }

        OnDamageTaken?.Invoke(this, dmgArgs);

        if (currentHealth <= 0)
        {
            var adrenaline = GetComponent<AdrenalineManager>();
            if (adrenaline != null && adrenaline.adrenalineReady)
            {
                currentHealth = 1;
                GetComponent<TemperatureReceiver>().ResetTemperature();
                StartCoroutine(adrenaline.StartAdrenaline());
                StartCoroutine(BecomeInvincible(2f));
                return;
            }

            var etherShard = GetComponent<EtherShardManager>();
            if (etherShard != null && etherShard.shardReady)
            {
                GetComponent<TemperatureReceiver>().ResetTemperature();
                StartCoroutine(BecomeInvincible(3f));
                GameManager.Instance.world.player.audio.Play("EnterEther", Vector3.zero, null, true, false, true);
                etherShard.BreakShard();

                if (_senderObj.GetComponent<RealMob>() != null)
                {
                    Announcer.SetText("WIN THE DUEL", Color.red);
                    RestoreHealth(99999);
                    EtherShardManager.SendToEther(_senderObj, true);
                    EtherShardManager.SendToEther(gameObject);
                    EtherShardManager.EnterEtherMode();
                }
                RestoreHealth(Mathf.RoundToInt((int)maxHealth / 2));
                return;
            }

            OnDeath?.Invoke(this, dmgArgs);
        }
    }

    public void RestoreHealth(int _newHealth)
    {
        currentHealth += _newHealth;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        OnHealed?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator BecomeInvincible(float duration)
    {
        adrenalineInvincible = true;
        yield return new WaitForSeconds(duration);
        adrenalineInvincible = false;
    }

    public void CheckPlayerArmor()//player armor will be in equip slot behavior ig. I guess I should have written it to be compatible with mobs too. Oof. Actually maybe I can :o
    {
        PlayerMain player = GetComponent<PlayerMain>();
        currentArmor = 0;
        int headArmor = 0;
        int chestArmor = 0;
        int legArmor = 0;
        int feetArmor = 0;
        if (player.headSlot.currentItem != null)
        {
            headArmor = player.headSlot.currentItem.itemSO.armorValue;
        }
        if (player.chestSlot.currentItem != null)
        {
            chestArmor = player.chestSlot.currentItem.itemSO.armorValue;
        }
        if (player.leggingsSlot.currentItem != null)
        {
            legArmor = player.leggingsSlot.currentItem.itemSO.armorValue;
        }
        if (player.feetSlot.currentItem != null)
        {
            feetArmor = player.feetSlot.currentItem.itemSO.armorValue;
        }
        currentArmor = headArmor;
        if (chestArmor > currentArmor)
        {
            currentArmor = chestArmor;
        }
        if (legArmor > currentArmor)
        {
            currentArmor = legArmor;
        }
        if (feetArmor > currentArmor)
        {
            currentArmor = feetArmor;
        }
    }
}
