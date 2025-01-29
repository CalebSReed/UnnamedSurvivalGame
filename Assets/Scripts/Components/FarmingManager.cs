using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FarmingManager : NetworkBehaviour
{
    public SpriteRenderer plantSpr;

    [SerializeField] private int growthTarget = DayNightCycle.fullDayTimeLength;
    public float growthTimer;
    public bool isHarvestable;
    public bool isGrowing;
    public bool isPlanted;
    public Item seed;
    private Inventory plantLoot;
    RealWorldObject realObj;

    private void Awake()
    {
        realObj = GetComponent<RealWorldObject>();
        plantSpr = transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>();
        plantSpr.sprite = null;
        plantLoot = new Inventory(64);
        isHarvestable = false;
        isGrowing = false;
        realObj.receiveEvent.AddListener(ReceiveFarmingItems);
        realObj.interactEvent.AddListener(ReceiveFarmingItems);
        realObj.hasSpecialInteraction = true;

        realObj.hoverBehavior.SpecialCase = true;
        realObj.hoverBehavior.specialCaseModifier.AddListener(CheckStatus);
        realObj.onLoaded += OnLoaded;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AskForFarmDataRPC();
    }

    private void Update()
    {
        if (isGrowing)
        {
            realObj.spriteRenderer.color = new Color(.8f, .8f, 1);

            growthTimer += Time.deltaTime;
            realObj.saveData.timerProgress = growthTimer;

            if (growthTimer >= growthTarget)
            {
                BecomeHarvestable();
            }

            if (DayNightCycle.Instance.currentSeason == DayNightCycle.Season.Winter)
            {
                RealItem.DropItem(new Item { itemSO = seed.itemSO, amount = 1 }, transform.position);
                isGrowing = false; 
            }
        }
    }

    private void CheckStatus()
    {
        if (realObj.playerMain.isHoldingItem && !isPlanted && realObj.playerMain.heldItem.itemSO.isSeed)
        {
            realObj.hoverBehavior.Name = realObj.playerMain.heldItem.itemSO.itemName;
            realObj.hoverBehavior.Prefix = "LMB: Plant ";
        }
        else if (isPlanted && !isGrowing && !isHarvestable)
        {
            if (realObj.playerMain.isHoldingItem && realObj.playerMain.heldItem.itemSO.itemType == "BowlOfWater")
            {
                realObj.hoverBehavior.Name = realObj.woso.objName;//add check for holding water bucket
                realObj.hoverBehavior.Prefix = "LMB: Water ";
            }
            else if (realObj.playerMain.doAction == Action.ActionType.Water && realObj.playerMain.equippedHandItem.ammo > 0)
            {
                realObj.hoverBehavior.Name = realObj.woso.objName;//add check for holding water bucket
                realObj.hoverBehavior.Prefix = "RMB: Water ";
            }
            else
            {
                realObj.hoverBehavior.Name = realObj.woso.objName;
                realObj.hoverBehavior.Prefix = "";
            }
        }
        else if (isHarvestable)
        {
            realObj.hoverBehavior.Name = realObj.woso.objName;
            realObj.hoverBehavior.Prefix = "RMB: Harvest ";
        }
        else
        {
            realObj.hoverBehavior.Name = realObj.woso.objName;
            realObj.hoverBehavior.Prefix = "";
        }
    }

    public void PlantItem(Item _item, bool alertOthers = true)
    {
        plantSpr.sprite = _item.itemSO.itemSprite;
        seed = _item;
        realObj.saveData.heldItemType = seed.itemSO.itemType;
        //plantLoot.SimpleAddItemArray(_item.itemSO.seedRewards);
        isPlanted = true;
        //isGrowing = true;
        //StartCoroutine(GrowPlant());//growthTimer
        if (WeatherManager.Instance.isRaining)
        {
            isGrowing = true;
        }
        if (alertOthers)
        {
            PlantItemRPC(seed.itemSO.itemType);
        }
    }

    [Rpc(SendTo.NotMe)]
    private void PlantItemRPC(string itemType)
    {
        Item newItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(itemType) };
        PlantItem(newItem, false);
    }

    [Rpc(SendTo.NotMe)]
    private void GrowItemRPC()
    {
        isGrowing = true;
    }

    [Rpc(SendTo.NotMe)]
    private void BecomeHarvestableRPC(string itemType)
    {
        seed = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(itemType) };
        BecomeHarvestable();
    }

    [Rpc(SendTo.NotMe)]
    private void HarvestedByOtherPlayerRPC()
    {
        Harvest(false);
    }

    [Rpc(SendTo.Server)]
    private void AskForFarmDataRPC()
    {
        if (isGrowing)
        {
            GrowItemRPC();
        }
        else if (isHarvestable)
        {
            BecomeHarvestableRPC(seed.itemSO.itemType);
        }
        else if (isPlanted)
        {
            PlantItemRPC(seed.itemSO.itemType);
        }
    }

    public void BecomeHarvestable()
    {
        realObj.saveData.timerProgress = growthTarget;
        realObj.spriteRenderer.color = Color.white;
        isHarvestable = true;
        isGrowing = false;
        //clear inventory if we ever save all object's inventories which seems like a pretty nice function later maybe
        if (seed.itemSO.seedObjectReward != null)
        {
            if (GameManager.Instance.isServer)
            {
                RealWorldObject.SpawnWorldObject(transform.position, new WorldObject { woso = seed.itemSO.seedObjectReward });//this should always be a playermade domestic version of a plant
            }
            else
            {
                ClientHelper.Instance.AskToSpawnObjRPC(transform.position, seed.itemSO.seedObjectReward.objType);
            }
            GetComponent<RealWorldObject>().Break();
            return;
        }
        plantLoot = new Inventory(16);
        plantLoot.SimpleAddItemArray(seed.itemSO.seedRewards);
        plantSpr.sprite = plantLoot.GetItemList()[0].itemSO.itemSprite;
    }

    public void Harvest(bool dropItem = true)
    {
        isHarvestable = false;
        if (dropItem)
        {
            plantLoot.DropAllItems(transform.position);
        }
        plantSpr.sprite = null;
        isPlanted = false;
        seed = null;
        growthTimer = 0;
        realObj.saveData.heldItemType = null;
        realObj.saveData.timerProgress = growthTimer;
        if (dropItem)
        {
            HarvestedByOtherPlayerRPC();
        }
    }

    private void ReceiveFarmingItems()
    {
        if (Vector3.Distance(realObj.playerMain.transform.position, transform.position) > realObj.playerMain.collectRange)
        {
            return;
        }
        if (isHarvestable)
        {
            Harvest();
            return;
        }
        if (realObj.playerMain.isHoldingItem)
        {
            if (realObj.playerMain.heldItem.itemSO.doActionType == Action.ActionType.Water && isPlanted && !isGrowing && !realObj.playerMain.heldItem.itemSO.needsAmmo || realObj.playerMain.heldItem.itemSO.doActionType == Action.ActionType.Water && isPlanted && realObj.playerMain.heldItem.itemSO.needsAmmo && realObj.playerMain.heldItem.ammo > 0 && !isGrowing)
            {
                isGrowing = true;
                GrowItemRPC();
            }
            else if (realObj.playerMain.heldItem.itemSO.isSeed && !isPlanted)
            {
                PlantItem(realObj.playerMain.heldItem);
            }
            else//cant do anything, dont use item
            {
                return;
            }
        }
        else if (realObj.playerMain.isHandItemEquipped && realObj.playerMain.equippedHandItem.itemSO.doActionType == Action.ActionType.Water && realObj.playerMain.equippedHandItem.ammo > 0 && isPlanted && !isGrowing)
        {
            isGrowing = true;
            GrowItemRPC();
            realObj.playerMain.equippedHandItem.ammo--;
            realObj.playerMain.UpdateEquippedItem(realObj.playerMain.equippedHandItem);
            return;
        }
        else//do nothing if u have nothing
        {
            return;
        }
        realObj.playerMain.UseHeldItem(true);
    }

    private void OnRaining(object sender, System.EventArgs e)
    {
        if (isPlanted && !isGrowing)
        {
            isGrowing = true;
        }
    }

    private void OnLoaded(object sender, System.EventArgs e)
    {
        if (realObj.saveData.heldItemType != null)
        {
            seed = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(realObj.saveData.heldItemType) };
        }

        if (realObj.saveData.timerProgress > 0 && realObj.saveData.timerProgress < growthTarget)
        {
            growthTimer = realObj.saveData.timerProgress;
            PlantItem(seed);
            isGrowing = true;
        }
        else if (seed != null && realObj.saveData.timerProgress == 0)
        {
            PlantItem(seed);
        }
        else if (realObj.saveData.timerProgress >= growthTarget)
        {
            BecomeHarvestable();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        realObj.interactEvent.RemoveListener(ReceiveFarmingItems);
    }

    private void OnEnable()
    {
        WeatherManager.Instance.onRaining += OnRaining;
    }

    private void OnDisable()
    {
        WeatherManager.Instance.onRaining -= OnRaining;
    }
}
