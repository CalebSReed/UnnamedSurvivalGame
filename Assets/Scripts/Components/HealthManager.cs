using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public EventHandler OnDamageTaken;

    public void SetHealth(int _val)
    {
        maxHealth = _val;
        currentHealth = _val;
    }

    public void TakeDamage(int _dmg)
    {
        currentHealth -= _dmg;
        Debug.Log(currentHealth + " is my health " + _dmg);
        OnDamageTaken?.Invoke(this, EventArgs.Empty);
    }
}
