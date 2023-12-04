using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Random = UnityEngine.Random;

public class KilnBehavior : MonoBehaviour
{
    RealWorldObject obj;
    SpriteRenderer objSpriteRenderer;
    Smelter smelter;

    public Item smeltingItemReward = null;
    public Item originalSmeltItem = null;
    public int logsToReplace = 0;

    public event EventHandler OnClosed;
    public event EventHandler OnOpened;

    private AudioManager audio;

    private void Start()//kilns like this can break, make a brick version that will last
    {
        obj = gameObject.GetComponent<RealWorldObject>();
        obj.receiveEvent.AddListener(ReceiveItem);

        gameObject.AddComponent<Smelter>();

        audio = GetComponent<AudioManager>();

        smelter = gameObject.GetComponent<Smelter>();
        smelter.SetMaxTemperature(obj.obj.woso.maxTemp);
        smelter.SetMintemperature(obj.obj.woso.minTemp);

        objSpriteRenderer = obj.spriteRenderer;

        smelter.OnFinishedSmelting += OnFinishedSmelting;
        //obj.inventory.OnItemListChanged += OnItemListChanged;
        smelter.ReplaceWood += ReplaceWood;
        logsToReplace = 0;
        GetComponent<TemperatureEmitter>().StopAllCoroutines();
    }

    private void Update()
    {
        //obj.light.intensity = float(smelter.currentTemperature/)

        if (obj.obj.woso.objType == "Kiln")
        {
            //obj.light.pointLightOuterRadius = (float)smelter.currentFuel / smelter.maxFuel * 25;

            if (smelter.isClosed)
            {
                objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnCovered;
            }
            else if (smelter.isSmelting)
            {
                if (smeltingItemReward != null)
                {
                    if (smelter.currentTemperature >= originalSmeltItem.itemSO.smeltValue)
                    {
                        objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnSmelting;
                    }
                    else
                    {
                        objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnLit;
                    }
                }
                else
                {
                    objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnLit;
                }
            }
            else if (smelter.currentFuel > 0)
            {
                objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnFueled;
            }
            else
            {
                objSpriteRenderer.sprite = WorldObject_Assets.Instance.kiln;
            }
        }
    }

    public void ReceiveItem()//change so kiln can only hold 5 items btw
    {
        var _item = obj.playerMain.heldItem;

        if (!smelter.isClosed)//if we return that means action failed. if not, at the very end we will use player item
        {
            Debug.Log("bam added");
            if (_item.itemSO == ItemObjectArray.Instance.SearchItemList("Clay"))
            {
                OnClosed?.Invoke(this, EventArgs.Empty);
                smelter.isClosed = true;
            }
            else if (_item.itemSO.isSmeltable)
            {
                if (!smelter.isSmeltingItem)
                {
                    smeltingItemReward = new Item { amount = 1, itemSO = _item.itemSO.smeltReward };//OHHHH This is a new item outside of inventory
                    originalSmeltItem = new Item { amount = 1, itemSO = _item.itemSO };
                    Debug.Log("ORIGINAL SMELT ITEM SET");
                    smelter.isSmeltingItem = true;
                    if (smelter.isSmelting)
                    {
                        PlayRandomLightSound();
                    }
                    StartCoroutine(smelter.SmeltItem(originalSmeltItem));
                }
                else
                {
                    return;//dont do nuttin
                }
            }
            else if (_item.itemSO.isFuel)
            {
                smelter.SetMaxFuel(obj.obj.woso.maxFuel);
                smelter.AddFuel(_item.itemSO.fuelValue / 2);//half value
                smelter.SetTemperature(_item.itemSO.temperatureBurnValue);
                if (smelter.isSmelting)
                {
                    PlayRandomLightSound();
                }
                //Debug.Log(latestItem.itemType);

                if (_item.itemSO == ItemObjectArray.Instance.SearchItemList("Log"))//last item put into kiln turns into charcoal
                {
                    logsToReplace++;
                }
            }
            else if (_item.itemSO.doActionType == Action.ActionType.Burn && !smelter.isSmelting && smelter.currentFuel > 0)//if burns, not already active, and has fuel
            {
                LightKiln();//yes we finally using durability on torches
            }

        }
        else if (smelter.isClosed)
        {
            return;//if closed, do nothing.
        }

        obj.playerMain.UseHeldItem();
    }

    private bool IsValidKilnItem()
    {
        if (obj.playerMain.heldItem.itemSO.itemType == "Clay")
        {
            return true;
        }
        foreach (ItemSO item in obj.woso.acceptableSmeltItems)
        {
            if (item == obj.playerMain.heldItem.itemSO)
            {
                return true;
            }
        }
        foreach (ItemSO item in obj.woso.acceptableFuels)
        {
            if (item == obj.playerMain.heldItem.itemSO)
            {
                return true;
            }
        }
        return false;
    }

    private void OnFinishedSmelting(object sender, System.EventArgs e)
    {
        if (!smelter.isClosed)
        {
            RealItem newItem = RealItem.SpawnRealItem(transform.position, smeltingItemReward, true, false, 0, false, true, true);
            CalebUtils.RandomDirForceNoYAxis3D(newItem.GetComponent<Rigidbody>(), 5);

            if (originalSmeltItem.itemSO.isBowl)
            {
                RealItem bowlItem = RealItem.SpawnRealItem(transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 }, true, false, 0, false, true, true);
                CalebUtils.RandomDirForceNoYAxis3D(bowlItem.GetComponent<Rigidbody>(), 5);
            }
            if (originalSmeltItem.itemSO.isPlate && !originalSmeltItem.itemSO.isEatable)//drops plate if is crucible and not if they are food (food is always on plate)
            {
                RealItem plateItem = RealItem.SpawnRealItem(transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 1 }, true, false, 0, false, true, true);
                CalebUtils.RandomDirForceNoYAxis3D(plateItem.GetComponent<Rigidbody>(), 5);
            }
            originalSmeltItem = null;
            smeltingItemReward = null;
            smelter.isSmeltingItem = false;
            audio.Play("KilnOut", gameObject);
        }
        else
        {
            obj.inventory.SetValue(smeltingItemReward);
            if (originalSmeltItem.itemSO.isBowl)
            {
                obj.inventory.SetValue(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 });
            }
            if (originalSmeltItem.itemSO.isPlate)
            {
                obj.inventory.SetValue(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 1 });
            }
        }

    }

    private void ReplaceWood(object sender, System.EventArgs e)
    {
        if (smelter.isSmeltingItem)//if we failed to fully smelt item before running out of fuel
        {
            Debug.Log("DROPPING");
            RealItem newItem = RealItem.SpawnRealItem(transform.position, originalSmeltItem, true, false, 0, false, true, true);
            CalebUtils.RandomDirForceNoYAxis3D(newItem.GetComponent<Rigidbody>(), 5);
        }

        if (smelter.isClosed)
        {
            int _logsReplaced = logsToReplace;
            while (_logsReplaced > 0)
            {
                obj.inventory.SetValue((new Item { amount = 1, itemSO = ItemObjectArray.Instance.SearchItemList("Charcoal") }));
                _logsReplaced--;
            }
        }

        obj.inventory.DropAllItems(obj.transform.position, false, true);
        obj.inventory.ClearArray();

        originalSmeltItem = null;
        smeltingItemReward = null;
        smelter.isSmeltingItem = false;
        smelter.isClosed = false;
        OnOpened?.Invoke(this, EventArgs.Empty);
        audio.Stop("KilnRunning");
        audio.Play("KilnOut", gameObject);
        GetComponent<TemperatureEmitter>().StopAllCoroutines();
    }

    public void LightKiln()
    {
        if (smelter.currentFuel > 0 && !smelter.isSmelting)
        {
            smelter.StartSmelting();
            StartCoroutine(GetComponent<TemperatureEmitter>().EmitTemperature());
            PlayRandomLightSound();
            audio.Play("KilnRunning", gameObject);
        }
    }

    public void PlayRandomLightSound()
    {
        int randVal = Random.Range(1, 4);
        if (randVal == 1)
        {
            audio.Play("KilnLight1", gameObject);
        }
        else if (randVal == 2)
        {
            audio.Play("KilnLight2", gameObject);
        }
        else if (randVal == 3)
        {
            audio.Play("KilnLight3", gameObject);
        }
    }

    private void OnDestroy()
    {
        obj.receiveEvent.RemoveListener(ReceiveItem);
    }
}
