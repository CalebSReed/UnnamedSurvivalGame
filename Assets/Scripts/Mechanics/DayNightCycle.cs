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
        Spring,
        Summer,
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
    public static readonly int fullYearLength = 40;//40 days. 10 of spring, summer, autumn, and winter.
    public static readonly int seasonLength = 10;//every season lasts the same amount of time
    public int currentYear;//save
    public int currentDayOfYear;//save
    public int currentDay;//save
    public int currentTime;//save
    public int currentSeasonProgress;
    public DayPart dayPart;
    public Season currentSeason;


    //public int percentageDawnLength;
    //public int percentageDayLength;
    //public int percentageDuskLength;
    //public int percentageNightLength;

    public float dawnLength;//set as percentages.
    public float dayLength;
    public float duskLength;
    public float nightLength;

    public Light2D globalLight;

    public Gradient nightDawnGradient;
    public Gradient dawnDayGradient;
    public Gradient dayDuskGradient;
    public Gradient duskNightGradient;

    public Gradient springNightDawnGradient;
    public Gradient springDawnDayGradient;
    public Gradient springDayDuskGradient;
    public Gradient springDuskNightGradient;

    public Gradient summerNightDawnGradient;
    public Gradient summerDawnDayGradient;
    public Gradient summerDayDuskGradient;
    public Gradient summerDuskNightGradient;

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
    private bool isLoading;

    void Awake()
    {
        currentDay = 1;
        currentDayOfYear = 1;
        currentYear = 1;
        currentSeasonProgress = 1;
        dayPart = DayPart.Night;
        currentSeason = Season.Spring;
        OnSpring?.Invoke(this, EventArgs.Empty);
        Instance = this;        
        StartCoroutine(DoDayProgress());
    }

    private IEnumerator DoDayProgress()//EXAMPLE: dawn = 100, day = 200, dusk = 100, night = 100, fullday = 500
    {
        if (currentTime <= dawnLength && dayPart != DayPart.Dawn)//0 to 100
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

        yield return new WaitForSeconds(1);
        currentTime++;

        if (isLoading)
        {
            isLoading = false;
        }

        StartCoroutine(DoDayProgress());
    }

    private void SetDayPart(DayPart _part)
    {
        dayPart = _part;
        switch (_part)
        {
            case DayPart.Dawn:
                switch (currentSeason)
                {
                    case Season.Spring:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, springNightDawnGradient));
                        break;
                    case Season.Summer:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, summerNightDawnGradient));
                        break;
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
                    case Season.Spring:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, springDawnDayGradient));
                        break;
                    case Season.Summer:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, summerDawnDayGradient));
                        break;
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
                {
                    case Season.Spring:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, springDayDuskGradient));
                        break;
                    case Season.Summer:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, summerDayDuskGradient));
                        break;
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
                {
                    case Season.Spring:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, springDuskNightGradient));
                        break;
                    case Season.Summer:
                        lastTransition = StartCoroutine(DoDayPartTransition(_part, summerDuskNightGradient));
                        break;
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
        if (isLoading)
        {
            globalLight.color = gradient.Evaluate(1);
            isLoading = false;
        }
        float i = .01f;
        while (i < 1)
        {
            globalLight.color = gradient.Evaluate(i);
            yield return new WaitForSeconds(.5f);
            i += .01f;
        }
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

    private void CheckTimeOfYear()
    {
        //if (currentDay <= seasonLength)  currentDayInYear, reset on new year

        if (currentDay >= 5)
        {
            JournalNoteController.Instance.UnlockSpecificEntry("Day5");
        }

        if (currentDayOfYear > fullYearLength)
        {
            currentDayOfYear = 1;
            currentYear++;
        }

        if (currentSeasonProgress > 10)
        {
            switch (currentSeason)//season transition
            {
                default:
                    break;
                case Season.Spring: currentSeason = Season.Summer;
                    OnSummer?.Invoke(this, EventArgs.Empty);
                    break;
                case Season.Summer: currentSeason = Season.Autumn;
                    OnAutumn?.Invoke(this, EventArgs.Empty);
                    break;
                case Season.Autumn: currentSeason = Season.Winter;
                    OnWinter?.Invoke(this, EventArgs.Empty);
                    break;
                case Season.Winter: currentSeason = Season.Spring;
                    OnSpring?.Invoke(this, EventArgs.Empty);
                    break;
            }
            currentSeasonProgress = 1;
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

    public void LoadNewTime(int _currentTime, int _currentDay, int _currentDayOfYear, int _currentYear, int _currentSeason, int _seasonProg)
    {
        currentTime = _currentTime;
        currentDay = _currentDay;
        currentDayOfYear = _currentDayOfYear;
        currentYear = _currentYear;
        currentSeason = (Season)_currentSeason;
        currentSeasonProgress = _seasonProg;
        isLoading = true;
    }
}
