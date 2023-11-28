using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSkeletonList : MonoBehaviour
{
    public static MobSkeletonList Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public List<GameObject> mobSkeletonList = new List<GameObject>();

    public GameObject FindSkeleton(string _mobType)
    {
        foreach (GameObject _skeleton in mobSkeletonList)
        {
            if (_mobType == _skeleton.name)
            {
                return _skeleton;
            }
        }
        Debug.LogError("Mob not found!");
        return null;
    }
}
