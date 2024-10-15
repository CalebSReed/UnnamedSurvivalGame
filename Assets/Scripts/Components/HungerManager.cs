using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungerManager : MonoBehaviour
{
    public float maxHunger;
    public float currentHunger;
    public float hungerMult;
    public EventHandler onStarvation;
    public EventHandler onAlmostStarving;
    public EventHandler onNotStarving;

    private void Start()
    {
        //StartCoroutine(DecrementHunger());
    }

    public void SetMaxHunger(float _val)
    {
        maxHunger = _val;
        currentHunger = _val;
    }

    public void SetHunger(float val)
    {
        currentHunger = val;
    }

    public void AddHunger(float _val)
    {
        currentHunger += _val;
        if (currentHunger > maxHunger)
        {
            currentHunger = maxHunger;
        }
    }

    public IEnumerator DecrementHunger()//change so we just decrease by delta time bro...
    {
        yield return null;
        if (currentHunger <= 25f && currentHunger > 0f)
        {
            onAlmostStarving?.Invoke(this, EventArgs.Empty);
            currentHunger -= Time.deltaTime / hungerMult;
            StartCoroutine(DecrementHunger());
        }
        else if (currentHunger > 0f)
        {
            onNotStarving?.Invoke(this, EventArgs.Empty);
            currentHunger -= Time.deltaTime / hungerMult;
            StartCoroutine(DecrementHunger());
        }
        else
        {
            currentHunger = 0;
            onStarvation?.Invoke(this, EventArgs.Empty);
            StartCoroutine(DecrementHunger());
        }
    }
}
