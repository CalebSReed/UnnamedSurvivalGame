using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectPfFinder : MonoBehaviour
{
    public static WorldObjectPfFinder Instance { get; private set; }
    public List<Transform> objList = new List<Transform>();

    private void Awake()
    {
        Instance = this;
    }

    public Transform FindObjPf(string objName)
    {
        foreach (var obj in objList)
        {
            if (obj.name == objName)
            {
                return obj;
            }
        }
        Debug.LogError($"Wrong string entered: {objName}");
        return null;
    }
}
