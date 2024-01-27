using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TanningRack : MonoBehaviour
{
    private RealWorldObject obj;
    private bool isFinished;
    private bool isTanning;
    private Item heldItem;
    private int progress;
    private int goal;
    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();

        obj.receiveEvent.AddListener(ReceiveItem);
        obj.interactEvent.AddListener(ReceiveItem);
        obj.hasSpecialInteraction = true;
    }

    private void ReceiveItem()
    {
        if (!isFinished && !isTanning && obj.playerMain.isHoldingItem)
        {
            var _item = obj.playerMain.heldItem;

            foreach (ItemSO item in obj.woso.itemAttachments)
            {
                if (item == _item.itemSO)
                {
                    heldItem = _item;
                    StartCoroutine(StartTanning(_item));
                    break;
                }
            }
            obj.playerMain.UseHeldItem();
        }
        else if (isFinished)
        {
            obj.inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("leather"), amount = 1 }, transform.position);
            obj.inventory.DropAllItems(transform.position);
            heldItem = null;
            isFinished = false;
            obj.storedItemRenderer.sprite = null;
        }
    }

    private IEnumerator StartTanning(Item item)
    {
        isTanning = true;
        obj.storedItemRenderer.sprite = item.itemSO.itemSprite;
        //pos.y -= item.itemSO.itemSprite.border.y;
        obj.storedItemRenderer.transform.localPosition = new Vector3(0,4,0);
        yield return new WaitForSeconds(60 * 5);
        obj.storedItemRenderer.sprite = WorldObject_Assets.Instance.tanningskin;
        obj.storedItemRenderer.transform.localPosition = new Vector3(0, 6, 0);
        isTanning = false;
        isFinished = true;
    }
}
