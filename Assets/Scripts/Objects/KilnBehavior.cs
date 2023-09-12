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

    public bool isSmeltingItem = false;
    public Item smeltingItem = null;
    public Item originalSmeltItem = null;
    public int logsToReplace = 0;

    public event EventHandler OnClosed;
    public event EventHandler OnOpened;

    private AudioManager audio;

    private void Start()//kilns like this can break, make a brick version that will last
    {
        obj = gameObject.GetComponent<RealWorldObject>();

        gameObject.AddComponent<Smelter>();

        audio = FindObjectOfType<AudioManager>();

        smelter = gameObject.GetComponent<Smelter>();
        smelter.SetMaxTemperature(obj.obj.woso.maxTemp);
        smelter.SetMintemperature(obj.obj.woso.minTemp);

        objSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        smelter.OnFinishedSmelting += OnFinishedSmelting;
        obj.inventory.OnItemListChanged += OnItemListChanged;
        smelter.ReplaceWood += ReplaceWood;
        logsToReplace = 0;
    }

    private void Update()
    {
        //obj.light.intensity = float(smelter.currentTemperature/)

        if (obj.obj.woso.objType == "Kiln")
        {
            obj.light.pointLightOuterRadius = (float)smelter.currentFuel / smelter.maxFuel * 25;

            if (smelter.isClosed)
            {
                objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnCovered;
            }
            else if (smelter.isSmelting)
            {
                if (smeltingItem != null)
                {
                    if (smelter.currentTemperature >= smeltingItem.itemSO.smeltValue)
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

    private void OnItemListChanged(object sender, System.EventArgs e)//change so kiln can only hold 5 items btw
    {
        if (!smelter.isClosed)//is open
        {
            if (obj.inventory.ItemCount() > 0)
            {
                Debug.Log("bam added");
                Item latestItem = obj.inventory.GetItemList()[obj.inventory.LastItem()];

                if (latestItem.itemSO == ItemObjectArray.Instance.SearchItemList("Clay"))
                {
                    OnClosed?.Invoke(this, EventArgs.Empty);
                    smelter.isClosed = true;
                }
                else if (latestItem.itemSO.isSmeltable)
                {
                    if (!isSmeltingItem)
                    {
                        isSmeltingItem = true;
                        smeltingItem = new Item { amount = latestItem.amount, itemSO = latestItem.itemSO};//OHHHH This is a new item outside of inventory
                        originalSmeltItem = new Item { amount = latestItem.amount, itemSO = latestItem.itemSO };
                        Debug.Log("ORIGINAL SMELT ITEM SET");
                        smelter.isSmeltingItem = true;
                        if (smelter.isSmelting)
                        {
                            PlayRandomLightSound();
                        }
                        StartCoroutine(smelter.SmeltItem(smeltingItem, smeltingItem.itemSO.smeltReward));
                    }
                }
                else if (latestItem.itemSO.isFuel)
                {
                    smelter.SetMaxFuel(obj.obj.woso.maxFuel);
                    if (obj.inventory.ItemCount() > 0)
                    {
                        smelter.AddFuel(latestItem.itemSO.fuelValue);
                        smelter.SetTemperature(latestItem.itemSO.temperatureBurnValue);
                        if (smelter.isSmelting)
                        {
                            PlayRandomLightSound();
                        }
                        //Debug.Log(latestItem.itemType);

                        if (obj.inventory.GetItemList()[obj.inventory.LastItem()].itemSO == ItemObjectArray.Instance.SearchItemList("Log"))//last item put into kiln turns into charcoal
                        {
                            logsToReplace++;
                        }
                        else
                        {
                            Debug.LogError("REMOVED");
                            obj.inventory.SetNull(obj.inventory.LastItem());
                        }
                    }
                }
            }
        }
    }

    private void OnFinishedSmelting(object sender, System.EventArgs e)
    {
        if (!smelter.isClosed)
        {
            int i = 0;
            for (i = 0; i < obj.inventory.GetItemList().Length; i++)
            {
                if (obj.inventory.GetItemList()[i] != null)
                {
                    if (obj.inventory.GetItemList()[i].itemSO.itemType == originalSmeltItem.itemSO.itemType)
                    {
                        Debug.Log("ITEM SMELTED REMOVED AT " + i);
                        obj.inventory.SetNull(i);
                        break;
                    }
                }
            }
            Debug.LogError("DROPPING NOT ERROR");
            Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
            RealItem newItem = RealItem.SpawnRealItem(transform.position, smeltingItem, true, false, 0, smeltingItem.itemSO.needsToBeHot);
            newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);

            if (originalSmeltItem.itemSO.isBowl)
            {
                RealItem bowlItem = RealItem.SpawnRealItem(transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 }, true, false);
                direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
                bowlItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
            }
            if (originalSmeltItem.itemSO == ItemObjectArray.Instance.SearchItemList("BronzeCrucible"))
            {
                RealItem plateItem = RealItem.SpawnRealItem(transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 1 }, true, false);
                direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
                plateItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
            }
            originalSmeltItem = null;
            smeltingItem = null;
            smelter.isSmeltingItem = false;
            isSmeltingItem = false;
            audio.Play("KilnOut");
        }
        else
        {
            if (originalSmeltItem.itemSO.isBowl)
            {
                obj.inventory.SimpleAddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 });
            }
            if (originalSmeltItem.itemSO.isPlate)
            {
                obj.inventory.SimpleAddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 1 });
            }
        }

    }

    private void ReplaceWood(object sender, System.EventArgs e)
    {
        int i = 0;

        if (originalSmeltItem != null)
        {
            for (i = 0; i < obj.inventory.GetItemList().Length; i++)
            {
                if (obj.inventory.GetItemList()[i] != null)
                {
                    if (obj.inventory.GetItemList()[i].itemSO == originalSmeltItem.itemSO)
                    {
                        Debug.Log("ITEM SMELTED REMOVED AT " + i);
                        obj.inventory.SetNull(i);
                        break;
                    }
                }
            }
            Debug.LogError("DROPPING NOT ERROR");
            Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
            RealItem newItem = RealItem.SpawnRealItem(transform.position, smeltingItem, true, false, 0, smeltingItem.itemSO.needsToBeHot);
            newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
        }


        //print(obj.inventory.GetItemList().Count());
        if (obj.inventory.ItemCount() != 0)
        {
            int logsReplaced = 0;
            i = 0;
            while (logsToReplace > 0)
            {
                for (i = 0; i < obj.inventory.GetItemList().Length; i++)
                {
                    if (obj.inventory.GetItemList()[i] != null)
                    {
                        if (obj.inventory.GetItemList()[i].itemSO == ItemObjectArray.Instance.SearchItemList("Log"))
                        {
                            Debug.Log("INT I IS " + i);
                            obj.inventory.SetNull(i);
                            logsReplaced++;
                            logsToReplace--;
                            break;
                        }
                    }
                }
            }

            /*foreach (Item _item in obj.inventory.GetItemList())
            {
                if (_item == originalSmeltItem)
                {
                    obj.inventory.GetItemList().RemoveAt(i);
                    break;
                }
                i++;
            }*/

            if (smelter.isClosed)
            {
                while (logsReplaced > 0)
                {
                    obj.inventory.SetValue((new Item { amount = 1, itemSO = ItemObjectArray.Instance.SearchItemList("Charcoal") }));
                    logsReplaced--;
                }
            }
            obj.inventory.DropAllItems(obj.transform.position);
            obj.inventory.ClearArray();

            originalSmeltItem = null;
            smeltingItem = null;
            isSmeltingItem = false;
            smelter.isSmeltingItem = false;
            smelter.isClosed = false;
            OnOpened?.Invoke(this, EventArgs.Empty);
        }
        audio.Stop("KilnRunning");
        audio.Play("KilnOut");
    }

    public void LightKiln()
    {
        if (smelter.currentFuel > 0 && !smelter.isSmelting)
        {
            smelter.StartSmelting();
            PlayRandomLightSound();
            audio.Play("KilnRunning");
        }
    }

    public void PlayRandomLightSound()
    {
        int randVal = Random.Range(1, 4);
        if (randVal == 1)
        {
            audio.Play("KilnLight1");
        }
        else if (randVal == 2)
        {
            audio.Play("KilnLight2");
        }
        else if (randVal == 3)
        {
            audio.Play("KilnLight3");
        }
    }
}
