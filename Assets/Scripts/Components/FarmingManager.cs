using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingManager : MonoBehaviour
{
    public SpriteRenderer plantSpr;

    [SerializeField] private int growthTarget = DayNightCycle.fullDayTimeLength;
    public int growthTimer;
    public bool isHarvestable;
    public bool isGrowing;
    public bool isPlanted;
    public ItemSO seed;
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
        realObj.interactEvent.AddListener(ReceiveFarmingItems);
        realObj.hasSpecialInteraction = true;
    }

    public void PlantItem(Item _item)
    {
        plantSpr.sprite = _item.itemSO.itemSprite;
        seed = _item.itemSO;
        //plantLoot.SimpleAddItemArray(_item.itemSO.seedRewards);
        isPlanted = true;
        //isGrowing = true;
        //StartCoroutine(GrowPlant());//growthTimer
    }

    public IEnumerator GrowPlant()
    {
        realObj.spriteRenderer.color = new Color(.8f, .8f, 1);
        isGrowing = true;
        yield return new WaitForSeconds(1);
        growthTimer++;

        if (growthTimer >= growthTarget)
        {
            BecomeHarvestable();
            yield break;
        }

        StartCoroutine(GrowPlant());
    }

    public void BecomeHarvestable()
    {
        realObj.spriteRenderer.color = Color.white;
        isHarvestable = true;
        isGrowing = false;
        //clear inventory if we ever save all object's inventories which seems like a pretty nice function later maybe
        if (seed.seedObjectReward != null)
        {
            RealWorldObject.SpawnWorldObject(transform.position, new WorldObject { woso = seed.seedObjectReward });//this should always be a playermade domestic version of a plant
            GetComponent<RealWorldObject>().Break();
            return;
        }
        plantLoot = new Inventory(16);
        plantLoot.SimpleAddItemArray(seed.seedRewards);
        plantSpr.sprite = plantLoot.GetItemList()[0].itemSO.itemSprite;
    }

    public void Harvest()
    {
        isHarvestable = false;
        plantLoot.DropAllItems(transform.position);
        plantSpr.sprite = null;
        isPlanted = false;
        seed = null;
        growthTimer = 0;

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
            if (realObj.playerMain.heldItem.itemSO.doActionType == Action.ActionType.Water && isPlanted && !realObj.playerMain.heldItem.itemSO.needsAmmo || realObj.playerMain.heldItem.itemSO.doActionType == Action.ActionType.Water && isPlanted && realObj.playerMain.heldItem.itemSO.needsAmmo && realObj.playerMain.heldItem.ammo > 0)
            {
                StartCoroutine(GrowPlant());               
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
        else if (realObj.playerMain.isHandItemEquipped && realObj.playerMain.equippedHandItem.itemSO.doActionType == Action.ActionType.Water && realObj.playerMain.equippedHandItem.ammo > 0 && isPlanted)
        {
            StartCoroutine(GrowPlant());
            realObj.playerMain.equippedHandItem.ammo--;
            realObj.playerMain.UpdateEquippedItem(realObj.playerMain.equippedHandItem, realObj.playerMain.handSlot);
            return;
        }
        else//do nothing if u have nothing
        {
            return;
        }
        realObj.playerMain.UseHeldItem(true);
    }

    private void OnDestroy()
    {
        realObj.interactEvent.RemoveListener(ReceiveFarmingItems);
    }
}
