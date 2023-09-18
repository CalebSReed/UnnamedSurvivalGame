using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingManager : MonoBehaviour
{
    private SpriteRenderer plantSpr;

    [SerializeField] private int growthTimer;
    [SerializeField] private int growthTarget;
    [SerializeField] private bool isHarvestable;
    [SerializeField] private bool isGrowing;
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
        plantLoot.SimpleAddItemArray(_item.itemSO.seedRewards);
        growthTarget = 10;
        isGrowing = true;
        StartCoroutine(GrowPlant());//growthTimer
    }

    private IEnumerator GrowPlant()
    {
        yield return new WaitForSeconds(1);
        growthTimer++;

        if (growthTimer >= growthTarget)
        {
            isHarvestable = true;
            isGrowing = false;
            yield break;
        }

        StartCoroutine(GrowPlant());
    }

    public void Harvest()
    {
        isHarvestable = false;
        plantLoot.DropAllItems(transform.position);
        plantSpr.sprite = null;
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
