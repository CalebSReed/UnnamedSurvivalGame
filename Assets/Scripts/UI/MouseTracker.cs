using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseTracker : MonoBehaviour, IPointerEnterHandler
{
    public enum TrackingType
    {
        HorizontalBash,
        VerticalBash
    }
    private GestureManager gestureManager;
    public TrackingType type;
    public int order;
    public bool activated { get; private set; }

    private void Start()
    {
        order = transform.GetSiblingIndex();
        gestureManager = transform.parent.GetComponent<GestureManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gestureManager.activatedTrackersCount++;
        if (order == 0)
        {
            gestureManager.AddProgress();
            gestureManager.ResetAllTrackers();
        }
        activated = true;
        GetComponent<Image>().color = Color.green;
        switch (type)
        {
            case TrackingType.HorizontalBash:
                gestureManager.CompareHorizontalTrackers(order);
                break;
            case TrackingType.VerticalBash:
                break;
        }
    }

    public void Deactivate()
    {
        activated = false;
        GetComponent<Image>().color = Color.black;
    }
}
