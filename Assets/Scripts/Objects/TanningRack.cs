using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TanningRack : NetworkBehaviour
{
    private RealWorldObject obj;
    private bool isFinished;
    private bool isTanning;
    private Item heldItem;
    [SerializeField] private float progress;
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

    private void Update()
    {
        if (isTanning)
        {
            progress += Time.deltaTime;
            obj.saveData.timerProgress = progress;

            if (progress >= goal)
            {
                obj.saveData.timerProgress = goal;
                FinishTanning();
            }
        }
    }

    private void StartTanning()
    {
        isTanning = true;
        obj.storedItemRenderer.sprite = heldItem.itemSO.itemSprite;
        obj.storedItemRenderer.transform.localPosition = new Vector3(0, 4, 0);
    }

    [Rpc(SendTo.Server)]
    private void AskToTanItemRPC(string itemType)
    {
        heldItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(itemType) };
        StartTanning();
        UpdateTanningDataRPC(heldItem.itemSO.itemType);
    }

    [Rpc(SendTo.NotMe)]
    private void UpdateTanningDataRPC(string itemType)
    {
        if (itemType != null)
        {
            isTanning = true;
            Item newItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(itemType) };
            obj.storedItemRenderer.sprite = newItem.itemSO.itemSprite;
            obj.storedItemRenderer.transform.localPosition = new Vector3(0, 4, 0);
            heldItem = newItem;
            StartTanning();
        }
        else
        {
            isTanning = false;
            obj.storedItemRenderer.sprite = null;
            heldItem = null;
        }
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
                    if (GameManager.Instance.isServer)
                    {
                        StartTanning();
                        UpdateTanningDataRPC(heldItem.itemSO.itemType);
                    }
                    else
                    {
                        AskToTanItemRPC(heldItem.itemSO.itemType);
                    }
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
            UpdateTanningDataRPC(null);
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
            StartTanning();
        }
        else if (obj.saveData.timerProgress >= goal)
        {
            FinishTanning();
        }
    }
}
