using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungerManager : MonoBehaviour
{
    public int maxHunger;
    public int currentHunger;
    public EventHandler onStarvation;
    public EventHandler onAlmostStarving;

    private void Start()
    {
        //StartCoroutine(DecrementHunger());
    }

    public void SetMaxHunger(int _val)
    {
        maxHunger = _val;
        currentHunger = _val;
    }

    public void AddHunger(int _val)
    {
        currentHunger += _val;
        if (currentHunger > maxHunger)
        {
            currentHunger = maxHunger;
        }
    }

    public IEnumerator DecrementHunger()//change so we just decrease by delta time bro...
    {
        if (currentHunger <= 25)
        {
            yield return new WaitForSeconds(2f);
            currentHunger--;
            onAlmostStarving?.Invoke(this, EventArgs.Empty);
            StartCoroutine(DecrementHunger());
        }
        else if (currentHunger > 0)
        {
            yield return new WaitForSeconds(2f);//one second is simply too god damn fast lol
            currentHunger--;
            StartCoroutine(DecrementHunger());
        }
        else
        {
            yield return new WaitForSeconds(2f);
            onStarvation?.Invoke(this, EventArgs.Empty);
            StartCoroutine(DecrementHunger());
        }
    }
}
