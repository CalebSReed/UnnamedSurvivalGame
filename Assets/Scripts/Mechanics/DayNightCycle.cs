using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.EventSystems;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }
    public bool timeIsTicking;

    public enum DayPart
    {
        Dawn,
        Day,
        Dusk,
        Night
    }

    public enum Season
    {
        Autumn,
        Winter,
        //monsoon, hurricane, 
    }

    public enum DayType
    {
        Normal,
        FullMoon,
        NewMoon,
        SolarEclipse,
        BloodMoon,
        BlueMoon,
        BlackMoon
    }

    public static readonly int fullDayTimeLength = 1440;//24 min instead of 24 hours. 24 min multiplied by 60 seconds for each minute: 
    public static readonly int fullYearLength = 40;//40 days. 20 of autumn / non winter and 20 of winter
    public static readonly int seasonLength = 20;//every season lasts the same amount of time
    public int currentYear;//save
    public int currentDayOfYear;//save
    public int currentDay;//save
    public float currentTime;//save
    public int currentSeasonProgress;
    public DayPart dayPart;
    public Season currentSeason;
    public DayType dayType;


    //public int percentageDawnLength;
    //public int percentageDayLength;
    //public int percentageDuskLength;
    //public int percentageNightLength;

    public float defaultDawnLength;
    public float defaultDayLength;
    public float defaultDuskLength;
    public float defaultNightLength;

    public float dawnLength;
    public float dayLength;
    public float duskLength;
    public float nightLength;

    public Light globalLight;

    /*public Gradient nightDawnGradient;
    public Gradient dawnDayGradient;
    public Gradient dayDuskGradient;
    public Gradient duskNightGradient;*/

    [SerializeField] Gradient autumnDayGradient;
    [SerializeField] Gradient autumnFogGradient;

    public Gradient autumnNightDawnGradient;
    public Gradient autumnDawnDayGradient;
    public Gradient autumnDayDuskGradient;
    public Gradient autumnDuskNightGradient;

    public Gradient winterNightDawnGradient;
    public Gradient winterDawnDayGradient;
    public Gradient winterDayDuskGradient;
    public Gradient winterDuskNightGradient;

    public event EventHandler OnDawn;
    public event EventHandler OnDay;
    public event EventHandler OnDusk;
    public event EventHandler OnNight;

    public event EventHandler OnSpring;
    public event EventHandler OnSummer;
    public event EventHandler OnAutumn;
    public event EventHandler OnWinter;

    public bool isDawn;
    public bool isDay;
    public bool isDusk;
    public bool isNight;

    public bool isLoading;

    private Animator anim;

    public TimeSaveData saveData = new TimeSaveData();
    private bool fogEnabled = true;
    [SerializeField] Camera mainCam;

    void Awake()
    {
        currentDay = 1;
        currentDayOfYear = 1;
        currentYear = 1;
        currentSeasonProgress = 1;
        dayPart = DayPart.Night;
        currentSeason = Season.Autumn;
        OnAutumn?.Invoke(this, EventArgs.Empty);
        Instance = this;
        anim = GetComponent<Animator>();

        //StartCoroutine(DoDayProgress());
    }

    private void OnLocalPlayerSpawned(object sender, EventArgs e)
    {
        timeIsTicking = true;
    }

    private void Start()
    {
        GameManager.Instance.OnJoinedServer += SendTimeRequest;
        GameManager.Instance.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
        //OnDawn += SendTimeRequest;
    }

    private void Update()
    {
        if (timeIsTicking)
        {
            DoDayProgress();
        }
    }

    private void SendTimeRequest(object sender, EventArgs e)
    {
        StartCoroutine(WaitToRequestTime());
    }

    private IEnumerator WaitToRequestTime()
    {
        yield return new WaitForSeconds(.5f);
        ClientHelper.Instance.RequestTimeDataRPC();
    }

    public void SyncTime(int year, int dayOfYear, float time, int day, int seasonProgress, int season, int dayType)
    {
        Debug.Log($"Syncing time! : {time}");
        currentYear = year;
        currentDayOfYear = dayOfYear;
        currentTime = time;
        currentDay = day;
        currentSeasonProgress = seasonProgress;
        currentSeason = (Season)season;
        this.dayType = (DayType)dayType;
        //isLoading = true;
        if (this.dayType == DayType.BlackMoon)
        {
            dawnLength = 0;
            dayLength = 0;
            duskLength = 0;
            nightLength = fullDayTimeLength;
        }
        else//default
        {
            dawnLength = defaultDawnLength;
            dayLength = defaultDayLength;
            duskLength = defaultDuskLength;
            nightLength = defaultNightLength;
        }
    }

    private void DoDayProgress()//EXAMPLE NOT REAL TIMES: dawn = 100, day = 200, dusk = 100, night = 100, fullday = 500
    { 
        if (currentTime <= dawnLength && dayPart != DayPart.Dawn)//0 to 180
        {
            DoDayPartTasks(DayPart.Dawn);
        }
        else if (currentTime > dawnLength && currentTime <= dawnLength + dayLength && dayPart != DayPart.Day)//if more than 100, and less than 200 + 100 = 300. more than 100 less than or equal to 300
        {
            DoDayPartTasks(DayPart.Day);
        }
        else if (currentTime > dawnLength + dayLength && currentTime <= dawnLength + dayLength + duskLength && dayPart != DayPart.Dusk)//if more than 300 (100 + 200), and less than or = 400 (100 + 200 + 100)
        {
            DoDayPartTasks(DayPart.Dusk);
        }
        else if (currentTime > dawnLength + dayLength + duskLength && currentTime <= fullDayTimeLength && dayPart != DayPart.Night)//fulldaytimelength is all parts added so 500
        {
            DoDayPartTasks(DayPart.Night);
        }
        else if (currentTime > fullDayTimeLength)
        {
            currentTime -= fullDayTimeLength;
            currentDay++;
            currentDayOfYear++;
            currentSeasonProgress++;
            CheckTimeOfYear();//reset currentTime, up daycount and yearcount if needed, change season if needed, change daypart lengths if needed
        }


        currentTime += Time.deltaTime;

        float timePercent = (float)currentTime / (float)fullDayTimeLength;

        //globalLight.transform.localRotation = Quaternion.Euler(new Vector3(25f, (timePercent * 360f * .75f) - 85f, 0f));

        anim.Play("AutumnCycle", 0, timePercent);

        globalLight.color = autumnDayGradient.Evaluate(timePercent);
        RenderSettings.fogColor = autumnFogGradient.Evaluate(timePercent);
        mainCam.backgroundColor = autumnFogGradient.Evaluate(timePercent);

        //globalLight.transform.Rotate(Vector3.up, (1f / fullDayTimeLength) * 360f, Space.World);
    }

    public void SetFog(bool on)
    {
        fogEnabled = on;
    }

    private void DoDayPartTasks(DayPart dayPart, bool loading = false)
    {
        this.dayPart = dayPart;
        if (loading)
        {
            switch (dayPart)
            {
                case DayPart.Dawn:
                    ResetBools("dawn");
                    break;
                case DayPart.Day:
                    ResetBools("day");
                    break;
                case DayPart.Dusk:
                    ResetBools("dusk");
                    break;
                case DayPart.Night:
                    ResetBools("night");
                    break;
            }
        }
        else
        {
            switch (dayPart)
            {
                case DayPart.Dawn:
                    ResetBools("dawn");
                    OnDawn?.Invoke(this, EventArgs.Empty);
                    break;
                case DayPart.Day:
                    ResetBools("day");
                    OnDay?.Invoke(this, EventArgs.Empty);
                    break;
                case DayPart.Dusk:
                    ResetBools("dusk");
                    OnDusk?.Invoke(this, EventArgs.Empty);
                    break;
                case DayPart.Night:
                    ResetBools("night");
                    OnNight?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }
    }

    private void CheckTimeOfYear()
    {
        Announcer.SetText($"Day {currentDay}");
        if (GameManager.Instance.isServer)
        {
            ClientHelper.Instance.SendTimeDataRPC(currentYear, currentDayOfYear, currentTime, currentDay, currentSeasonProgress, (int)currentSeason, (int)dayType);
        }
        //if (currentDay <= seasonLength)  currentDayInYear, reset on new year
        if (currentDay >= 10)
        {
            JournalNoteController.Instance.UnlockSpecificEntry("Day10");
        }

        if (currentDay >= 21)
        {
            JournalNoteController.Instance.UnlockSpecificEntry("Day21");
        }

        if (currentDayOfYear > fullYearLength)
        {
            currentDayOfYear = 1;
            currentYear++;
        }

        if (currentSeasonProgress > 20)
        {
            switch (currentSeason)//season transition
            {
                default:
                    break;
                /*case Season.Spring: currentSeason = Season.Summer;
                    OnSummer?.Invoke(this, EventArgs.Empty);
                    break;
                case Season.Summer: currentSeason = Season.Autumn;
                    OnAutumn?.Invoke(this, EventArgs.Empty);
                    break;*/
                case Season.Autumn: currentSeason = Season.Winter;
                    OnWinter?.Invoke(this, EventArgs.Empty);
                    break;
                case Season.Winter: currentSeason = Season.Autumn;
                    OnSpring?.Invoke(this, EventArgs.Empty);
                    break;
            }
            currentSeasonProgress = 1;
        }

        if (dayType != DayType.Normal)
        {
            dayType = DayType.Normal;
            dawnLength = defaultDawnLength;
            dayLength = defaultDayLength;
            duskLength = defaultDuskLength;
            nightLength = defaultNightLength;
            ClientHelper.Instance.SendTimeDataRPC(currentYear, currentDayOfYear, currentTime, currentDay, currentSeasonProgress, (int)currentSeason, (int)dayType);
            return;
        }

        if (!GameManager.Instance.isServer)
        {
            return;
        }

        int _rand = UnityEngine.Random.Range(0, 11);
        if (_rand == 10 && currentDay >= 10)
        {
            dayType = DayType.BlackMoon;
            dawnLength = 0;
            dayLength = 0;
            duskLength = 0;
            nightLength = fullDayTimeLength;
            JournalNoteController.Instance.UnlockSpecificEntry("BlackMoon");
            GameManager.Instance.Save();
        }
        ClientHelper.Instance.SendTimeDataRPC(currentYear, currentDayOfYear, currentTime, currentDay, currentSeasonProgress, (int)currentSeason, (int)dayType);
    }

    private void ResetBools(string _timeOfDay)
    {
        if (_timeOfDay == "dawn")
        {
            isDawn = true;
            isDay = false;
            isDusk = false;
            isNight = false;
        }
        else if (_timeOfDay == "day")
        {
            isDawn = false;
            isDay = true;
            isDusk = false;
            isNight = false;
        }
        else if (_timeOfDay == "dusk")
        {
            isDawn = false;
            isDay = false;
            isDusk = true;
            isNight = false;
        }
        else if (_timeOfDay == "night")
        {
            isDawn = false;
            isDay = false;
            isDusk = false;
            isNight = true;
        }
    }

    public void SaveTime()
    {
        saveData.currentDay = currentDay;
        saveData.currentDayOfYear = currentDayOfYear;
        saveData.currentDayType = (int)dayType;
        saveData.currentSeason = (int)currentSeason;
        saveData.currentTime = currentTime;
        saveData.currentYear = currentYear;
        saveData.currentSeasonProgress = currentSeasonProgress;
    }

    public void LoadNewTime(float _currentTime, int _currentDay, int _currentDayOfYear, int _currentYear, int _currentSeason, int _seasonProg, int _dayType)
    {
        currentTime = _currentTime;
        currentDay = _currentDay;
        currentDayOfYear = _currentDayOfYear;
        currentYear = _currentYear;
        currentSeason = (Season)_currentSeason;
        currentSeasonProgress = _seasonProg;
        dayType = (DayType)_dayType;
        if (dayType == DayType.BlackMoon)
        {
            dayType = DayType.BlackMoon;
            dawnLength = 0;
            dayLength = 0;
            duskLength = 0;
            nightLength = fullDayTimeLength;
        }
        isLoading = true;
        StopAllCoroutines();
        LoadDayPart();
        //StartCoroutine(DoDayProgress());
        timeIsTicking = true;
        //StartCoroutine(CheckIfStillLoading());
    }

    private void LoadDayPart()
    {
        if (currentTime <= dawnLength)//0 to 180
        {
            DoDayPartTasks(DayPart.Dawn, true);
        }
        else if (currentTime > dawnLength && currentTime <= dawnLength + dayLength)//if more than 100, and less than 200 + 100 = 300. more than 100 less than or equal to 300
        {
            DoDayPartTasks(DayPart.Day, true);
        }
        else if (currentTime > dawnLength + dayLength && currentTime <= dawnLength + dayLength + duskLength)//if more than 300 (100 + 200), and less than or = 400 (100 + 200 + 100)
        {
            DoDayPartTasks(DayPart.Dusk, true);
        }
        else if (currentTime > dawnLength + dayLength + duskLength && currentTime <= fullDayTimeLength)//fulldaytimelength is all parts added so 500
        {
            DoDayPartTasks(DayPart.Night, true);
        }
    }
}
