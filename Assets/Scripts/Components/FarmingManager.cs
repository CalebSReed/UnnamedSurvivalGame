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

    private void Awake()
    {
        plantSpr = transform.GetChild(2).GetComponent<SpriteRenderer>();
        plantSpr.sprite = null;
        plantLoot = new Inventory(64);
        isHarvestable = false;
        isGrowing = false;
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
        isHarvestable = true;
        isGrowing = false;
        //clear inventory if we ever save all object's inventories which seems like a pretty nice function later maybe
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

    public bool IsGrowing()
    {
        return isGrowing;
    }

    public bool GetHarvestability()
    {
        return isHarvestable;
    }
}
