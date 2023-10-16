using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureReceiver : MonoBehaviour//this should depend on tempEmitter and NEVER the other way around... hopefully!
{
    //Base temperature will be based on season progress + any weather changes + clothing bonuses + time of day?
    //lets say freezing temps are below 0 and overheating is over 100?
    //lets say your body always wants to return to base temp?
    //

    public int baseTemp { get; private set; }
    public int currentTemp { get; private set; }
    public int targetTemp { get; private set; }

    private bool tryingToReachTargetTemp;

    public int insulationMultiplier { get; private set; }

    private void Start()
    {
        currentTemp = 50;
        insulationMultiplier = 1;
        StartCoroutine(ChangeBaseTemperature());
        StartCoroutine(ReturnToBaseTemperature());
        StartCoroutine(CheckTemperature());
    }

    public IEnumerator ReceiveTemperature(int _newTemp)//make it so we stop when not receiving temperature
    {
        int tempTargetTemp = baseTemp + _newTemp;
        if (_newTemp > 0 && tempTargetTemp <= targetTemp)//if we are trying to get warmer, and the new target is colder than the old one than stop
        {
            tryingToReachTargetTemp = false;
            yield break;
        }
        if (_newTemp < 0 && tempTargetTemp >= targetTemp)//if trying to get colder, and new target is warmer we stop
        {
            tryingToReachTargetTemp = false;
            yield break;
        }
        //targetTemp = tempTargetTemp;

        //int _tempProg = _newTemp;
        //int _valChange = _newTemp / Mathf.Abs(_newTemp);//-25/25 = -1 OR 25/25 = 1 so we only do +1 or -1 over time
        tryingToReachTargetTemp = true;
        if (tempTargetTemp > targetTemp)//if new temp is hotter
        {
            targetTemp = tempTargetTemp;
            while (currentTemp < targetTemp)//while currenttemp is colder than new target
            {
                currentTemp += 1;
                yield return new WaitForSeconds((float)insulationMultiplier / 4);
            }
        }
        else if (tempTargetTemp < targetTemp)//if new temp is colder
        {
            targetTemp = tempTargetTemp;
            while (currentTemp > targetTemp)//while current is hotter than target
            {
                currentTemp -= 1;
                yield return new WaitForSeconds((float)insulationMultiplier / 4);
            }
        }
        tryingToReachTargetTemp = false;
        targetTemp = baseTemp;
    }

    private IEnumerator ReturnToBaseTemperature()
    {
        if (currentTemp > baseTemp)//dont change while gaining or losing temp from other sources
        {
            currentTemp--;
        }
        else if (currentTemp < baseTemp)
        {
            currentTemp++;
        }
        yield return new WaitForSeconds(insulationMultiplier);//slower change is better.
        StartCoroutine(ReturnToBaseTemperature());
    }

    public IEnumerator ChangeBaseTemperature()
    {
        baseTemp = 50; //temporary set until i add seasons
        switch (DayNightCycle.Instance.currentSeason)
        {
            default:
                break;
            case DayNightCycle.Season.Spring: baseTemp = 25;//will never freeze u with normal weather. but rain will cause freezing during dawn and night
                    break;
            case DayNightCycle.Season.Summer: baseTemp = 100;//will overheat at day and dusk.
                break;
            case DayNightCycle.Season.Autumn: baseTemp = 50;//feels good. Starting in autumn seems kinda weird so maybe year 1 spring will be baby mode
                break;
            case DayNightCycle.Season.Winter: baseTemp = 0;//freeze during dawn and night, and snow should cause freezing during all times
                break;
        }
        switch (DayNightCycle.Instance.dayPart)
        {
            default: 
                break;
            case DayNightCycle.DayPart.Dawn: baseTemp -= 10;
                break;
            case DayNightCycle.DayPart.Day: baseTemp += 20;
                break;
            case DayNightCycle.DayPart.Dusk: baseTemp += 10;
                break;
            case DayNightCycle.DayPart.Night: baseTemp -= 20;
                break;
        }

        if (WeatherManager.Instance.isRaining)
        {
            //enable later
            //baseTemp -= 20;//inside add more temp depending on player's rain protection and maybe add more levels of rainfall?
        }

        if (!tryingToReachTargetTemp)
        {
            targetTemp = baseTemp;
        }

        yield return new WaitForSeconds(1);
        StartCoroutine(ChangeBaseTemperature());
    }

    private IEnumerator CheckTemperature()
    {
        if (currentTemp < 0)
        {
            GetComponent<HealthManager>().TakeDamage(1, "Freezing", gameObject);//take one damage on a value of time based on insulation?? or should always be one second?
        }
        else if (currentTemp > 100)
        {
            GetComponent<HealthManager>().TakeDamage(1, "Overheating", gameObject);
        }
        else
        {
            GetComponent<PlayerMain>().DisableTemperatureVignettes();
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(CheckTemperature());
    }

    public void ChangeInsulation(int _newVal)
    {
        insulationMultiplier += _newVal;
    }
}
