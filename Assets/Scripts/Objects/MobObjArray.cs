using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobObjArray : MonoBehaviour
{
    public static MobObjArray Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public Transform pfMob;

    public MobSO Wolf;
    public MobSO Bunny;
    public MobSO Turkey;
}
