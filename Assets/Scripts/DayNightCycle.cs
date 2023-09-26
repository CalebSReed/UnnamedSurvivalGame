using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.EventSystems;

public class DayNightCycle : MonoBehaviour
{
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
        BlueMoon
    }

    public static readonly int fullDayTimeLength = 1440;//24 min instead of 24 hours. 24 min multiplied by 60 seconds for each minute: 
    public static readonly int fullYearLength = 40;//40 days. 10 of spring, summer, autumn, and winter.
    public static readonly int seasonLength = 10;//every season lasts the same amount of time
    public int currentYear;//save
    public int currentDay;//save
    public int currentTime;//save
    public DayPart dayPart;


    //public int percentageDawnLength;
    //public int percentageDayLength;
    //public int percentageDuskLength;
    //public int percentageNightLength;

    public float dawnLength;//set as percentages.
    public float dayLength;
    public float duskLength;
    public float nightLength;

    public Light2D globalLight;
    public Gradient dayDuskGradient;
    public Gradient duskNightGradient;
    public Gradient nightDawnGradient;
    public Gradient dawnDayGradient;

    public event EventHandler OnDawn;
    public event EventHandler OnDay;
    public event EventHandler OnDusk;
    public event EventHandler OnNight;

    public bool isDawn;
    public bool isDay;
    public bool isDusk;
    public bool isNight;

    private Coroutine lastTransition;

    void Awake()
    {
        currentDay = 1;
        currentYear = 1;
        dayPart = DayPart.Night;
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
            lastTransition = StartCoroutine(SetDayPart(DayPart.Dawn));
        }
        else if (currentTime > dawnLength && currentTime <= dawnLength + dayLength && dayPart != DayPart.Day)//if more than 100, and less than 200 + 100 = 300. more than 100 less than or equal to 300
        {
            if (lastTransition != null)
            {
                StopCoroutine(lastTransition);
            }
            lastTransition = StartCoroutine(SetDayPart(DayPart.Day));
        }
        else if (currentTime > dawnLength + dayLength && currentTime <= dawnLength + dayLength + duskLength && dayPart != DayPart.Dusk)//if more than 300 (100 + 200), and less than or = 400 (100 + 200 + 100)
        {
            if (lastTransition != null)
            {
                StopCoroutine(lastTransition);
            }
            lastTransition = StartCoroutine(SetDayPart(DayPart.Dusk));
        }
        else if (currentTime > dawnLength + dayLength + duskLength && currentTime <= fullDayTimeLength && dayPart != DayPart.Night)//fulldaytimelength is all parts added so 500
        {
            if (lastTransition != null)
            {
                StopCoroutine(lastTransition);
            }
            lastTransition = StartCoroutine(SetDayPart(DayPart.Night));
        }
        else if (currentTime > fullDayTimeLength)
        {
            currentTime = 0;
            currentDay++;
            CheckTimeOfYear();//reset currentTime, up daycount and yearcount if needed, change season if needed, change daypart lengths if needed
        }

        yield return new WaitForSeconds(1);
        currentTime++;
        StartCoroutine(DoDayProgress());
    }

    private IEnumerator SetDayPart(DayPart _part)
    {
        dayPart = _part;
        float i = 0;
        switch (_part)
        {
            default: break;
            case DayPart.Dawn:
                ResetBools("dawn");
                OnDawn?.Invoke(this, EventArgs.Empty);
                i = .01f;
                while (i < 1)
                {
                    globalLight.color = nightDawnGradient.Evaluate(i);
                    yield return new WaitForSeconds(.1f);
                    i += .01f;
                }
                break;

            case DayPart.Day:
                ResetBools("day");
                OnDay?.Invoke(this, EventArgs.Empty);
                i = .01f;
                while (i < 1)
                {
                    globalLight.color = dawnDayGradient.Evaluate(i);
                    yield return new WaitForSeconds(.1f);
                    i += .01f;
                }
                break;

            case DayPart.Dusk:
                ResetBools("dusk");
                OnDusk?.Invoke(this, EventArgs.Empty);
                i = .01f;
                while (i < 1)
                {
                    globalLight.color = dayDuskGradient.Evaluate(i);
                    yield return new WaitForSeconds(.1f);
                    i += .01f;
                }
                break;

            case DayPart.Night:
                ResetBools("night");
                OnNight?.Invoke(this, EventArgs.Empty);
                i = .01f;
                while (i < 1)
                {
                    globalLight.color = duskNightGradient.Evaluate(i);
                    yield return new WaitForSeconds(.1f);
                    i += .01f;
                }
                break;
        }
    }

    //globalLight.color = nightDawnGradient.Evaluate(1);

    private void CheckTimeOfYear()
    {
        //if (currentDay <= seasonLength)  currentDayInYear, reset on new year

        if (currentDay > fullYearLength)
        {
            currentYear++;
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
}
