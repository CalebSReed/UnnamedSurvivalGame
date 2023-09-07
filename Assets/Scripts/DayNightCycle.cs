using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.EventSystems;

public class DayNightCycle : MonoBehaviour
{
    public int dayCount;

    public float dayLength;
    public float duskLength;
    public float nightLength;
    public float dawnLength;
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

    void Awake()
    {
        dayCount = 0;
        StartCoroutine(DoDayNightCycle());
    }

    private IEnumerator DoDayNightCycle()//loop forevah
    {
        //globalLight.color = new Color(255, 195, 195);
        ResetBools("dawn");
        OnDawn?.Invoke(this, EventArgs.Empty);//move to bottom probs
        globalLight.color = nightDawnGradient.Evaluate(1);
        float i;
        yield return new WaitForSeconds(dawnLength);
        ResetBools("day");
        OnDay?.Invoke(this, EventArgs.Empty);
        i = .01f;
        while (i < 1)
        {
            globalLight.color = dawnDayGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        yield return new WaitForSeconds(dayLength);
        ResetBools("dusk");
        OnDusk?.Invoke(this, EventArgs.Empty);
        i = .01f;
        while (i < 1)
        {
            globalLight.color = dayDuskGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        yield return new WaitForSeconds(duskLength);
        ResetBools("night");
        OnNight?.Invoke(this, EventArgs.Empty);
        i = .01f;
        while (i < 1)
        {
            globalLight.color = duskNightGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        yield return new WaitForSeconds(nightLength);
        dayCount++;//we can use this to ramp up difficulty
        //
        i = .01f;
        while (i < 1)
        {
            globalLight.color = nightDawnGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        StartCoroutine(DoDayNightCycle());
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
