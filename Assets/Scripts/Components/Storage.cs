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
        obj.receiveEvent.AddListener(OnInteracted);
        obj.hasSpecialInteraction = true;

        obj.hoverBehavior.SpecialCase = true;
        obj.hoverBehavior.specialCaseModifier.AddListener(CheckState);

        obj.onSaved += OnSave;
        obj.onLoaded += OnLoad;
    }

    private void CheckState()
    {
        if (obj.playerMain.isHoldingItem)
        {
            obj.hoverBehavior.Prefix = $"LMB: Store {obj.playerMain.heldItem}";
            obj.hoverBehavior.Name = "";
        }
        else if (obj.containerOpen)
        {
            obj.hoverBehavior.Prefix = $"RMB: Close ";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
        else
        {
            obj.hoverBehavior.Prefix = $"RMB: Open ";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
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
        else if (obj.playerMain.chestObj == obj)//this is dumb just check if still open or properly stop this coroutine, ALSO setting chest obj reference is USELESS!
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

    private void OnSave(object sender, System.EventArgs e)
    {
        obj.saveData.invItemTypes.Clear();
        obj.saveData.invItemAmounts.Clear();
        obj.saveData.invItemAmmos.Clear();
        obj.saveData.invItemUses.Clear();

        int i = 0;
        foreach (var item in obj.inventory.GetItemList())
        {
            if (item != null)
            {
                obj.saveData.invItemTypes.Add(obj.inventory.GetItemList()[i].itemSO.itemType);
                obj.saveData.invItemAmounts.Add(obj.inventory.GetItemList()[i].amount);
                obj.saveData.invItemUses.Add(obj.inventory.GetItemList()[i].uses);
                obj.saveData.invItemAmmos.Add(obj.inventory.GetItemList()[i].ammo);
            }
            else
            {
                obj.saveData.invItemTypes.Add(null);
                obj.saveData.invItemAmounts.Add(0);
                obj.saveData.invItemUses.Add(0);
                obj.saveData.invItemAmmos.Add(0);
            }
            i++;
        }
    }
    
    private void OnLoad(object sender, System.EventArgs e)
    {
        for (int i = 0; i < obj.saveData.invItemTypes.Count; i++)
        {
            if (obj.saveData.invItemTypes[i] != null)
            {
                obj.inventory.GetItemList()[i] = new Item
                {
                    itemSO = ItemObjectArray.Instance.SearchItemList(obj.saveData.invItemTypes[i]),
                    ammo = obj.saveData.invItemAmmos[i],
                    amount = obj.saveData.invItemAmounts[i],
                    uses = obj.saveData.invItemUses[i]
                };
            }
        }
    }
}
