using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class RealItem : NetworkBehaviour
{
    private TextMeshPro textMeshPro;
    private PlayerMain player;
    private TextMeshProUGUI txt;
    private GameObject mouse;
    private Interactable interactable;
    public Hoverable hoverBehavior;
    public PlayerInteractUnityEvent interactEvent = new PlayerInteractUnityEvent();
    public GameObject vfx;
    private Transform playerTarget;
    private TileData currentTile;

    /*public static RealItem SpawnRealItem(Vector3 position, Item item, bool visible = true, bool used = false, int _ammo = 0, bool _isHot = false, bool pickupCooldown = false, bool isMagnetic = false, bool loading = false) //spawns item into the game world.
    {
        Transform transform = Instantiate(ItemObjectArray.Instance.pfItem, position, Quaternion.identity); //sets transform variable to instance that was just created

        RealItem realItem = transform.GetComponent<RealItem>(); //Gets component of this class for the item just spawned so it can use SetItem() function to set the item type to whatever the spawnrealitem function received when called.
        SpriteRenderer spr = realItem.GetComponent<SpriteRenderer>();
        TextMeshPro txt = transform.Find("Text").GetComponent<TextMeshPro>();
        if (visible)
        {
            spr.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            spr.color = new Color(1f, 1f, 1f, 0f);
            txt.color = Color.clear;
        }
        if (!used && item.itemSO.maxUses != 0)
        {
            item.uses = item.itemSO.maxUses;
        }

        if (pickupCooldown)
        {
            realItem.pickUpCooldown = pickupCooldown;
        }

        if (isMagnetic)
        {
            realItem.isMagnetic = true;
        }

        item.ammo = _ammo;
        item.equipType = item.itemSO.equipType;
        realItem.SetItem(item, item.isHot, loading);
        return realItem;
    }*/

    public static RealItem SpawnRealItem(Vector3 position, ItemsSaveData itemData, bool loading = false) //spawns item into the game world.
    {
        Transform transform = Instantiate(ItemObjectArray.Instance.pfItem, position, Quaternion.identity); //sets transform variable to instance that was just created

        RealItem realItem = transform.GetComponent<RealItem>(); //Gets component of this class for the item just spawned so it can use SetItem() function to set the item type to whatever the spawnrealitem function received when called.
        SpriteRenderer spr = realItem.GetComponent<SpriteRenderer>();
        TextMeshPro txt = transform.Find("Text").GetComponent<TextMeshPro>();

        realItem.item = new Item() { 
            uses = itemData.uses,
            ammo = itemData.ammo, 
            itemSO = ItemObjectArray.Instance.SearchItemList(itemData.itemType), 
            amount = itemData.amount};

        realItem.item.equipType = realItem.item.itemSO.equipType;

        if (realItem.item.itemSO.canStoreItems)
        {
            realItem.item.containedItems = new Item[realItem.item.itemSO.maxStorageSpace];
        }

        if (itemData.containedTypes != null)
        {
            Item[] containedTypes = new Item[itemData.containedTypes.Length];
            for (int i = 0; i < itemData.containedTypes.Length; i++)
            {
                if (itemData.containedTypes[i] != null)
                {
                    containedTypes[i] = new Item
                    {
                        itemSO = ItemObjectArray.Instance.SearchItemList(itemData.containedTypes[i]),
                        amount = 1
                    };
                }
            }
            realItem.item.containedItems = containedTypes;
        }

        if (itemData.currentPickupCooldown > 0f)
        {
            realItem.pickUpCooldown = true;
        }

        if (itemData.isMagnetic)
        {
            realItem.isMagnetic = true;
        }

        realItem.SetItem(realItem.item, realItem.item.isHot, itemData.currentPickupCooldown, loading);
        return realItem;
    }

    public static RealItem SpawnNewRealItem(Vector3 position, ItemSO itemSO, int amount, bool _isMagnetic = false, bool _isHot = false, float cooldown = 0f, bool loading = false) //spawns item into the game world.
    {
        Transform transform = Instantiate(ItemObjectArray.Instance.pfItem, position, Quaternion.identity); //sets transform variable to instance that was just created

        RealItem realItem = transform.GetComponent<RealItem>(); //Gets component of this class for the item just spawned so it can use SetItem() function to set the item type to whatever the spawnrealitem function received when called.
        SpriteRenderer spr = realItem.GetComponent<SpriteRenderer>();
        TextMeshPro txt = transform.Find("Text").GetComponent<TextMeshPro>();

        realItem.item = new Item()
        {
            uses = itemSO.maxUses,
            ammo = 0,
            itemSO = itemSO,
            amount = amount
        };

        realItem.item.equipType = realItem.item.itemSO.equipType;

        if (realItem.item.itemSO.canStoreItems)
        {
            realItem.item.containedItems = new Item[realItem.item.itemSO.maxStorageSpace];
        }

        if (cooldown > 0f)
        {
            realItem.pickUpCooldown = true;
        }

        if (_isMagnetic)
        {
            realItem.isMagnetic = true;
        }

        realItem.SetItem(realItem.item, _isHot, cooldown, loading);
        return realItem;
    }

    public Item item;
    private SpriteRenderer spriteRenderer;
    public SpriteRenderer shadowCaster;
    public bool isHot = false;
    public bool pickUpCooldown = false;
    public bool isMagnetic = false;
    private float multiplier = 0f;
    public bool hasSpecialInteraction;

    private void Awake()
    {
        hoverBehavior = GetComponent<Hoverable>();
        interactable = GetComponent<Interactable>();
        interactable.OnInteractEvent.AddListener(CollectItem);
        
        //player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();
        mouse = GameObject.FindGameObjectWithTag("Mouse");
        txt = mouse.GetComponentInChildren<TextMeshProUGUI>();
        StartCoroutine(CoolDown());
    }

    private void Start()
    {
        GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;
        if (GameManager.Instance.localPlayerMain != null)
        {
            player = GameManager.Instance.localPlayerMain;
        }
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        player = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (item == null)
        {
            AskForItemDataRPC();
        }
    }

    public void OnInteract()
    {
        interactEvent?.Invoke();
        if (player.hasTongs && player.equippedHandItem.heldItem == null && UI_ItemSlotController.IsStorable(item, player.equippedHandItem))
        {
            player.equippedHandItem.heldItem = Item.DupeItem(item);
            player.equippedHandItem.heldItem.amount = 1;
            player.UpdateContainedItem(player.equippedHandItem.heldItem);
            item.amount--;
            if (item.amount <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void GetReadyToLeaveEther()
    {
        PlayerMain.Instance.GetComponent<EtherShardManager>().OnReturnToReality += LeaveEther;
    }

    private void LeaveEther(object sender, System.EventArgs e)
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    public static void DropItem(Item item, Vector3 pos, bool magnetic = false)
    {
        if (magnetic)
        {
            var Item = SpawnRealItem(pos, item.itemData);
            CalebUtils.RandomDirForceNoYAxis3D(Item.GetComponent<Rigidbody>(), 5);
        }
        else
        {
            var Item = SpawnRealItem(pos, item.itemData);
            CalebUtils.RandomDirForceNoYAxis3D(Item.GetComponent<Rigidbody>(), 5);
        }
    }

    private IEnumerator PickupCoolDown(float time = .5f)
    {
        transform.GetChild(0).GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(time);
        transform.GetChild(0).GetComponent<Collider>().enabled = true;

        if (isMagnetic)
        {
            StartCoroutine(Magnetize());
        }
    }

    private IEnumerator Magnetize()
    {
        if (playerTarget == null)
        {
            foreach (var player in GameManager.Instance.playerList)
            {
                if (playerTarget == null)
                {
                    playerTarget = player.transform;
                }
                else
                {
                    if (Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, playerTarget.position))
                    {
                        playerTarget = player.transform;
                    }
                }
            }
        }

        if (playerTarget == null)
        {
            yield break;
        }
        multiplier += .1f;

        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, Time.deltaTime * multiplier * 2);
        /*if (player.gameObject != null && player.StateMachine.currentPlayerState != player.deadState)
        {
            
        }
        else
        {
            yield break;
        }*/
        yield return null;
        StartCoroutine(Magnetize());
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(10f);
        if (isHot)
        {
            isHot = false;
        }
    }

    public void SetItem(Item item, bool _isHot, float remainingTime = 0f, bool loading = false)
    {
        if (remainingTime > 0f)
        {
            StartCoroutine(PickupCoolDown());
        }

        if (_isHot)
        {
            isHot = true;
            vfx.gameObject.SetActive(true);
            StartCoroutine(CheckHotness());
        }

        if (item == null)//this might break some things???? im not sure honestly
        {
            Destroy(gameObject);
        }

        spriteRenderer.sprite = item.itemSO.itemSprite;
        shadowCaster.sprite = item.itemSO.itemSprite;
        if (item.ammo > 0)
        {
            spriteRenderer.sprite = item.itemSO.loadedSprite;
            shadowCaster.sprite = item.itemSO.loadedSprite;
        }
        this.item = item;
        hoverBehavior.Name = item.itemSO.itemName;
        RefreshAmount(item);
        //gameObject.GetComponent<MonoBehaviour>().enabled = false; idk why this shit no work AND lag game

        if (GameManager.Instance.isServer)
        {
            GetComponent<NetworkObject>().Spawn();
        }

        int[] containedItemTypes = null;
        int[] containedItemAmounts = null;

        if (item.containedItems != null)
        {
            containedItemTypes = ConvertContainedItemTypes(item.containedItems);
            containedItemAmounts = ConvertContainedItemAmounts(item.containedItems);
        }

        string heldItemType = null;
        if (item.heldItem != null)
        {
            heldItemType = item.heldItem.itemSO.itemType;
        }

        if (IsServer)
        {
            SetItemRPC(item.itemSO.itemType, item.amount, item.uses, item.ammo, (int)item.itemSO.equipType, item.isHot, item.remainingTime, containedItemTypes, containedItemAmounts, heldItemType, isMagnetic);
        }

        if (!loading)
        {
            Cell _currentTile = WorldGeneration.Instance.FindTileByPosition(new Vector2Int(Mathf.RoundToInt(transform.position.x / WorldGeneration.Instance.tileSeparationDistance) + WorldGeneration.Instance.worldSize, Mathf.RoundToInt(transform.position.z / WorldGeneration.Instance.tileSeparationDistance + WorldGeneration.Instance.worldSize))).GetComponent<Cell>();
            _currentTile.tileData.itemDataList.Add(item.itemData);
            _currentTile.itemList.Add(this);
            currentTile = _currentTile.tileData;
        }
        else
        {
            Cell _currentTile = WorldGeneration.Instance.FindTileByPosition(new Vector2Int(Mathf.RoundToInt(transform.position.x / WorldGeneration.Instance.tileSeparationDistance) + WorldGeneration.Instance.worldSize, Mathf.RoundToInt(transform.position.z / WorldGeneration.Instance.tileSeparationDistance + WorldGeneration.Instance.worldSize))).GetComponent<Cell>();
            _currentTile.itemList.Add(this);
            currentTile = _currentTile.tileData;
        }
        Save();
    }

    public static int[] ConvertContainedItemTypes(Item[] containedItems)
    {
        int[] containedItemTypes = new int[containedItems.Length];
        for (int i = 0; i < containedItemTypes.Length; i++)
        {
            if (containedItems[i] != null)
            {
                containedItemTypes[i] = containedItems[i].itemSO.itemID;
            }
            else
            {
                containedItemTypes[i] = -1;
            }
        }
        return containedItemTypes;
    }

    public static int[] ConvertContainedItemAmounts(Item[] containedItems)
    {
        int[] containedItemAmounts = new int[containedItems.Length];
        for (int i = 0; i < containedItemAmounts.Length; i++)
        {
            if (containedItems[i] != null)
            {
                containedItemAmounts[i] = containedItems[i].amount;
            }
        }
        return containedItemAmounts;
    }

    [Rpc(SendTo.Server)]
    private void DespawnNetworkObjectRPC()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    [Rpc(SendTo.Server)]
    private void AskForItemDataRPC()
    {
        int[] containedItemTypes = null;
        int[] containedItemAmounts = null;

        if (item.containedItems != null)
        {
            containedItemTypes = ConvertContainedItemTypes(item.containedItems);
            containedItemAmounts = ConvertContainedItemAmounts(item.containedItems);
        }

        string heldItemType = null;
        if (item.heldItem != null)
        {
            heldItemType = item.heldItem.itemSO.itemType;
        }

        SetItemRPC(item.itemSO.itemType, item.amount, item.uses, item.ammo, (int)item.itemSO.equipType, item.isHot, item.remainingTime, containedItemTypes, containedItemAmounts, heldItemType, isMagnetic);
    }

    [Rpc(SendTo.NotServer)]
    private void SetItemRPC(string itemType, int amount, int uses, int ammo, int equipType, bool isHot, float timeRemaining = 0, int[] containedItemTypes = null, int[] containedItemAmounts = null, string heldItemType = null, bool magnetic = false)
    {
        Item newItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(itemType), amount = amount, uses = uses, ammo = ammo, equipType = (Item.EquipType)equipType, isHot = isHot, remainingTime = timeRemaining};
        isMagnetic = magnetic;
        pickUpCooldown = magnetic;

        if (newItem.itemSO.canStoreItems)
        {
            newItem.containedItems = new Item[newItem.itemSO.maxStorageSpace];
        }

        if (containedItemTypes != null)
        {
            Item[] newContainedItemsList = new Item[containedItemTypes.Length];
            for (int i = 0; i < containedItemTypes.Length; i++)
            {
                if (containedItemTypes[i] != -1)
                {
                    newContainedItemsList[i] = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(containedItemTypes[i]) , amount = containedItemAmounts[i]};
                }
                else
                {
                    newContainedItemsList[i] = null;
                }
            }
            newItem.containedItems = newContainedItemsList;
        }

        if (heldItemType != null)
        {
            newItem.heldItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(heldItemType), amount = 1 };
        }

        SetItem(newItem, isHot, timeRemaining);
    }

    public Item GetItem()
    {
        return item;
    }

    private IEnumerator CheckHotness()
    {
        yield return null;
        if (item.isHot)
        {
            StartCoroutine(CheckHotness());
        }
        else
        {
            vfx.gameObject.SetActive(false);
            yield break;
        }
    }

    public void RefreshAmount(Item item)
    {
        if (item.amount > 1)
        {
            textMeshPro.SetText(item.amount.ToString());
        }
        else if (item.uses > 0)
        {
            int newUses = Mathf.RoundToInt((float)item.uses / item.itemSO.maxUses * 100);
            textMeshPro.text = $"{newUses}%";
        }
        else
        {
            textMeshPro.SetText("");
        }
    }

    public void DestroySelf()
    {
        /*if (GetComponentInParent<Cell>() != null)
        {
            int i = 0;
            Cell cell = GetComponentInParent<Cell>();
            foreach (string tileItem in cell.tileData.itemTypes)
            {
                if (tileItem == item.itemSO.itemType)
                {
                    cell.tileData.itemTypes.RemoveAt(i);
                    cell.tileData.itemLocations.RemoveAt(i);
                    break;
                }
                i++;
            }
        }*/
        currentTile.itemDataList.Remove(item.itemData);
        Debug.Log($"removing from {currentTile.tileLocation}");
        DespawnNetworkObjectRPC();
        //Destroy(gameObject);
    }

    private void OnMouseEnter()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        txt.text = $"LMB: Pick up {item.itemSO.itemName}";
    }

    private void OnMouseExit()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        txt.text = "";
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.isTrigger && item != null && !item.isHot)
        {
            if (collision.transform.root.gameObject.name == "Player(Clone)" && isMagnetic && collision.transform.root.GetComponent<PlayerMain>().IsLocalPlayer)//no need to run this code per client
            {
                CollectItem(collision.transform.root.GetComponent<PlayerMain>().swingingState.interactArgs);
            }
        }
    }

    public void Save()//need to assign tile in new chunk system
    {
        if (item == null)
        {
            item.itemData.itemType = "NULL";
            Debug.LogError("Null item skipped!");
            return;
        }
        else if (item.itemSO.itemType == "")
        {
            item.itemData.itemType = "NULL";
            Debug.LogError("Item with empty string as type skipped!!!");
            return;
        }
        else if (ItemObjectArray.Instance.SearchItemList(item.itemSO.itemType) == null)
        {
            item.itemData.itemType = "NULL";
            Debug.LogError("You forgot to set item in the global item list!!! Skipping!!!");
            return;
        }
        item.itemData.itemType = item.itemSO.itemType;
        item.itemData.uses = item.uses;
        item.itemData.ammo = item.ammo;
        item.itemData.amount = item.amount;
        item.itemData.pos = transform.position;
        if (item.itemSO.canStoreItems && item.containedItems != null)
        {
            string[] containedTypes = new string[item.containedItems.Length];
            for (int i = 0; i < item.containedItems.Length; i++)
            {
                if (item.containedItems[i] != null)
                {
                    containedTypes[i] = item.containedItems[i].itemSO.itemType;
                }
            }
            containedTypes.Reverse();
            item.itemData.containedTypes = containedTypes;
        }
    }

    public void CollectItem(InteractArgs args)
    {
        if (item.isHot || args.playerSender.StateMachine.currentPlayerState == args.playerSender.deadState)
        {
            return;
        }
        args.playerSender.inventory.AddItem(item, args.playerSender.transform.position);
        interactable.OnInteractEvent.RemoveAllListeners();
        DestroySelf();
    }
}
