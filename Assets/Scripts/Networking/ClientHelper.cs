using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientHelper : NetworkBehaviour
{
    public static ClientHelper Instance { get; private set; }
    public NetworkManager networkManager;
    private void Awake()
    {
        Instance = this;
        //networkManager.OnClientDisconnectCallback += OnPlayerDisconnectedFromServer;
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

    public void OnPlayerDisconnectedFromServer(ulong obj)
    {
        //StartCoroutine(BeginToRemovePlayer());
    }

    [Rpc(SendTo.Server)]
    public void RequestTimeDataRPC()
    {
        var d = DayNightCycle.Instance;
        SendTimeDataRPC(d.currentYear, d.currentDayOfYear, d.currentTime, d.currentDay, d.currentSeasonProgress, (int)d.currentSeason, (int)d.dayType);
    }

    [Rpc(SendTo.NotServer)]
    public void SendTimeDataRPC(int year, int dayYear, int time, int day, int seasonProg, int season, int dayType)
    {
        DayNightCycle.Instance.SyncTime(year, dayYear, time, day, seasonProg, season, dayType);
    }

    [Rpc(SendTo.Server)]
    public void RequestWeatherRPC()
    {
        SendWeatherDataRPC(WeatherManager.Instance.isRaining, WeatherManager.Instance.targetReached, WeatherManager.Instance.rainProgress);
    }

    [Rpc(SendTo.NotServer)]
    private void SendWeatherDataRPC(bool isRaining, bool targetReached, int rainProg)
    {
        WeatherManager.Instance.SyncWeatherData(isRaining, targetReached, rainProg);
    }

    [Rpc(SendTo.Server)]
    public void SpawnEtherRPC(int playerId, float yLevel)
    {
        var newPos = GameManager.Instance.FindPlayerById(playerId).transform.position;
        newPos.y = yLevel;
        var arena = Instantiate(GameManager.Instance.localPlayer.GetComponent<EtherShardManager>().arenaFloor, newPos, Quaternion.identity);
        arena.GetComponent<NetworkObject>().Spawn();
        GameManager.Instance.FindPlayerById(playerId).GetComponent<EtherShardManager>().arenaInstance = arena;
    }

    [Rpc(SendTo.Server)]
    public void DespawnEtherRPC(int playerId)
    {
        GameManager.Instance.FindPlayerById(playerId).GetComponent<EtherShardManager>().arenaInstance.GetComponent<NetworkObject>().Despawn();
    }

    [Rpc(SendTo.Server)]
    public void RequestToMoveObjectRPC(Vector3 pos, ulong objId)
    {
        NetworkManager.SpawnManager.SpawnedObjects[objId].transform.position = pos;
    }

    [Rpc(SendTo.Server)]
    public void SpawnProjectileRPC(Vector3 pos, Quaternion rot, string itemType, int uses, ulong playerObjId)
    {
        var projectile = Instantiate(networkManager.SpawnManager.SpawnedObjects[playerObjId].GetComponent<PlayerMain>().pfProjectile, pos, rot);
        projectile.transform.position = new Vector3(pos.x, pos.y + 1, pos.z);
        var vel = projectile.transform.right * 100;
        vel.y = pos.y;
        projectile.GetComponent<Rigidbody>().velocity = vel;
        var newItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(itemType), uses = uses };
        projectile.GetComponent<ProjectileManager>().SetProjectile(newItem, pos, networkManager.SpawnManager.SpawnedObjects[playerObjId].gameObject, vel, true, false, .5f);
        projectile.GetComponent<NetworkObject>().Spawn();
    }
}
