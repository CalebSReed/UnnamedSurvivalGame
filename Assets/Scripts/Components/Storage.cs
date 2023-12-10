using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    RealWorldObject obj;
    Coroutine checkingPlayer;
    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.interactEvent.AddListener(OnInteracted);
        obj.hasSpecialInteraction = true;
    }

    private void OnInteracted()
    {
        if (Vector3.Distance(obj.playerMain.transform.position, transform.position) > obj.playerMain.collectRange)
        {
            return;
        }
        if (obj.playerMain.isHoldingItem)
        {
            StoreItem();
        }
        else
        {
            ToggleContainer();
        }
    }

    private void ToggleContainer()
    {
        if (obj.containerOpen)
        {
            CloseContainer();
        }
        else
        {
            OpenContainer();
        }
    }

    public void OpenContainer()
    {
        obj.containerOpen = true;
        obj.uiInv.SetInventory(obj.inventory, 3, obj);
        obj.uiInv.RefreshInventoryReference(obj.inventory);
        obj.uiInv.gameObject.SetActive(true);//set true after setInv func runs...
        obj.playerMain.SetContainerReference(obj);
        checkingPlayer = StartCoroutine(CheckToCloseContainer());
    }

    public void CloseContainer()
    {
        if (checkingPlayer != null)
        {
            StopCoroutine(checkingPlayer);
        }
        obj.containerOpen = false;
        obj.uiInv.gameObject.SetActive(false);
    }

    private IEnumerator CheckToCloseContainer()
    {
        if (Vector3.Distance(obj.playerMain.transform.position, transform.position) <= obj.playerMain.collectRange)
        {
            yield return null;
            StartCoroutine(CheckToCloseContainer());
        }
        else
        {
            CloseContainer();
        }

    }
    public void StoreItem()
    {
        Item tempItem = new Item() { amount = obj.playerMain.heldItem.amount, itemSO = obj.playerMain.heldItem.itemSO, equipType = obj.playerMain.heldItem.equipType, ammo = obj.playerMain.heldItem.ammo, uses = obj.playerMain.heldItem.uses };//must create new item, if we dont then both variables share same memory location and both values change at same time
        obj.inventory.AddItem(tempItem, obj.transform.position, false);
        obj.playerMain.heldItem = null;
        obj.playerMain.StopHoldingItem();

        if (!obj.IsContainerOpen())
        {
            OpenContainer();
        }
    }

    private void OnDestroy()
    {
        if (obj.containerOpen)
        {
            CloseContainer();
        }
        obj.interactEvent.RemoveListener(OnInteracted);
    }
}
