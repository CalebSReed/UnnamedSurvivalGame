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

    public int insulationMultiplier { get; private set; }

    private void Start()
    {
        currentTemp = 50;
        insulationMultiplier = 1;
        StartCoroutine(ChangeBaseTemperature());
        StartCoroutine(ReturnToBaseTemperature());
        StartCoroutine(CheckTemperature());
    }

    public IEnumerator ReceiveTemperature(int _newTemp)
    {
        int _tempProg = _newTemp;
        int _valChange = _newTemp / Mathf.Abs(_newTemp);//-25/25 = -1 OR 25/25 = 1 so we only do +1 or -1 over time
        while (_tempProg > 0)
        {
            currentTemp += _valChange;
            _tempProg--;
            yield return new WaitForSeconds(insulationMultiplier/2);
        }
    }

    private IEnumerator ReturnToBaseTemperature()
    {
        if (currentTemp > baseTemp)
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
        //switch(DayNightCycle.Instance.currentSeason)
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
            baseTemp -= 20;//inside add more temp depending on player's rain protection
        }

        yield return new WaitForSeconds(1);
        StartCoroutine(ChangeBaseTemperature());
    }

    private IEnumerator CheckTemperature()
    {
        if (currentTemp > 100 || currentTemp < 0)
        {
            GetComponent<HealthManager>().TakeDamage(1);//take one damage on a value of time based on insulation?? or should always be one second?
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(CheckTemperature());
    }
}
