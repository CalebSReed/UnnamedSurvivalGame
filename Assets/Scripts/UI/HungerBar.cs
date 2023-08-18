using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HungerBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxHunger(int _hunger)
    {
        slider.maxValue = _hunger;
        slider.value = _hunger;
    }

    public void SetHunger(int _hunger)
    {
        slider.value = _hunger;
    }
}
