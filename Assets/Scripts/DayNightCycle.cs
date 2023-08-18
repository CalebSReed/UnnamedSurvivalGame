using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public float dayLength;
    public float duskLength;
    public float nightLength;
    public float dawnLength;
    public Light2D globalLight;
    public Gradient dayDuskGradient;
    public Gradient duskNightGradient;
    public Gradient nightDawnGradient;
    public Gradient dawnDayGradient;

    void Start()
    {
        StartCoroutine(DoDayNightCycle());
    }

    private IEnumerator DoDayNightCycle()
    {
        //globalLight.color = new Color(255, 195, 195);
        yield return new WaitForSecondsRealtime(dayLength);
        float i = .01f;
        while (i < 1)
        {
            globalLight.color = dayDuskGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        yield return new WaitForSecondsRealtime(duskLength);
        i = .01f;
        while (i < 1)
        {
            globalLight.color = duskNightGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        yield return new WaitForSecondsRealtime(nightLength);
        i = .01f;
        while (i < 1)
        {
            globalLight.color = nightDawnGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        yield return new WaitForSecondsRealtime(dawnLength);
        i = .01f;
        while (i < 1)
        {
            globalLight.color = dawnDayGradient.Evaluate(i);
            yield return new WaitForSeconds(.1f);
            i += .01f;
        }
        StartCoroutine(DoDayNightCycle());
    }
}
