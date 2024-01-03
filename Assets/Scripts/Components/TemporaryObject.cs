using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryObject : MonoBehaviour
{
    RealWorldObject realObj;
    public float Timer = 30;
    private void Awake()
    {
        realObj = GetComponent<RealWorldObject>();
        Timer = realObj.woso.lifeTime;
        StartCoroutine(WaitToDespawn());
    }

    private IEnumerator WaitToDespawn()
    {
        yield return new WaitForSeconds(Timer);
        realObj.Break(true);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnEnable()
    {
        StartCoroutine(WaitToDespawn());
    }
}
