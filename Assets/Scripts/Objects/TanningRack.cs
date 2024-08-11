using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TanningRack : MonoBehaviour
{
    private RealWorldObject obj;
    private bool isFinished;
    private bool isTanning;
    private Item heldItem;
    [SerializeField] private int progress;
    [SerializeField] private int goal = 60 * 5;
    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();

        obj.receiveEvent.AddListener(ReceiveItem);
        obj.interactEvent.AddListener(OnInteract);
        obj.hasSpecialInteraction = true;

        obj.hoverBehavior.SpecialCase = true;
        obj.hoverBehavior.specialCaseModifier.AddListener(CheckItems);

        obj.onLoaded += OnLoad;
    }

    private void CheckItems()
    {
        if (!isFinished && !isTanning && obj.playerMain.isHoldingItem)
        {
            var _item = obj.playerMain.heldItem;

            foreach (ItemSO item in obj.woso.itemAttachments)
            {
                if (item == _item.itemSO)
                {
                    obj.hoverBehavior.Prefix = $"LMB: Dry {_item.itemSO.itemName}";
                    obj.hoverBehavior.Name = "";
                    break;
                }
            }
        }
        else if (isFinished)
        {
            obj.hoverBehavior.Prefix = $"RMB: Collect {heldItem}";
            obj.hoverBehavior.Name = "";
        }
        else
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
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
                    obj.saveData.heldItemType = _item.itemSO.itemType;
                    StartCoroutine(StartTanning(_item));
                    break;
                }
            }
            obj.playerMain.UseHeldItem();
        }
    }

    private void OnInteract()
    {
        if (isFinished)
        {
            obj.inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("leather"), amount = 1 }, transform.position);
            obj.inventory.DropAllItems(transform.position);
            heldItem = null;
            isFinished = false;
            obj.storedItemRenderer.sprite = null;
            progress = 0;
            obj.saveData.heldItemType = null;
            obj.saveData.timerProgress = 0;
        }
    }

    private IEnumerator StartTanning(Item item)
    {
        isTanning = true;
        obj.storedItemRenderer.sprite = item.itemSO.itemSprite;
        //pos.y -= item.itemSO.itemSprite.border.y;
        obj.storedItemRenderer.transform.localPosition = new Vector3(0,4,0);
        progress++;
        obj.saveData.timerProgress = progress;
        yield return new WaitForSeconds(1);

        if (progress >= goal)
        {
            obj.saveData.timerProgress = goal;
            FinishTanning();
        }
        else
        {
            StartCoroutine(StartTanning(item));
        }
    }
    private void FinishTanning()
    {
        obj.storedItemRenderer.sprite = WorldObject_Assets.Instance.tanningskin;
        obj.storedItemRenderer.transform.localPosition = new Vector3(0, 6, 0);
        isTanning = false;
        isFinished = true;
    }

    private void OnLoad(object sender, System.EventArgs e)
    {
        if (obj.saveData.heldItemType != null)
        {
            heldItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(obj.saveData.heldItemType) };
        }

        if (obj.saveData.timerProgress > 0 && obj.saveData.timerProgress < goal)
        {
            progress = obj.saveData.timerProgress;
            StartCoroutine(StartTanning(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(obj.saveData.heldItemType)}));
        }
        else if (obj.saveData.timerProgress >= goal)
        {
            FinishTanning();
        }
    }
}
