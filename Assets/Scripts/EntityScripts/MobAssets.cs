using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAssets : MonoBehaviour
{
    public static MobAssets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public GameObject[] assetList;

    public GameObject cgolem;

    public GameObject summonEffect;
}
