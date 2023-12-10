using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeDimensionalObjectArray : MonoBehaviour
{
    public static ThreeDimensionalObjectArray Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public GameObject[] objList;

    public GameObject SearchObjList(string objType)
    {
        foreach (GameObject _obj in objList)
        {
            if (_obj.name == objType)
            {
                return _obj;
            }
        }
        Debug.Log(objList[0]);
        Debug.LogError($"object not found!! of type: {objType}");
        return null;
    }
}
