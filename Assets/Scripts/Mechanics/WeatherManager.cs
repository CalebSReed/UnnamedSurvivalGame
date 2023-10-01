using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }
    public ParticleSystem rainSystem;

    //weather, rain, thunder, hail, snow
    private int rainProgress;
    private int thunderProgress;
    private int stormCooldown = 5;
    //temperature from global temperature

    void Awake()
    {
        Instance = this;
        rainProgress = 0;
        thunderProgress = 0;
        StartCoroutine(CheckWeather());
    }

    private IEnumerator CheckWeather()//should new storms only start on a new day? or should they 
    {
        var newVal = Random.Range(0,2);
        if (newVal == 0)//50% chance
        {
            rainProgress--;
        }
        else
        {
            rainProgress++;
        }
        newVal = Random.Range(0, 11);
        if (newVal == 10)//10% chance
        {
            thunderProgress++;
        }
        else
        {
            thunderProgress--;
        }

        if (rainProgress >= 100 && stormCooldown == 0)
        {
            StartRaining();
        }

        yield return new WaitForSeconds(1);//set to 10 seconds 
        StartCoroutine(CheckWeather());
    }

    private IEnumerator StartRaining()
    {
        rainSystem.emissionRate = 0;//shutup unity 

    }

    private void StartThunderStorm()
    {

    }

    private void StartHailStorm()
    {

    }

    private void StartSnowing()
    {

    }
}
