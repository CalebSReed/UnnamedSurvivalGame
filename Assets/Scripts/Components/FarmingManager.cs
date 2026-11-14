using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

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
        //realObj.receiveEvent.AddListener(ReceiveFarmingItems);
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
        Item potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;
        if (potentialItem == null)
        {
            realObj.hoverBehavior.Name = realObj.woso.objName;
            realObj.hoverBehavior.Prefix = "";
            return;
        }
        else
        {
            if (!isPlanted && potentialItem.itemSO.isSeed)
            {
                realObj.hoverBehavior.Name = potentialItem.itemSO.itemName;
                realObj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Plant ";
                return;
            }
            else if (isPlanted && !isGrowing && !isHarvestable)
            {
                if (potentialItem.itemSO.itemType == "BowlOfWater")
                {
                    realObj.hoverBehavior.Name = realObj.woso.objName;//add check for holding water bucket
                    realObj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Water ";
                    return;
                }
                else if (realObj.playerMain.doAction == Action.ActionType.Water && realObj.playerMain.equippedHandItem.ammo > 0)
                {
                    realObj.hoverBehavior.Name = realObj.woso.objName;//add check for holding water bucket
                    realObj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Water ";
                    return;
                }
            }
        }


        if (isHarvestable)
        {
            realObj.hoverBehavior.Name = realObj.woso.objName;
            realObj.hoverBehavior.Prefix = $"{GameManager.Instance.localPlayerMain.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Harvest ";
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
            PlantItemRPC(seed.itemSO.itemType);
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
        realObj.spriteRenderer.color = Color.white;
        if (dropItem)
        {
            HarvestedByOtherPlayerRPC();
        }
    }

    private void ReceiveFarmingItems()
    {
        Item potentialItem = UI_ItemSlotController.Instance.selectedItemSlot.item;

        if (Vector3.Distance(realObj.playerMain.transform.position, transform.position) > realObj.playerMain.collectRange)
        {
            return;
        }
        if (isHarvestable)
        {
            Harvest();
            return;
        }
        if (potentialItem != null)
        {
            if (potentialItem.itemSO.doActionType == Action.ActionType.Water && isPlanted && !isGrowing && !potentialItem.itemSO.needsAmmo)
            {
                isGrowing = true;
                GrowItemRPC();
                realObj.playerMain.inventory.SubtractItem(UI_ItemSlotController.Instance.selectedItemSlot.itemSlotNumber);
                if (potentialItem.itemSO.isBowl)
                {
                    var itemToAdd = new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 };
                    itemToAdd.SaveData();
                    GameManager.Instance.localPlayerMain.inventory.AddItem(itemToAdd, transform.position, false);
                }
            }
            else if (potentialItem.itemSO.isSeed && !isPlanted)
            {
                PlantItem(potentialItem);
                realObj.playerMain.inventory.SubtractItem(UI_ItemSlotController.Instance.selectedItemSlot.itemSlotNumber);
            }
            else if (potentialItem.itemSO.doActionType == Action.ActionType.Water && isPlanted && !isGrowing && potentialItem.itemSO.needsAmmo && potentialItem.ammo > 0)
            {
                isGrowing = true;
                GrowItemRPC();
                realObj.playerMain.equippedHandItem.ammo--;
                realObj.playerMain.UpdateEquippedItem(realObj.playerMain.equippedHandItem);
            }
        }        
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
        realObj.hoverBehavior.specialCaseModifier.RemoveListener(CheckStatus);
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
