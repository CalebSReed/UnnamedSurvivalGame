using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolingRackBehavior : MonoBehaviour
{
    RealWorldObject obj;
    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.hasSpecialInteraction = true;
        obj.interactEvent.AddListener(ReceiveItem);
    }

    private void ReceiveItem()
    {
        if (obj.playerMain.hasTongs && obj.playerMain.equippedHandItem.containedItem != null && obj.playerMain.equippedHandItem.containedItem.isHot)
        {
            StartCoroutine(CoolItem());
        }
    }

    private IEnumerator CoolItem()
    {
        Item _item = obj.playerMain.equippedHandItem.containedItem;
        var _time = obj.playerMain.equippedHandItem.containedItem.remainingTime;
        obj.playerMain.equippedHandItem.containedItem = null;
        obj.playerMain.RemoveContainedItem();
        yield return new WaitForSeconds(_time/2);
        var _realItem = RealItem.SpawnRealItem(transform.position, _item, true, false, 0, false, true, true);
        _item.StopBeingHot();
        CalebUtils.RandomDirForceNoYAxis3D(_realItem.GetComponent<Rigidbody>(), 5);
    }
}
