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

    public MobSO[] mobList;

    public MobSO SearchMobList(string _mobType)
    {
        foreach (MobSO _mob in mobList)
        {
            if (_mobType == _mob.mobType)
            {
                return _mob;
            }
        }
        return null;
    }

    public MobSO SearchMobListByName(string _mobName)
    {
        foreach(MobSO _mob in mobList)
        {
            if (_mobName == _mob.mobName)
            {
                return _mob;
            }
        }
        return null;
    }

    public Transform pfMob;

    /*public MobSO Wolf;
    public MobSO Bunny;
    public MobSO Turkey;
    public MobSO Sheep;
    public MobSO DepthWalker;*/
}
