using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobPfFinder : MonoBehaviour
{
    public static MobPfFinder Instance { get; private set; }
    public List<Transform> mobList = new List<Transform>();

    private void Awake()
    {
        Instance = this;
    }

    public Transform FindMobPf(string mobName)
    {
        foreach (var mob in mobList)
        {
            if (mob.name == mobName)
            {
                return mob;
            }
        }
        Debug.LogError($"Wrong string entered: {mobName}");
        return null;
    }
}
