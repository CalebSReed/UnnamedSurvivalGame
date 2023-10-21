using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CalebUtils
{
    public static Vector3 RandomPositionInRadius(Vector3 oldPos, int innerRadius, int outerRadius)
    {
        Vector3 newPos = oldPos;
        while (Vector3.Distance(newPos, oldPos) < innerRadius)
        {
            Vector3 randomPosition = Random.insideUnitSphere * outerRadius;
            randomPosition = new Vector3(randomPosition.x, randomPosition.y, newPos.z);
            newPos = randomPosition + oldPos;
        }
        return newPos;
    }

    public static Vector2 MoveAway(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        Vector3 a = target - current;
        float magnitude = a.magnitude;
        if (magnitude <= maxDistanceDelta || magnitude == 0f)
        {
            return target;
        }
        return current - a / magnitude * maxDistanceDelta;
    }

    public static List<GameObject> FindChildrenWithTag(Transform parent, string tag)
    {
        List<GameObject> _objectList = new List<GameObject>();
        foreach (Transform child in parent)
        {
            if (child.gameObject.CompareTag(tag))
            {
                _objectList.Add(child.gameObject);
            }
        }
        return _objectList;
    }
}
