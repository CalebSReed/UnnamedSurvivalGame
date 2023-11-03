using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WOPlacementManager : MonoBehaviour
{
    public WOSO Woso;
    public SpriteRenderer spr;

    private void Awake()
    {
        var realObj = RealWorldObject.SpawnWorldObject(transform.position, new WorldObject { woso = Woso });
        if (realObj.obj.woso.isHWall)
        {
            realObj.transform.position = new Vector3(realObj.transform.position.x, realObj.transform.position.y + 2, realObj.transform.position.z);
        }
        spr.sprite = null;
    }

    private void OnValidate()//holy this is a godsend funciton
    {
        spr.sprite = Woso.objSprite;
    }
}
