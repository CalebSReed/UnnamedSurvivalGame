using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChangeImageColor : MonoBehaviour
{
    [SerializeField] private Image image;

    public void ResetAllChildColors()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<ChangeImageColor>().ResetColor();
        }
    }

    public void ChangeColor(Color color)
    {
        image.color = color;
    }

    public void ChangeColor()
    {
        transform.parent.GetComponent<ChangeImageColor>().ResetAllChildColors();
        image.color = Color.red;
    }

    public void ResetColor()
    {
        image.color = Color.black;
    }
}
