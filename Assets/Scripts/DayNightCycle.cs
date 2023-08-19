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

    void Awake()
    {
        dayCount = 0;
        StartCoroutine(DoDayNightCycle());
    }

    private IEnumerator DoDayNightCycle()//loop forevah
    {
        //globalLight.color = new Color(255, 195, 195);
        OnDawn?.Invoke(this, EventArgs.Empty);//move to bottom probs
        globalLight.color = nightDawnGradient.Evaluate(1);
        float i;
        yield return new WaitForSeconds(dawnLength);
        OnDay?.Invoke(this, EventArgs.Empty);
        i = .01f;
        while (i < 1)
        {
            globalLight.color = dawnDayGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        yield return new WaitForSeconds(dayLength);
        OnDusk?.Invoke(this, EventArgs.Empty);
        i = .01f;
        while (i < 1)
        {
            globalLight.color = dayDuskGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        yield return new WaitForSeconds(duskLength);
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
}
