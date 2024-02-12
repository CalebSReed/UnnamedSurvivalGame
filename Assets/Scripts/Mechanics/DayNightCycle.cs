using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.EventSystems;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

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
    public int currentTime;//save
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

    private Coroutine lastTransition;
    public bool isLoading;

    void Awake()
    {
        currentDay = 1;
        currentDayOfYear = 1;
        currentYear = 1;
        currentSeasonProgress = 1;
        dayPart = DayPart.Night;
        currentSeason = Season.Autumn;
        OnSpring?.Invoke(this, EventArgs.Empty);
        Instance = this;        
        StartCoroutine(DoDayProgress());
    }

    private IEnumerator DoDayProgress()//EXAMPLE NOT REAL TIMES: dawn = 100, day = 200, dusk = 100, night = 100, fullday = 500
    {
        yield return new WaitForSeconds(1);

        if (currentTime <= dawnLength && dayPart != DayPart.Dawn)//0 to 180
        {
            if (lastTransition != null)
            {
                StopCoroutine(lastTransition);
            }
            SetDayPart(DayPart.Dawn);
        }
        else if (currentTime > dawnLength && currentTime <= dawnLength + dayLength && dayPart != DayPart.Day)//if more than 100, and less than 200 + 100 = 300. more than 100 less than or equal to 300
        {
            if (lastTransition != null)
            {
                StopCoroutine(lastTransition);
            }
            SetDayPart(DayPart.Day);
        }
        else if (currentTime > dawnLength + dayLength && currentTime <= dawnLength + dayLength + duskLength && dayPart != DayPart.Dusk)//if more than 300 (100 + 200), and less than or = 400 (100 + 200 + 100)
        {
            if (lastTransition != null)
            {
                StopCoroutine(lastTransition);
            }
            SetDayPart(DayPart.Dusk);
        }
        else if (currentTime > dawnLength + dayLength + duskLength && currentTime <= fullDayTimeLength && dayPart != DayPart.Night)//fulldaytimelength is all parts added so 500
        {
            if (lastTransition != null)
            {
                StopCoroutine(lastTransition);
            }
            SetDayPart(DayPart.Night);
        }
        else if (currentTime > fullDayTimeLength)
        {
            currentTime = 0;
            currentDay++;
            currentDayOfYear++;
            currentSeasonProgress++;
            CheckTimeOfYear();//reset currentTime, up daycount and yearcount if needed, change season if needed, change daypart lengths if needed
        }


        currentTime++;

        StartCoroutine(DoDayProgress());
    }

    private IEnumerator CheckIfStillLoading()
    {
        yield return new WaitForSeconds(.5f);
        if (isLoading)//get rid of this???
        {
            Debug.LogError("not loading anymore");
            isLoading = false;
        }
    }

    private void SetDayPart(DayPart _part)
    {
        dayPart = _part;
        switch (_part)
        {
            case DayPart.Dawn:
                switch (currentSeason)
                {
                    /*case Season.Spring:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, springNightDawnGradient));
                        break;
                    case Season.Summer:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, summerNightDawnGradient));
                        break;*/
                    case Season.Autumn:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, autumnNightDawnGradient));
                        break;
                    case Season.Winter:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, winterNightDawnGradient));
                        break;
                }
                break;
            case DayPart.Day:
                switch (currentSeason)
                {
                    /*case Season.Spring:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, springDawnDayGradient));
                        break;
                    case Season.Summer:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, summerDawnDayGradient));
                        break;*/
                    case Season.Autumn:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, autumnDawnDayGradient));
                        break;
                    case Season.Winter:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, winterDawnDayGradient));
                        break;
                }
                break;
            case DayPart.Dusk:
                switch (currentSeason)
                {/*
                    case Season.Spring:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, springDayDuskGradient));
                        break;
                    case Season.Summer:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, summerDayDuskGradient));
                        break;*/
                    case Season.Autumn:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, autumnDayDuskGradient));
                        break;
                    case Season.Winter:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, winterDayDuskGradient));
                        break;
                }
                break;
            case DayPart.Night:
                switch (currentSeason)
                {/*
                    case Season.Spring:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, springDuskNightGradient));
                        break;
                    case Season.Summer:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, summerDuskNightGradient));
                        break;*/
                    case Season.Autumn:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, autumnDuskNightGradient));
                        break;
                    case Season.Winter:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, winterDuskNightGradient));
                        break;
                }
                break;
        }
    }

    //globalLight.color = nightDawnGradient.Evaluate(1);

    private IEnumerator DoDayPartTransition(DayPart dayPart, Gradient gradient)
    {
        Debug.Log("Transitioning...");
        if (isLoading)
        {
            globalLight.color = gradient.Evaluate(1);
            DoDayPartTasks(dayPart, true);
            isLoading = false;
            Debug.Log("WasLoading... caught immediately " + dayPart);
            yield break;
        }
        else
        {
            float i = .01f;
            while (i < 1)
            {
                if (isLoading)
                {
                    globalLight.color = gradient.Evaluate(1);
                    DoDayPartTasks(dayPart, true);
                    isLoading = false;
                    Debug.Log("WasLoading... " + dayPart);
                    yield break;
                }
                globalLight.color = gradient.Evaluate(i);
                yield return new WaitForSeconds(.5f);
                i += .01f;
            }
        }

        DoDayPartTasks(dayPart);
    }

    private void DoDayPartTasks(DayPart dayPart, bool loading = false)
    {
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
        //if (currentDay <= seasonLength)  currentDayInYear, reset on new year
        if (currentDay >= 10)
        {
            JournalNoteController.Instance.UnlockSpecificEntry("Day10");
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
        }
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

    public void LoadNewTime(int _currentTime, int _currentDay, int _currentDayOfYear, int _currentYear, int _currentSeason, int _seasonProg, int _dayType)
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
        StartCoroutine(DoDayProgress());
        //StartCoroutine(CheckIfStillLoading());
    }

    private void LoadDayPart()
    {
        if (currentTime <= dawnLength)//0 to 180
        {
            SetDayPart(DayPart.Dawn);
        }
        else if (currentTime > dawnLength && currentTime <= dawnLength + dayLength)//if more than 100, and less than 200 + 100 = 300. more than 100 less than or equal to 300
        {
            SetDayPart(DayPart.Day);
        }
        else if (currentTime > dawnLength + dayLength && currentTime <= dawnLength + dayLength + duskLength)//if more than 300 (100 + 200), and less than or = 400 (100 + 200 + 100)
        {
            SetDayPart(DayPart.Dusk);
        }
        else if (currentTime > dawnLength + dayLength + duskLength && currentTime <= fullDayTimeLength)//fulldaytimelength is all parts added so 500
        {
            SetDayPart(DayPart.Night);
        }
    }
}
