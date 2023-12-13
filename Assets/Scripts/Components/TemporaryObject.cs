using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryObject : MonoBehaviour
{
    RealWorldObject realObj;
    private void Awake()
    {
        realObj = GetComponent<RealWorldObject>();
        StartCoroutine(WaitToDespawn());
    }

    private IEnumerator WaitToDespawn()
    {
        yield return new WaitForSeconds(30);
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
