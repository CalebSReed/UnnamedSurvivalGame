using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class Smelter : MonoBehaviour
{
    private RealWorldObject obj;

    public event EventHandler OnFinishedSmelting;
    public event EventHandler ReplaceWood;

    public int maxTemperature;
    public int minTemperature;
    public int targetTemperature;
    public float currentTemperature;
    public bool temperatureAtTarget = false;

    public int currentFuel;
    public int maxFuel;

    public bool isSmelting = false;
    public bool isSmeltingItem = false;
    public int smeltingProgress;

    public bool isClosed = false;

    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        isClosed = false;
    }

    public void StartSmelting()//primitive kilns can reach up to 1400C with wood, and charcoal can make it 1600C (using bellows)
    {
        isSmelting = true;
        StartCoroutine(SpendFuel());
        StartCoroutine(ReachTargetTemperature());
        obj.light.intensity = 1f;
    }

    public IEnumerator SmeltItem(Item _item, Item.ItemType _reward)
    {
        if (!isSmeltingItem)//if not smelting an item, stop running, but if the furnace turns off and we ARE smelting an item, we will still keep checking... could be bad for performance tho maybe....?
        {
            Debug.LogError("OOPSIES");
        }
        else if (_item.GetSmeltValue() <= currentTemperature && smeltingProgress < _item.GetRequiredSmeltingTime() && isSmeltingItem)//if high enough temp, and isSmelting, add value
        {
            Debug.Log(smeltingProgress + " AMOUNT OF SMELTING PROGRESS");
            yield return new WaitForSeconds(1f);
            smeltingProgress++;
            StartCoroutine(SmeltItem(_item, _reward));
        }
        else if (_item.GetSmeltValue() > currentTemperature && smeltingProgress > 0 && isSmeltingItem)//if temp not enough and progress is not 0, subtract value
        {
            Debug.Log(smeltingProgress + " AMOUNT OF SMELTING PROGRESS");
            yield return new WaitForSeconds(1f);
            StartCoroutine(SmeltItem(_item, _reward));
            smeltingProgress--;
            if (smeltingProgress < 0)
            {
                smeltingProgress = 0;
            }
        }
        else if (_item.GetSmeltValue() > currentTemperature && smeltingProgress <= 0 && isSmeltingItem)//if temp not enough and progress is 0 or lower, do not subtract value but still run coroutine
        {
            Debug.Log(smeltingProgress + " AMOUNT OF SMELTING PROGRESS");
            yield return new WaitForSeconds(1f);
            StartCoroutine(SmeltItem(_item, _reward));
        }
        else if (smeltingProgress >= _item.GetRequiredSmeltingTime())//if we're done smelting, dont run coroutine and change itemtype to reward
        {
            _item.itemType = _reward;
            OnFinishedSmelting?.Invoke(this, EventArgs.Empty);
            ResetSmeltingProgress();
            Debug.Log("done!");
        }
    }

    public void ResetSmeltingProgress()
    {
        Debug.Log("reset smelt");
        smeltingProgress = 0;
    }

    public IEnumerator SpendFuel()
    {
        if (currentFuel > 0)
        {
            yield return new WaitForSeconds(1f);
            currentFuel--;
            Debug.Log("fuel: " + currentFuel + " temp: " + currentTemperature);
            StartCoroutine(SpendFuel());
        }
        else
        {
            Debug.Log("out of fuel");
            ResetSmeltingProgress();
            ReplaceWood?.Invoke(this, EventArgs.Empty);
            obj.light.intensity = 0f;
            isSmelting = false;
            currentTemperature = 0;
            currentFuel = 0;
        }
    }

    public IEnumerator SpendTemperature()
    {
        if (currentTemperature > minTemperature && temperatureAtTarget)
        {
            yield return new WaitForSeconds(.1f);//10 seconds = 100C
            currentTemperature--;
            StartCoroutine(SpendTemperature());
        }
        else if (currentTemperature > 0 && !isSmelting)
        {
            yield return new WaitForSeconds(.1f);//10 seconds = 100C
            currentTemperature--;
            StartCoroutine(SpendTemperature());
        }
    }

    public void AddFuel(int _fuel)
    {
        currentFuel += _fuel;
        if (currentFuel > maxFuel)
        {
            currentFuel = maxFuel;
        }
    }

    public IEnumerator ReachTargetTemperature()
    {
        if (currentTemperature < targetTemperature && !temperatureAtTarget && isSmelting)
        {
            yield return new WaitForSeconds(.01f);//1 sec = 100C
            currentTemperature++;
            if (currentTemperature > maxTemperature)
            {
                Explode();
            }
            StartCoroutine(ReachTargetTemperature());
        }
        else
        {
            Debug.Log("temp at target");
            temperatureAtTarget = true;
            StartCoroutine(SpendTemperature());
        }
    }

    public void Addtemperature(int _temperature)//we dont need to add, we need to just set 
    {
        currentTemperature += _temperature;
        temperatureAtTarget = false;
        if (currentTemperature > maxTemperature)
        {
            Explode();
        }
    }

    public void SetTemperature(int _temperature)
    {
        if (currentTemperature < _temperature && _temperature < maxTemperature)//change so explodes / breaks if set to higher than max instead of refusing to change temp
        {
            targetTemperature = _temperature;
            temperatureAtTarget = false;
            if (isSmelting)
            {
                StartCoroutine(ReachTargetTemperature());
            }
        }
    }

    public void SetMaxTemperature(int _temperature)
    {
        maxTemperature = _temperature;
    }

    public void SetMintemperature(int _temperature)
    {
        minTemperature = _temperature;
    }

    public void SetMaxFuel(int _fuel)
    {
        maxFuel = _fuel;
    }

    public void Explode()//change so we drop like half the materials or something... or maybe not. maybe just drop a bunch of ashes or charcoal or dust or whatever
    {       
        Destroy(gameObject);
    }
}
