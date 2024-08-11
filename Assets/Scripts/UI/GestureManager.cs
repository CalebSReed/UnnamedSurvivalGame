using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestureManager : MonoBehaviour
{
    public List<MouseTracker> trackers;
    public int currentTracker = 0;
    public int activatedTrackersCount;
    public float currentPower;
    public float currentProgress;
    public float goal;

    private void Start()
    {
        trackers = new List<MouseTracker>();
        for (int i = 0; i < transform.childCount; i++)
        {
            trackers.Add(transform.GetChild(i).GetComponent<MouseTracker>());
        }
    }

    public void CompareHorizontalTrackers(int newestTracker)
    {
        if (currentTracker < newestTracker)
        {
            currentPower += 1;
        }
        else
        {
            StartCoroutine(LosePower());
        }
        currentTracker = newestTracker;
    }

    private IEnumerator LosePower()
    {
        while (currentPower > 0)
        {
            yield return null;
            currentPower -= .1f;
        }
        currentPower = 0;
        ResetAllTrackers();
    }

    public void AddProgress()
    {
        currentProgress += currentPower;
        if (currentProgress > goal)
        {
            transform.parent.GetComponent<Image>().color = Color.green;
            currentProgress = 0;
        }
    }

    public void ResetAllTrackers()
    {
        StopAllCoroutines();
        currentPower = 0;
        activatedTrackersCount = 0;
        foreach (var tracker in trackers)
        {
            tracker.Deactivate();
        }
    }
}
