using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientHelper : NetworkBehaviour
{
    public static ClientHelper Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [Rpc(SendTo.Server)]//specify everything!
    public void AskToSpawnItemSpecificRPC(Vector3 position, bool pickUpCooldown, bool magnetized, string itemType, int amount, int uses, int ammo, int equipType, bool isHot, float timeRemaining = 0, int[] containedItemTypes = null, int[] containedItemAmounts = null, string heldItemType = null)
    {
        Item newItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(itemType), amount = amount, uses = uses, ammo = ammo, equipType = (Item.EquipType)equipType, isHot = isHot, remainingTime = timeRemaining };

        if (containedItemTypes != null)
        {
            Item[] newContainedItemsList = new Item[containedItemTypes.Length];
            for (int i = 0; i < containedItemTypes.Length - 1; i++)
            {
                newContainedItemsList[i] = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(containedItemTypes[i]), amount = containedItemAmounts[i] };
            }
            newItem.containedItems = newContainedItemsList;
        }

        if (heldItemType != null)
        {
            newItem.heldItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(heldItemType), amount = 1 };
        }

        var realItem = RealItem.SpawnRealItem(position, newItem, true, true, newItem.ammo, newItem.isHot, pickUpCooldown, magnetized);

        if (timeRemaining > 0)
        {
            realItem.StartCoroutine(newItem.RemainHot(timeRemaining));
        }

        if (magnetized)
        {
            CalebUtils.RandomDirForceNoYAxis3D(realItem.GetComponent<Rigidbody>(), 5);
        }
    }

    [Rpc(SendTo.Server)]//Assume most values, like uses, 
    public void AskToSpawnItemBasicRPC(Vector3 position, string itemType, bool magnetized)
    {
        ItemSO newSO = ItemObjectArray.Instance.SearchItemList(itemType);
        Item newItem = new Item { itemSO = newSO, ammo = newSO.maxAmmo, amount = 1, equipType = newSO.equipType, uses = newSO.maxUses };
        var realItem = RealItem.SpawnRealItem(position, newItem, true, false, newItem.ammo, false, magnetized, magnetized);
        if (magnetized)
        {
            CalebUtils.RandomDirForceNoYAxis3D(realItem.GetComponent<Rigidbody>(), 5);//assuming that this item is gonna be magnetized.
        }
    }

    [Rpc(SendTo.NotServer)]
    public void AnnounceToOtherClientsRPC(string text, Color _color)
    {
        Announcer.SetText(text, _color);
    }

    [Rpc(SendTo.NotServer)]
    public void TogglePvpRPC(bool pvp)
    {
        GameManager.Instance.pvpEnabled = pvp;
    }
}
