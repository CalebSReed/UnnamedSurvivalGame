using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

public class Storage : NetworkBehaviour
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

    private void Start()
    {
        obj.inventory.OnItemListChanged += UpdateInventoryForOthers;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        AskServerForStorageDataRPC();//fix later to target only specific client
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
        obj.uiInv.CloseContainers();
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

    [Rpc(SendTo.Server)]
    private void AskServerForStorageDataRPC()
    {
        UpdateInventoryForOthers(this, System.EventArgs.Empty);//fix later to only update specific client
    }

    public void UpdateInventoryForOthers(object sender, System.EventArgs e)
    {
        int[] newTypes = new int[obj.inventory.GetItemList().Length];
        int[] newAmounts = new int[newTypes.Length];
        int[] newUses = new int[newTypes.Length];
        int[] newAmmo = new int[newTypes.Length];
        int[] newStoredTypes1 = new int[newTypes.Length];//this is dumb but oh well!
        int[] newStoredTypes2 = new int[newTypes.Length];

        for (int i = 0; i < obj.inventory.GetItemList().Length; i++)
        {
            if (obj.inventory.GetItemList()[i] != null)
            {
                newTypes[i] = obj.inventory.GetItemList()[i].itemSO.itemID;
                newAmounts[i] = obj.inventory.GetItemList()[i].amount;
                newUses[i] = obj.inventory.GetItemList()[i].uses;
                newAmmo[i] = obj.inventory.GetItemList()[i].ammo;
                if (obj.inventory.GetItemList()[i].containedItems != null)
                {
                    if (obj.inventory.GetItemList()[i].containedItems.Length > 0 && obj.inventory.GetItemList()[i].containedItems[0] != null)
                    {
                        newStoredTypes1[i] = obj.inventory.GetItemList()[i].containedItems[0].itemSO.itemID;
                    }
                    else
                    {
                        newStoredTypes1[i] = -1;//-1 will be our null
                    }

                    if (obj.inventory.GetItemList()[i].containedItems.Length > 0 && obj.inventory.GetItemList()[i].containedItems[1] != null)
                    {
                        newStoredTypes2[i] = obj.inventory.GetItemList()[i].containedItems[1].itemSO.itemID;
                    }
                    else
                    {
                        newStoredTypes2[i] = -1;
                    }
                }
            }
            else
            {
                newTypes[i] = -1;
            }
        }

        UpdateInventoryForOthersRPC(newTypes, newAmounts, newUses, newAmmo, newStoredTypes1, newStoredTypes2);
    }

    [Rpc(SendTo.NotMe)]
    private void UpdateInventoryForOthersRPC(int[] itemTypes, int[] newAmounts, int[] newUses, int[] newAmmo, int[] containedTypes1, int[] containedTypes2)
    {
        Item[] newItemList = new Item[obj.inventory.GetItemList().Length];
        for (int i = 0; i < obj.inventory.GetItemList().Length; i++)
        {
            if (itemTypes[i] != -1)
            {
                newItemList[i] = new Item
                {
                    itemSO = ItemObjectArray.Instance.SearchItemList(itemTypes[i]),
                    amount = newAmounts[i],
                    uses = newUses[i],
                    ammo = newAmmo[i],
                    equipType = ItemObjectArray.Instance.SearchItemList(itemTypes[i]).equipType,
                    containedItems = new Item[ItemObjectArray.Instance.SearchItemList(itemTypes[i]).maxStorageSpace]
                };
                if (newItemList[i].itemSO.canStoreItems && containedTypes1[i] != -1)
                {
                    newItemList[i].containedItems[0] = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(containedTypes1[i]), amount = 1 };
                }
                if (newItemList[i].itemSO.canStoreItems && containedTypes1[i] != -1)
                {
                    newItemList[i].containedItems[1] = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(containedTypes2[i]), amount = 1 };
                }
            }
            else
            {
                newItemList[i] = null;
            }
        }
        obj.inventory.SetItemList(newItemList);
        obj.uiInv.RefreshInventoryItems();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

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

                if (item.itemSO.canStoreItems)
                {
                    string[] containedTypes = new string[item.containedItems.Length];
                    for (int j = 0; j < item.containedItems.Length; j++)
                    {
                        if (item.containedItems[j] != null)
                        {
                            containedTypes[j] = item.containedItems[j].itemSO.itemType;
                        }
                    }
                    containedTypes.Reverse();
                    obj.saveData.containedTypes = containedTypes;
                }
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
                    uses = obj.saveData.invItemUses[i],
                    equipType = ItemObjectArray.Instance.SearchItemList(obj.saveData.invItemTypes[i]).equipType
                };

                Item item = obj.inventory.GetItemList()[i];
                if (item.itemSO.canStoreItems)
                {
                    item.containedItems = new Item[item.itemSO.maxStorageSpace];
                }

                if (obj.saveData.containedTypes != null)
                {
                    Item[] containedTypes = new Item[obj.saveData.containedTypes.Length];
                    for (int j = 0; j < obj.saveData.containedTypes.Length; j++)
                    {
                        if (obj.saveData.containedTypes[j] != null)
                        {
                            containedTypes[j] = new Item
                            {
                                itemSO = ItemObjectArray.Instance.SearchItemList(obj.saveData.containedTypes[j]),
                                amount = 1
                            };
                        }
                    }
                    item.containedItems = containedTypes;
                }
            }
        }
    }
}
