 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_SliderController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sliderText = null;
    [SerializeField] private float maxSliderAmount = 100.0f;
    
    public void OnSliderChange(float val)
    {
        float localVal = val * maxSliderAmount;
        sliderText.text = localVal.ToString("0");
    }
}
