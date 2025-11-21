using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WosoArray : MonoBehaviour
{
    public static WosoArray Instance { get; set; }
    private void Awake()
    {
        Instance = this;
    }

    private void OnValidate()
    {
        //SetNewIDs();
    }
#if UNITY_EDITOR
    //[MenuItem("ManageGameAssets/Set New IDs for Asset Type/WorldObjects")]
    public void SetNewIDs()
    {
        int i = 0;
        foreach (WOSO SO in wosoList)
        {
            if (i == 0)
            {
                i++;
                continue;
            }
            else if (SO.objID == 0)
            {
                Undo.RecordObject(SO, "Set Object ID");
                SO.objID = i;
                EditorUtility.SetDirty(SO);
                Debug.Log($"SET {SO.objType} TO ID: {SO.objID}");
            }
            i++;
        }
    }
#endif
    public WOSO[] wosoList;

    public WOSO SearchWOSOList(string _wosoType)
    {
        foreach (WOSO _woso in wosoList)
        {
            if (_wosoType == _woso.objType)
            {
                return _woso;
            }
        }
        Debug.LogError($"No WOSO type found! wosoType: {_wosoType}");
        return null;
    }

    public WOSO SearchWOSOList(int _wosoType)
    {
        foreach (WOSO _woso in wosoList)
        {
            if (_wosoType == _woso.objID)
            {
                return _woso;
            }
        }
        Debug.LogError($"No WOSO type found! wosoType: {_wosoType}");
        return null;
    }

    public Transform pfWorldObject;

    /*public WOSO Tree;
    public WOSO Boulder;
    public WOSO Kiln;
    public WOSO Campfire;
    public WOSO HotCoals;
    public WOSO ClayDeposit;
    public WOSO Sapling;
    public WOSO Milkweed;
    public WOSO WildParsnip;
    public WOSO WildCarrot;
    public WOSO BrownShroom;
    public WOSO BunnyHole;
    public WOSO MagicalTree;
    public WOSO Pond;
    public WOSO BirchTree;
    public WOSO Wheat;
    public WOSO CypressTree;
    public WOSO Oven;
    public WOSO GoldBoulder;
    public WOSO DirtBeacon;
    public WOSO FungTree;*/
}
