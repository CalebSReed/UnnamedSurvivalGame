using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryBushBehavior : MonoBehaviour
{
    private RealWorldObject obj;
    [SerializeField] private int progress;
    [SerializeField] private int goal = DayNightCycle.fullDayTimeLength / 2;

    void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.onLoaded += OnLoad;
        StartCoroutine(GrowBack());
    }

    private IEnumerator GrowBack()
    {
        yield return new WaitForSeconds(1);
        progress++;
        obj.saveData.timerProgress = progress;
        if (progress >= goal)
        {
            obj.saveData.timerProgress = goal;
            FinishGrowing();
            yield break;
        }
        StartCoroutine(GrowBack());
    }

    private void FinishGrowing()
    {
        RealWorldObject.SpawnWorldObject(transform.position, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Elderberry Bush") });
        obj.Break(true);
    }

    private void OnLoad(object sender, System.EventArgs e)
    {
        if (obj.saveData.timerProgress > 0 && obj.saveData.timerProgress < goal)
        {
            progress = obj.saveData.timerProgress;
            StartCoroutine(GrowBack());
        }
        else if (obj.saveData.timerProgress >= goal)
        {
            FinishGrowing();
        }
    }
}
