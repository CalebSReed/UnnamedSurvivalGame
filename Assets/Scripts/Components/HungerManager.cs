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
        if (currentHunger <= 25 && currentHunger > 0)
        {
            yield return new WaitForSeconds(4f);//we need to do delta time and use a float.... this is kinda silly isnt it?
            currentHunger--;
            onAlmostStarving?.Invoke(this, EventArgs.Empty);
            StartCoroutine(DecrementHunger());
        }
        else if (currentHunger > 0)
        {
            yield return new WaitForSeconds(4f);//one second is simply too god damn fast lol. at 4 seconds with 200 max, it takes 800 seconds or 13 min to starve. thats half a day. Tune down food spawns!
            currentHunger--;
            StartCoroutine(DecrementHunger());
        }
        else
        {
            yield return new WaitForSeconds(4f);
            onStarvation?.Invoke(this, EventArgs.Empty);
            StartCoroutine(DecrementHunger());
        }
    }
}
