using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using System;
using Random = UnityEngine.Random;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType
    {
        Rain,
        Thunder,
        Hail,
        Snow,
        Fog
    }

    public static WeatherManager Instance { get; private set; }
    public ParticleSystem rainSystem;
    public ParticleSystem rainSplashSystem;

    //weather, rain, thunder, hail, snow
    public int rainProgress { get; private set; }
    public int thunderProgress { get; private set; }
    public int rainTarget { get; private set; }
    public int thunderTarget { get; private set; }
    public int stormCooldown { get; private set; }
    public bool isRaining { get; private set; }
    public bool targetReached { get; private set; }



    private bool loading = false;
    //temperature from global temperature

    void Awake()
    {
        Instance = this;
        stormCooldown = 0;
        rainProgress = 0;
        thunderProgress = 0;
        rainSystem.emissionRate = 0;
        rainSplashSystem.emissionRate = 0;
        rainTarget = 100;
        thunderTarget = 100;
        DayNightCycle.Instance.OnDawn += WeatherCheck;
        StartCoroutine(WeatherProgress());
    }

    private IEnumerator WeatherProgress()//should new storms only start on a new day? or should they begin whenever?
    {
        var newVal = Random.Range(0,2);
        if (rainProgress >= rainTarget && !isRaining || rainProgress <= 0 && isRaining)
        {
            targetReached = true;
        }
        else if (newVal == 0 && rainProgress > 0 && !targetReached || rainProgress >= rainTarget && !targetReached)//50% chance if at 0 or less then always increase. if above 100, go lower
        {
            rainProgress--;
        }
        else
        {
            rainProgress++;
        }

        newVal = Random.Range(0, 13);
        if (newVal == 0 && isRaining)//slight bias to lose progress while raining so that storms dont last forever lol
        {
            rainProgress--;
        }

        newVal = Random.Range(0, 2);
        if (newVal == 0 && thunderProgress > 0 && thunderProgress < thunderTarget)//add thunderstorms later... and other types of storms too lul :3 dont forget to save new storms lol
        {
            //thunderProgress--;
        }
        else
        {
            //thunderProgress++;
        }
        yield return new WaitForSeconds(1);//set to 10 seconds 
        loading = false;
        StartCoroutine(WeatherProgress());
    }

    private void WeatherCheck(object sender, EventArgs e)
    {
        switch (DayNightCycle.Instance.currentSeason)
        {
            default:
                break;
            case DayNightCycle.Season.Spring: rainTarget = 50;
                thunderTarget = 50;
                break;
            case DayNightCycle.Season.Summer: rainTarget = 150;
                thunderTarget = 25;
                break;
            case DayNightCycle.Season.Autumn: rainTarget = 100;
                thunderTarget = 100;
                break;
            case DayNightCycle.Season.Winter: rainTarget = 50;
                thunderTarget = 150;
                break;
        }

        if (rainProgress >= 100 && stormCooldown == 0 && !isRaining)
        {
            StartCoroutine(StartRaining());
        }

        if (rainProgress <= 0 && isRaining)
        {
            StartCoroutine(StopRaining());
        }
    }

    private IEnumerator StartRaining()
    {
        targetReached = false;
        isRaining = true;
        Light2D light = DayNightCycle.Instance.GetComponent<Light2D>();
        if (!loading)
        {
            while (rainSystem.emissionRate < 50)
            {
                yield return new WaitForSeconds(1f);
                rainSystem.emissionRate++;
                rainSplashSystem.emissionRate += .5f;
                light.intensity -= .01f;
            }
        }
        rainSystem.emissionRate = 50;
        rainSplashSystem.emissionRate = 25;
        light.intensity = .5f;
    }

    private IEnumerator StopRaining()
    {
        targetReached = false;
        Light2D light = DayNightCycle.Instance.GetComponent<Light2D>();
        if (!loading)
        {
            while (rainSystem.emissionRate > 0)
            {
                yield return new WaitForSeconds(1f);
                rainSystem.emissionRate--;
                rainSplashSystem.emissionRate -= .5f;
                light.intensity += .01f;
            }
        }
        rainSystem.emissionRate = 0;
        rainSplashSystem.emissionRate = 0;
        isRaining = false;
        light.intensity = 1;
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

    public void LoadWeatherData(int _rainProg, int _thundProg, int _cooldown, bool _raining, bool _target)
    {
        rainProgress = _rainProg;
        thunderProgress = _thundProg;
        stormCooldown = _cooldown;
        isRaining = _raining;
        targetReached = _target;
        loading = true;
    }
}
