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
        smelter.SetMaxTemperature(obj.obj.GetMaxTemperature());
        smelter.SetMintemperature(obj.obj.GetMintemperature());

        objSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        smelter.OnFinishedSmelting += OnFinishedSmelting;
        obj.inventory.OnItemListChanged += OnItemListChanged;
        smelter.ReplaceWood += ReplaceWood;
        logsToReplace = 0;
    }

    private void Update()
    {
        //obj.light.intensity = float(smelter.currentTemperature/)
        obj.light.pointLightOuterRadius = (float)smelter.currentFuel / smelter.maxFuel * 25;

        if (smelter.isClosed)
        {
            objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnCovered;
        }
        else if (smelter.isSmelting)
        {
            objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnLit;
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

    private void OnItemListChanged(object sender, System.EventArgs e)//change so kiln can only hold 5 items btw
    {
        if (!smelter.isClosed)//is open
        {
            if (obj.inventory.GetItemList().Count > 0)
            {
                Debug.Log("bam added");
                Item latestItem = obj.inventory.GetItemList().Last();

                if (latestItem.itemType == Item.ItemType.Clay)
                {
                    OnClosed?.Invoke(this, EventArgs.Empty);
                    smelter.isClosed = true;
                }
                else if (latestItem.isSmeltable())
                {
                    if (!isSmeltingItem)
                    {
                        isSmeltingItem = true;
                        smeltingItem = new Item { amount = latestItem.amount, itemType = latestItem.itemType};
                        originalSmeltItem = new Item { amount = latestItem.amount, itemType = latestItem.itemType };
                        smelter.isSmeltingItem = true;
                        if (smelter.isSmelting)
                        {
                            PlayRandomLightSound();
                        }
                        StartCoroutine(smelter.SmeltItem(smeltingItem, smeltingItem.GetSmeltReward()));
                    }
                }
                else if (latestItem.IsFuel())
                {
                    smelter.SetMaxFuel(obj.obj.GetMaxFuel());
                    if (obj.inventory.GetItemList().Count > 0)
                    {
                        smelter.AddFuel(latestItem.GetFuelValue());
                        smelter.SetTemperature(latestItem.GetTemperatureBurnValue());
                        if (smelter.isSmelting)
                        {
                            PlayRandomLightSound();
                        }
                        //Debug.Log(latestItem.itemType);

                        if (obj.inventory.GetItemList().Last().itemType == Item.ItemType.Log)//last item put into kiln turns into charcoal
                        {
                            logsToReplace++;
                        }
                        else
                        {
                            Debug.LogError("REMOVED");
                            obj.inventory.GetItemList().RemoveAt(obj.inventory.GetItemList().Count-1);
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
            foreach (Item _item in obj.inventory.GetItemList())
            {
                if (_item.itemType == originalSmeltItem.itemType)
                {
                    Debug.Log("ITEM SMELTED REMOVED AT "+i);
                    obj.inventory.GetItemList().RemoveAt(i);
                    break;
                }
                i++;
            }
            Debug.LogError("DROPPING NOT ERROR");
            Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
            RealItem newItem = RealItem.SpawnRealItem(transform.position, smeltingItem, true, false);
            newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);

            if (originalSmeltItem.IsBowl())
            {
                RealItem bowlItem = RealItem.SpawnRealItem(transform.position, new Item { itemType = Item.ItemType.ClayBowl, amount = 1 }, true, false);
                direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
                bowlItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);
            }
            if (originalSmeltItem.itemType == Item.ItemType.BronzeCrucible)
            {
                RealItem plateItem = RealItem.SpawnRealItem(transform.position, new Item { itemType = Item.ItemType.ClayPlate, amount = 1 }, true, false);
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
            if (originalSmeltItem.IsBowl())
            {
                obj.inventory.SimpleAddItem(new Item { itemType = Item.ItemType.ClayBowl, amount = 1 });
            }
            if (originalSmeltItem.itemType == Item.ItemType.BronzeCrucible)
            {
                obj.inventory.SimpleAddItem(new Item { itemType = Item.ItemType.ClayPlate, amount = 1 });
            }
        }

    }

    private void ReplaceWood(object sender, System.EventArgs e)
    {
        int i = 0;
        foreach (Item _item in obj.inventory.GetItemList())
        {
            if (_item.itemType == originalSmeltItem.itemType)
            {
                Debug.Log("ITEM SMELTED REMOVED AT " + i);
                obj.inventory.GetItemList().RemoveAt(i);
                break;
            }
            i++;
        }
        Debug.LogError("DROPPING NOT ERROR");
        Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
        RealItem newItem = RealItem.SpawnRealItem(transform.position, smeltingItem, true, false);
        newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);

        print(obj.inventory.GetItemList().Count);
        if (obj.inventory.GetItemList().Count != 0)
        {
            int logsReplaced = 0;
            i = 0;
            while (logsToReplace > 0)
            {
                foreach (Item _item in obj.inventory.GetItemList())
                {
                    if (_item.itemType == Item.ItemType.Log)
                    {
                        Debug.Log("INT I IS " + i);
                        obj.inventory.GetItemList().RemoveAt(i);
                        logsReplaced++;
                        logsToReplace--;
                        break;
                    }
                    i++;
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
                    obj.inventory.GetItemList().Add(new Item { amount = 1, itemType = Item.ItemType.Charcoal });
                    logsReplaced--;
                }
            }
            obj.inventory.DropAllItems(obj.transform.position);

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
