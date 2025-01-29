using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Random = UnityEngine.Random;
using Unity.Netcode;

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
    public WeatherSaveData weatherSave = new WeatherSaveData();

    //weather, rain, thunder, hail, snow
    [field: SerializeField] public int rainProgress { get; private set; }
    public int thunderProgress { get; private set; }
    public int rainTarget { get; private set; }
    public int thunderTarget { get; private set; }
    public int stormCooldown { get; private set; }
    public bool isRaining { get; private set; }
    public bool targetReached { get; private set; }

    private bool regrowingShrooms;

    private Coroutine shroomRoutine;

    public EventHandler onRaining;

    private bool loading = false;
    //temperature from global temperature

    void Awake()
    {
        Instance = this;
        stormCooldown = 0;
        rainProgress = 0;
        thunderProgress = 0;
        /*
        var em = rainSystem.emission;
        em.rateOverTime = 0f;
        var em2 = rainSplashSystem.emission;
        em2.rateOverTime = 0f;
        */
        rainTarget = 50;
        thunderTarget = 75;

        rainSystem = transform.GetChild(0).GetComponent<ParticleSystem>();
        rainSplashSystem = transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        DayNightCycle.Instance.OnDawn += WeatherCheck;
        GameManager.Instance.OnJoinedServer += AskForWeatherData;
        DayNightCycle.Instance.OnDawn += AskForWeatherData;
        GameManager.Instance.OnLocalPlayerSpawned += BeginWeather;
    }

    private void Update()
    {
        if (GameManager.Instance.localPlayer != null)
        {
            rainSystem.transform.position = GameManager.Instance.localPlayer.transform.position;
        }
    }

    private void BeginWeather(object sender, EventArgs e)
    {
        StartCoroutine(WeatherProgress());
    }

    private IEnumerator WeatherProgress()//should new storms only start on a new day? or should they begin whenever?
    {
        if (!GameManager.Instance.isServer)
        {
            yield break;
        }
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

        if (isRaining && !regrowingShrooms && DayNightCycle.Instance.currentSeason != DayNightCycle.Season.Winter)
        {
            regrowingShrooms = true;
            shroomRoutine = StartCoroutine(RegrowPlants());
        }
        else if (!isRaining && regrowingShrooms)
        {
            regrowingShrooms = false;
            if (shroomRoutine != null)
            {
                StopCoroutine(shroomRoutine);
            }
        }

        yield return new WaitForSeconds(1);//set to 10 seconds 
        loading = false;
        StartCoroutine(WeatherProgress());
    }

    private IEnumerator RegrowPlants()
    {
        yield return new WaitForSeconds(20);
        if (DayNightCycle.Instance.currentSeason == DayNightCycle.Season.Winter)
        {
            yield break;
        }
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Tile"))
        {
            if (_obj.GetComponent<Cell>() == null)
            {
                continue;
            }
            Cell cell = _obj.GetComponent<Cell>();
            if (cell.biomeType == Cell.BiomeType.Grasslands || cell.biomeType == Cell.BiomeType.Forest || cell.biomeType == Cell.BiomeType.Savannah)
            {
                int rand = Random.Range(0, 3);
                if (rand == 0)
                {
                    WorldGeneration.Instance.GenerateTileObject("object", .25f, "BrownShroom", (int)cell.tileData.tileLocation.x, (int)cell.tileData.tileLocation.y, cell.tileData, _obj.transform.position);
                }
                else if (rand == 1)
                {
                    WorldGeneration.Instance.GenerateTileObject("object", .25f, "Tork Shroom", (int)cell.tileData.tileLocation.x, (int)cell.tileData.tileLocation.y, cell.tileData, _obj.transform.position);
                }
                else if (rand == 2)
                {
                    WorldGeneration.Instance.GenerateTileObject("object", .05f, "Gold Morel", (int)cell.tileData.tileLocation.x, (int)cell.tileData.tileLocation.y, cell.tileData, _obj.transform.position);
                }
                WorldGeneration.Instance.GenerateTileObject("object", .01f, "gyreflower", (int)cell.tileData.tileLocation.x, (int)cell.tileData.tileLocation.y, cell.tileData, _obj.transform.position);
                WorldGeneration.Instance.GenerateTileObject("object", .01f, "opalflower", (int)cell.tileData.tileLocation.x, (int)cell.tileData.tileLocation.y, cell.tileData, _obj.transform.position);
            }
        }
        shroomRoutine = StartCoroutine(RegrowPlants());
    }

    private void AskForWeatherData(object sender, EventArgs e)
    {
        StartCoroutine(WaitToRequest());
    }

    private IEnumerator WaitToRequest()
    {
        yield return new WaitForSeconds(.5f);
        Debug.Log("Asking for weather data!");
        ClientHelper.Instance.RequestWeatherRPC();
    }

    public void SyncWeatherData(bool isRaining, bool targetReached, int rainProg)
    {
        Debug.Log("Got the weather data!");
        this.isRaining = isRaining;
        this.targetReached = targetReached;
        rainProgress = rainProg;

        if (isRaining)
        {
            StartCoroutine(StartRaining());
        }
        else
        {
            StartCoroutine(StopRaining());
        }
    }

    private void WeatherCheck(object sender, EventArgs e)
    {
        if (!GameManager.Instance.isServer)
        {
            return;
        }
        switch (DayNightCycle.Instance.currentSeason)
        {
            default:
                break;
            /*case DayNightCycle.Season.Spring: rainTarget = 50;
                thunderTarget = 50;
                break;
            case DayNightCycle.Season.Summer: rainTarget = 150;
                thunderTarget = 25;
                break;*/
            case DayNightCycle.Season.Autumn: rainTarget = 50;
                thunderTarget = 100;
                break;
            case DayNightCycle.Season.Winter: rainTarget = 25;
                thunderTarget = 150;
                break;
        }

        if (rainProgress >= rainTarget && stormCooldown == 0 && !isRaining)
        {           
            StartCoroutine(StartRaining());
        }

        if (rainProgress <= 0 && isRaining)
        {
            StartCoroutine(StopRaining());
        }
    }

    public IEnumerator StartRaining()
    {
        onRaining?.Invoke(this, EventArgs.Empty);
        JournalNoteController.Instance.UnlockSpecificEntry("Rain");
        targetReached = false;
        isRaining = true;
        Light light = DayNightCycle.Instance.GetComponent<Light>();
        if (!loading)
        {
            while (rainSystem.emissionRate < 50)
            {
                yield return new WaitForSeconds(1f);
                rainSystem.emissionRate++;
                rainSplashSystem.emissionRate += 1f;
                light.intensity -= .02f;
            }
        }
        rainSystem.emissionRate = 50;
        rainSplashSystem.emissionRate = 25;
        light.intensity = 2f;
    }

    public IEnumerator StopRaining()
    {
        targetReached = false;
        Light light = DayNightCycle.Instance.GetComponent<Light>();
        if (!loading)
        {
            while (rainSystem.emissionRate > 0)
            {
                yield return new WaitForSeconds(1f);
                rainSystem.emissionRate--;
                rainSplashSystem.emissionRate -= .5f;
                light.intensity += .02f;
            }
        }
        rainSystem.emissionRate = 0;
        rainSplashSystem.emissionRate = 0;
        isRaining = false;
        light.intensity = 3;
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

    public void SaveWeather()
    {
        weatherSave.isRaining = isRaining;
        weatherSave.rainProgress = rainProgress;
        weatherSave.stormCooldown = stormCooldown;
        weatherSave.targetReached = targetReached;
        weatherSave.thunderProgress = thunderProgress;
    }

    public void LoadWeatherData(int _rainProg, int _thundProg, int _cooldown, bool _raining, bool _target)
    {
        rainProgress = _rainProg;
        thunderProgress = _thundProg;
        stormCooldown = _cooldown;
        isRaining = _raining;
        targetReached = _target;
        loading = true;
        if (isRaining)
        {
            StartCoroutine(StartRaining());
        }
        else
        {
            StartCoroutine(StopRaining());
        }
    }
}
