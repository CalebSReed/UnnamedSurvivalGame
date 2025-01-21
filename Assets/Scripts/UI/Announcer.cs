using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Announcer : MonoBehaviour
{
    public static TextMeshProUGUI tmPro;
    private bool isTextLocked = false;

    public static Announcer instance { get; private set; }
    private void Awake()
    {
        tmPro = GetComponent<TextMeshProUGUI>();
        tmPro.text = "";
        instance = this;
    }

    public static void SetText(string _announcement, Color? _color = null, bool lockText = false, bool announceToOtherPlayers = false)
    {
        if (!instance.isTextLocked)
        {
            if (lockText)
            {
                instance.isTextLocked = true;

                instance.StopAllCoroutines();
                tmPro.faceColor = new Color(tmPro.faceColor.r, tmPro.faceColor.g, tmPro.faceColor.b, 1f);
                tmPro.outlineColor = new Color(tmPro.outlineColor.r, tmPro.outlineColor.g, tmPro.outlineColor.b, 1f);
                tmPro.text = _announcement;
            }
            else
            {
                instance.StopAllCoroutines();
                tmPro.faceColor = new Color(tmPro.faceColor.r, tmPro.faceColor.g, tmPro.faceColor.b, 1f);
                tmPro.outlineColor = new Color(tmPro.outlineColor.r, tmPro.outlineColor.g, tmPro.outlineColor.b, 1f);
                tmPro.text = _announcement;
                instance.StartCoroutine(FadeText(_color));
            }

            if (announceToOtherPlayers)
            {
                if (_color == null)
                {
                    ClientHelper.Instance.AnnounceToOtherClientsRPC(_announcement, Color.white);
                }
                else
                {
                    ClientHelper.Instance.AnnounceToOtherClientsRPC(_announcement, Color.white);
                }
            }
        }
    }

    public static void SetText(string _announcement, Color color, bool lockText = false, bool announceToOtherPlayers = false)
    {
        if (!instance.isTextLocked)
        {
            if (lockText)
            {
                instance.isTextLocked = true;

                instance.StopAllCoroutines();
                tmPro.faceColor = new Color(tmPro.faceColor.r, tmPro.faceColor.g, tmPro.faceColor.b, 1f);
                tmPro.outlineColor = new Color(tmPro.outlineColor.r, tmPro.outlineColor.g, tmPro.outlineColor.b, 1f);
                tmPro.text = _announcement;
            }
            else
            {
                instance.StopAllCoroutines();
                tmPro.faceColor = new Color(tmPro.faceColor.r, tmPro.faceColor.g, tmPro.faceColor.b, 1f);
                tmPro.outlineColor = new Color(tmPro.outlineColor.r, tmPro.outlineColor.g, tmPro.outlineColor.b, 1f);
                tmPro.text = _announcement;
                instance.StartCoroutine(FadeText(color));
            }

            if (announceToOtherPlayers)
            {
                ClientHelper.Instance.AnnounceToOtherClientsRPC(_announcement, color);
            }
        }
    }

    public static void RemoveLock()
    {
        tmPro.faceColor = new Color(tmPro.faceColor.r, tmPro.faceColor.g, tmPro.faceColor.b, 0f);
        tmPro.outlineColor = new Color(tmPro.outlineColor.r, tmPro.outlineColor.g, tmPro.outlineColor.b, 0f);
        instance.isTextLocked = false;
    }

    public static IEnumerator FadeText(Color? _color = null)
    {
        tmPro.faceColor = _color ?? Color.white;// ?? means if null, make it white.
        
        yield return new WaitForSeconds(3f);
        int i = 255;
        while (i > 0)
        {
            tmPro.faceColor = new Color32(tmPro.faceColor.r, tmPro.faceColor.g, tmPro.faceColor.b, (byte)i);
            tmPro.outlineColor = new Color32(tmPro.outlineColor.r, tmPro.outlineColor.g, tmPro.outlineColor.b, (byte)i);
            yield return new WaitForSeconds(.1f);
            i -= 10;
        }
        tmPro.faceColor = new Color(tmPro.faceColor.r, tmPro.faceColor.g, tmPro.faceColor.b, 0f);
        tmPro.outlineColor = new Color(tmPro.outlineColor.r, tmPro.outlineColor.g, tmPro.outlineColor.b, 0f);

    }
}
