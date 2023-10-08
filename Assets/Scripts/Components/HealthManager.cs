using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public int currentArmor = 0;
    public DamageArgs dmgArgs = new DamageArgs();
    public EventHandler<DamageArgs> OnDamageTaken;
    public EventHandler OnDeath;

    public void SetHealth(int _val)
    {
        maxHealth = _val;
        currentHealth = _val;
    }

    public void TakeDamage(int _dmg, string _dmgSenderTag, GameObject _senderObj)
    {
        dmgArgs.damageSenderTag = _dmgSenderTag;
        dmgArgs.senderObject = _senderObj;
        if (GetComponent<PlayerMain>() != null)
        {
            if (GetComponent<PlayerMain>().godMode)
            {
                return;
            }
        }

        if (currentArmor != 0)
        {
            int _newDmg = _dmg - Mathf.RoundToInt((float)_dmg / 100 * currentArmor); //50 - 50 / 100 (.5) * armor (5) so 2.5... 50 - 2.5 = 47; // 2.5 is 5% of 50. so we are taking out percentage of armor!
            currentHealth -= _newDmg;
        }
        else
        {
            currentHealth -= _dmg;
        }

        OnDamageTaken?.Invoke(this, dmgArgs);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RestoreHealth(int _newHealth)
    {
        currentHealth += _newHealth;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}
