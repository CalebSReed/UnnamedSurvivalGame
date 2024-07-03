using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureReceiver : MonoBehaviour//this should depend on tempEmitter and NEVER the other way around... hopefully!
{
    //Base temperature will be based on season progress + any weather changes + clothing bonuses + time of day?
    //lets say freezing temps are below 0 and overheating is over 100?
    //lets say your body always wants to return to base temp?
    //
    public PlayerMain player;
    public int baseTemp { get; private set; }
    public int currentTemp { get; private set; }
    public int targetTemp { get; private set; }

    private bool tryingToReachTargetTemp;

    public int insulationMultiplier { get; private set; }
    public int rainProtectionMultiplier { get; private set; }
    public int temperatureMultiplier { get; private set; }

    private WaitForSeconds oneSecond = new WaitForSeconds(1);
    private WaitForSeconds hundrethSecond = new WaitForSeconds(.01f);

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

        tryingToReachTargetTemp = true;
        if (tempTargetTemp > targetTemp)//if new temp is hotter
        {
            targetTemp = tempTargetTemp;
            while (currentTemp < targetTemp)//while currenttemp is colder than new target
            {
                currentTemp += 1;
                yield return oneSecond;
            }
        }
        else if (tempTargetTemp < targetTemp)//if new temp is colder
        {
            targetTemp = tempTargetTemp;
            while (currentTemp > targetTemp)//while current is hotter than target
            {
                currentTemp -= 1;
                yield return oneSecond;
            }
        }
        tryingToReachTargetTemp = false;
        targetTemp = baseTemp;
    }

    private IEnumerator ReturnToBaseTemperature()
    {
        if (currentTemp > baseTemp && !tryingToReachTargetTemp)//dont change while gaining or losing temp from other sources
        {
            currentTemp--;
        }
        else if (currentTemp < baseTemp && !tryingToReachTargetTemp)
        {
            currentTemp++;
        }

        if (currentTemp >= 100 && currentTemp >= baseTemp)
        {
            yield return hundrethSecond;
        }
        else if (currentTemp <= 0 && currentTemp <= baseTemp)
        {
            yield return hundrethSecond;
        }
        else
        {
            yield return new WaitForSeconds(insulationMultiplier);//slower change is better.
        }
        StartCoroutine(ReturnToBaseTemperature());
    }

    public IEnumerator ChangeBaseTemperature()
    {
        yield return oneSecond;
        baseTemp = 50; //temporary set until i add seasons
        switch (DayNightCycle.Instance.currentSeason)
        {
            default:
                break;
            /*case DayNightCycle.Season.Spring: baseTemp = 25;//will never freeze u with normal weather. but rain will cause freezing during dawn and night
                    break;
            case DayNightCycle.Season.Summer: baseTemp = 100;//will overheat at day and dusk.
                break;*/
            case DayNightCycle.Season.Autumn: baseTemp = 30;//feels good. Starting in autumn seems kinda weird so maybe year 1 spring will be baby mode
                break;
            case DayNightCycle.Season.Winter: baseTemp = -25;//freeze during dawn and night, and snow should cause freezing during all times
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

        Cell.BiomeType _currentBiome = Cell.BiomeType.Forest;
        GameManager.Instance.world.existingTileDictionary.TryGetValue(new Vector2Int(player.cellPosition[0] + GameManager.Instance.world.worldSize, player.cellPosition[1] + GameManager.Instance.world.worldSize), out GameObject _tile);
        if (_tile != null)
        {
            _currentBiome = _tile.GetComponent<Cell>().biomeType;
        }
        else
        {
            Debug.Log("Biome was not found!");
        }

        switch (_currentBiome)
        {
            default:
                break;
            case Cell.BiomeType.Desert:
                baseTemp += 95;
                break;
            case Cell.BiomeType.Snowy:
                baseTemp -= 75;
                break;
        }

        if (WeatherManager.Instance.isRaining)
        {
            //baseTemp -= 20;
            //baseTemp += rainProtectionMultiplier;//make sure to cap at max rain temperature decrement 
        }

        if (!tryingToReachTargetTemp)
        {
            targetTemp = baseTemp;
        }

        if (_currentBiome == Cell.BiomeType.Desert && temperatureMultiplier >= 0 || _currentBiome == Cell.BiomeType.Desert && insulationMultiplier == 1)//having default insulation OR  causes temperature shocks, punishing the player for wearing nothing
        {
            currentTemp = baseTemp;
        }
        else if (_currentBiome == Cell.BiomeType.Snowy && temperatureMultiplier <= 0 || _currentBiome == Cell.BiomeType.Snowy && insulationMultiplier == 1)
        {
            currentTemp = baseTemp;
        }

        baseTemp += temperatureMultiplier;

        StartCoroutine(ChangeBaseTemperature());
    }

    private IEnumerator CheckTemperature()
    {
        if (currentTemp < 0)
        {
            if (currentTemp <= -20)
            {
                GetComponent<HealthManager>().TakeDamage(4, "Freezing", gameObject);
            }
            else
            {
                GetComponent<HealthManager>().TakeDamage(1, "Freezing", gameObject);//take one damage on a value of time based on insulation?? or should always be one second?
            }
        }
        else if (currentTemp > 100)
        {
            if (currentTemp >= 120)
            {
                GetComponent<HealthManager>().TakeDamage(4, "Overheating", gameObject);
            }
            else
            {
                GetComponent<HealthManager>().TakeDamage(1, "Overheating", gameObject);
            }
        }
        else
        {
            GetComponent<PlayerMain>().DisableTemperatureVignettes();
        }
        yield return oneSecond;
        StartCoroutine(CheckTemperature());
    }

    public void ChangeInsulation(int _newVal)
    {
        insulationMultiplier += _newVal;
    }

    public void ChangeRainProtection(int newVal)
    {
        rainProtectionMultiplier += newVal;
    }

    public void ChangeTemperatureValue(int newVal)
    {
        temperatureMultiplier += newVal;
    }

    public void ResetTemperature()
    {
        currentTemp = 50;
        tryingToReachTargetTemp = false;
        targetTemp = 50;
        StopAllCoroutines();

        StartCoroutine(ChangeBaseTemperature());
        StartCoroutine(ReturnToBaseTemperature());
        StartCoroutine(CheckTemperature());
    }
}
