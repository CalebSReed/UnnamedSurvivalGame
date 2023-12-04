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
    public int baseTemperature { get; set; }
    private int bonusTemp;
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
        //obj.light.intensity = 1f;
    }

    public IEnumerator SmeltItem(Item _item)
    {
        if (!isSmeltingItem)//if not smelting an item, stop running, but if the furnace turns off and we ARE smelting an item, we will still keep checking... could be bad for performance tho maybe....?
        {
            Debug.Log("OOPSIES");
        }
        else if (_item.itemSO.smeltValue <= currentTemperature && smeltingProgress < _item.itemSO.requiredSmeltingTime && isSmeltingItem)//if high enough temp, and isSmelting, add value
        {
            Debug.Log(smeltingProgress + " AMOUNT OF SMELTING PROGRESS");
            yield return new WaitForSeconds(1f);
            smeltingProgress++;
            StartCoroutine(SmeltItem(_item));
        }
        else if (_item.itemSO.smeltValue > currentTemperature && smeltingProgress > 0 && isSmeltingItem)//if temp not enough and progress is not 0, subtract value
        {
            Debug.Log(smeltingProgress + " AMOUNT OF SMELTING PROGRESS");
            yield return new WaitForSeconds(1f);
            StartCoroutine(SmeltItem(_item));
            smeltingProgress--;
            if (smeltingProgress < 0)
            {
                smeltingProgress = 0;
            }
        }
        else if (_item.itemSO.smeltValue > currentTemperature && smeltingProgress <= 0 && isSmeltingItem)//if temp not enough and progress is 0 or lower, do not subtract value but still run coroutine
        {
            Debug.Log(smeltingProgress + " AMOUNT OF SMELTING PROGRESS");
            yield return new WaitForSeconds(1f);
            StartCoroutine(SmeltItem(_item));
        }
        else if (smeltingProgress >= _item.itemSO.requiredSmeltingTime)//if we're done smelting, dont run coroutine
        {
            //_item.itemSO = _reward;
            OnFinishedSmelting?.Invoke(this, EventArgs.Empty);
            ResetSmeltingProgress();
            Debug.Log("done!");
        }
    }

    public void ResetSmeltingProgress()
    {
        Debug.Log("reset smelt");
        smeltingProgress = 0;
        isSmeltingItem = false;
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
            ReplaceWood?.Invoke(this, EventArgs.Empty);
            //obj.light.intensity = 0f;
            ResetSmeltingProgress();
            isSmelting = false;
            currentTemperature = 0;
            currentFuel = 0;
        }
    }

    public IEnumerator SpendTemperature()
    {
        if (currentTemperature > minTemperature && temperatureAtTarget)
        {
            yield return new WaitForSeconds(.05f);//.5 seconds = 10C 
            currentTemperature--;
            StartCoroutine(SpendTemperature());
        }
        else if (currentTemperature > 0 && !isSmelting)
        {
            yield return new WaitForSeconds(.05f);//.5 seconds = 10C
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
        //targetTemperature += bonusTemp;
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

    private IEnumerator UseBonusTemp()
    {
        if (bonusTemp > 0)
        {
            float tempBonusTemp = bonusTemp;
            float _timer = 0.0025f / (tempBonusTemp / 100);//1/4 second but gets smaller / faster the higher the bonus temp is
            yield return new WaitForSeconds(_timer);
            bonusTemp--;
            currentTemperature--;
            StartCoroutine(UseBonusTemp());
        }
    }

    public void Addtemperature(int _temperature)//we dont need to add, we need to just set 
    {
        currentTemperature += _temperature;
        bonusTemp += _temperature;
        targetTemperature += bonusTemp;
        //temperatureAtTarget = false;
        StartCoroutine(UseBonusTemp());
        if (currentTemperature > maxTemperature)
        {
            Explode();
        }
    }

    public void SetTemperature(int _temperature)
    {
        if (currentTemperature < _temperature && _temperature < maxTemperature)//change so explodes / breaks if set to higher than max instead of refusing to change temp
        {
            targetTemperature = _temperature + baseTemperature;
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
        //gameObject.GetComponent<RealWorldObject>().CheckBroken;
        Destroy(gameObject);
    }
}
