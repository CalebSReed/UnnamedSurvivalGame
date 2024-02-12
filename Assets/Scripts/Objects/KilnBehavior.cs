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

    private void Awake()
    {
        obj = gameObject.GetComponent<RealWorldObject>();
        obj.receiveEvent.AddListener(ReceiveItem);
        obj.hasSpecialInteraction = true;
        obj.interactEvent.AddListener(OnInteract);
        obj.hoverBehavior.SpecialCase = true;
        obj.hoverBehavior.specialCaseModifier.AddListener(CheckPlayerItems);

        gameObject.AddComponent<Smelter>();

        audio = obj.audio;

        smelter = gameObject.GetComponent<Smelter>();
        smelter.baseTemperature += obj.woso.baseTemp;
        smelter.SetMaxTemperature(obj.obj.woso.maxTemp);
        smelter.SetMintemperature(obj.obj.woso.minTemp);
        smelter.OnFinishedSmelting += OnFinishedSmelting;
        smelter.ReplaceWood += ReplaceWood;

        objSpriteRenderer = obj.spriteRenderer;

        //obj.inventory.OnItemListChanged += OnItemListChanged;
        logsToReplace = 0;
        if (GetComponent<TemperatureEmitter>() != null)
        {
            GetComponent<TemperatureEmitter>().StopAllCoroutines();
        }

        obj.onLoaded += OnLoad;
        obj.onSaved += OnSave;
    }

    private void CheckPlayerItems()
    {
        obj.hoverBehavior.Name = obj.woso.objName;
        if (obj.playerMain.isHoldingItem && IsValidKilnItem())
        {
            if (obj.playerMain.heldItem.itemSO.isFuel)
            {
                obj.hoverBehavior.Name = "";
                obj.hoverBehavior.Prefix = "LMB: Add Fuel";
            }
            else if (obj.playerMain.heldItem.itemSO.isSmeltable && smelter.isSmelting)
            {
                obj.hoverBehavior.Name = "";
                obj.hoverBehavior.Prefix = $"LMB: Smelt {obj.playerMain.heldItem}";
            }
            else if (obj.playerMain.heldItem.itemSO.doActionType == Action.ActionType.Burn)
            {
                obj.hoverBehavior.Name = obj.woso.objName;
                obj.hoverBehavior.Prefix = "LMB: Light ";
            }
            else
            {
                obj.hoverBehavior.Name = obj.woso.objName;
                obj.hoverBehavior.Prefix = "LMB: Seal ";
            }
        }
        else if (obj.playerMain.hasTongs && obj.playerMain.equippedHandItem.containedItem != null)
        {
            Item kilnItem = obj.playerMain.equippedHandItem.containedItem;
            if (IsValidKilnItem(kilnItem) && smelter.isSmelting)
            {
                obj.hoverBehavior.Prefix = "RMB: Smelt ";
                obj.hoverBehavior.Name = kilnItem.itemSO.itemName;
            }
        }
        else if (obj.playerMain.doAction == Action.ActionType.Burn && !smelter.isSmelting && smelter.currentFuel > 0)
        {
            obj.hoverBehavior.Prefix = "RMB: Light ";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
        else if (obj.woso.objType == "brickkiln" && smelter.isSmelting && !smelter.isClosed)
        {
            obj.hoverBehavior.Prefix = "RMB: Shut ";
        }
        else
        {
            obj.hoverBehavior.Name = obj.woso.objName;
            obj.hoverBehavior.Prefix = "";
        }
    }

    private void Update()
    {
        if (smelter.isSmelting)
        {
            EmitLight();
        }
        else
        {
            obj.light.intensity = 0;
        }

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
                if (smelter.currentTemperature >= 1085)
                {
                    objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnSmelting;
                }
                else if (smelter.currentTemperature >= 760)
                {
                    objSpriteRenderer.sprite = WorldObject_Assets.Instance.kilnlvl2;
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
        else if (obj.woso.objType == "brickkiln")
        {
            if (smelter.isSmelting)
            {
                if (smelter.isClosed)
                {
                    if (smelter.currentTemperature >= 1538)
                    {
                        objSpriteRenderer.sprite = WorldObject_Assets.Instance.brickkilnlvl3;
                    }
                    else if (smelter.currentTemperature >= 1085)
                    {
                        objSpriteRenderer.sprite = WorldObject_Assets.Instance.brickkilnlvl2;
                    }
                    else
                    {
                        objSpriteRenderer.sprite = WorldObject_Assets.Instance.brickkilnclosed;
                    }
                }
                else
                {
                    if (smelter.currentTemperature >= 1538)
                    {
                        objSpriteRenderer.sprite = WorldObject_Assets.Instance.brickkilnopenlvl3;
                    }
                    else if (smelter.currentTemperature >= 1085)
                    {
                        objSpriteRenderer.sprite = WorldObject_Assets.Instance.brickkilnopenlvl2;
                    }
                    else if (smelter.currentTemperature >= 760)
                    {
                        objSpriteRenderer.sprite = WorldObject_Assets.Instance.brickkilnopenlvl1;
                    }
                    else
                    {
                        objSpriteRenderer.sprite = WorldObject_Assets.Instance.brickkilnopenlvl0;
                    }
                }
            }
            else if (smelter.currentFuel > 0)
            {
                objSpriteRenderer.sprite = WorldObject_Assets.Instance.brickkilnfueled;
            }
            else
            {
                objSpriteRenderer.sprite = obj.woso.objSprite;
            }
        }
    }

    private void OnInteract()
    {
        if (obj.playerMain.isHoldingItem)
        {
            ReceiveItem();
        }
        else if (obj.woso.objType == "Kiln" || obj.woso.objType == "brickkiln")
        {
            if (obj.playerMain.isHandItemEquipped && obj.playerMain.equippedHandItem.containedItem != null && IsValidKilnItem(obj.playerMain.equippedHandItem.containedItem.itemSO))
            {
                Item validItem = obj.playerMain.equippedHandItem.containedItem;
                if (validItem.itemSO.isReheatable && validItem.itemSO.smeltValue <= smelter.currentTemperature)
                {
                    PlayRandomLightSound();

                    if (obj.playerMain.equippedHandItem.containedItem.hotRoutine != null)
                    {
                        StopCoroutine(obj.playerMain.equippedHandItem.containedItem.hotRoutine);
                    }

                    obj.playerMain.equippedHandItem.containedItem.hotRoutine = StartCoroutine(obj.playerMain.equippedHandItem.containedItem.BecomeHot());
                }
                else if (validItem.itemSO.isSmeltable && smelter.currentTemperature >= validItem.itemSO.smeltValue)
                {
                    if (obj.playerMain.equippedHandItem.containedItem.itemSO.isBowl)
                    {
                        var newItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 };
                        var realItem = RealItem.SpawnRealItem(transform.position, newItem, true, false, 0, false, true, true);
                        CalebUtils.RandomDirForceNoYAxis3D(realItem.GetComponent<Rigidbody>(), 5f);
                    }
                    if (obj.playerMain.equippedHandItem.containedItem.itemSO.isPlate)
                    {
                        var newItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 1 };
                        var realItem = RealItem.SpawnRealItem(transform.position, newItem, true, false, 0, false, true, true);
                        CalebUtils.RandomDirForceNoYAxis3D(realItem.GetComponent<Rigidbody>(), 5f);
                    }
                    PlayRandomLightSound();
                    obj.playerMain.equippedHandItem.containedItem.itemSO = validItem.itemSO.smeltReward;

                    if (obj.playerMain.equippedHandItem.containedItem.hotRoutine != null)
                    {
                        StopCoroutine(obj.playerMain.equippedHandItem.containedItem.hotRoutine);
                    }

                    obj.playerMain.equippedHandItem.containedItem.hotRoutine = StartCoroutine(obj.playerMain.equippedHandItem.containedItem.BecomeHot());
                    obj.playerMain.UpdateContainedItem(obj.playerMain.equippedHandItem.containedItem);
                    obj.playerMain.inventory.RefreshInventory();
                }
            }
            else if (obj.playerMain.isHandItemEquipped && obj.playerMain.doAction == Action.ActionType.Burn && !smelter.isSmelting && smelter.currentFuel > 0)
            {
                LightKiln();
            }
            else if (obj.woso.objType == "brickkiln" && !smelter.isClosed && smelter.isSmelting)//not just if hands are free, dont want to unequip ur stuff to close this bitch everytime huh?
            {
                smelter.isClosed = true;
                obj.saveData.isOpen = !smelter.isClosed;
            }
        }
    }

    public void ReceiveItem()//change so kiln can only hold 5 items btw
    {
        var _item = obj.playerMain.heldItem;

        if (!smelter.isClosed)//if we return that means action failed. if not, at the very end we will use player item
        {
            Debug.Log("bam added");
            if (_item.itemSO == ItemObjectArray.Instance.SearchItemList("Clay") && smelter.isSmelting && obj.woso.objType == "Kiln")
            {
                OnClosed?.Invoke(this, EventArgs.Empty);
                smelter.isClosed = true;
                obj.saveData.isOpen = !smelter.isClosed;
            }
            else if (obj.woso.hasAttachments && _item.itemSO == obj.woso.itemAttachments[0])
            {
                obj.AttachItem(_item);
                return;
            }
            else if (_item.itemSO.isSmeltable && obj.woso.objType != "Kiln" && obj.woso.objType != "brickkiln")
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
            else if (_item.itemSO.isFuel && smelter.currentFuel < obj.woso.maxFuel)//OH maybe check if this was an accepted fuel by the woso!
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
                    obj.saveData.invItemTypes.Add("Log");
                    logsToReplace++;
                }
            }
            else if (_item.itemSO.doActionType == Action.ActionType.Burn && !smelter.isSmelting && smelter.currentFuel > 0)//if burns, not already active, and has fuel
            {
                LightKiln();//yes we finally using durability on torches
            }
            else//none applies, return
            {
                return;
            }

        }
        else if (smelter.isClosed && _item.itemSO == obj.woso.itemAttachments[0])
        {
            obj.AttachItem(_item);
            return;
        }
        else if (smelter.isClosed)
        {
            return;//if closed, do nothing.
        }

        obj.playerMain.UseHeldItem();
    }

    private bool IsValidKilnItem()
    {
        if (obj.playerMain.heldItem.itemSO.doActionType == Action.ActionType.Burn)
        {
            return true;
        }
        if (obj.playerMain.heldItem.itemSO.itemType == "Clay" && obj.woso.objType == "Kiln")
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
        if (obj.playerMain.heldItem.itemSO.isFuel)
        {
            return true;
        }
        return false;
    }

    private bool IsValidKilnItem(Item item)
    {
        if (item.itemSO.doActionType == Action.ActionType.Burn)
        {
            return true;
        }
        if (item.itemSO.itemType == "Clay" && obj.woso.objType == "Kiln")
        {
            return true;
        }
        foreach (ItemSO _item in obj.woso.acceptableSmeltItems)
        {
            if (_item == item.itemSO)
            {
                return true;
            }
        }
        if (item.itemSO.isFuel)
        {
            return true;
        }
        return false;
    }

    private bool IsValidKilnItem(ItemSO item)
    {
        if (item.isReheatable)
        {
            return true;
        }
        if (item.itemType == "Clay" && obj.woso.objType == "Kiln")
        {
            return true;
        }
        foreach (ItemSO _item in obj.woso.acceptableSmeltItems)
        {
            if (_item == item)
            {
                return true;
            }
        }
        foreach (ItemSO _item in obj.woso.acceptableFuels)
        {
            if (_item == item)
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

            if (originalSmeltItem.itemSO.isBowl && !smeltingItemReward.itemSO.isBowl)
            {
                RealItem bowlItem = RealItem.SpawnRealItem(transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 }, true, false, 0, false, true, true);
                CalebUtils.RandomDirForceNoYAxis3D(bowlItem.GetComponent<Rigidbody>(), 5);
            }
            if (originalSmeltItem.itemSO.isPlate && !smeltingItemReward.itemSO.isPlate)//drops plate if is crucible and not if they are food (food is always on plate)
            {
                RealItem plateItem = RealItem.SpawnRealItem(transform.position, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 1 }, true, false, 0, false, true, true);
                CalebUtils.RandomDirForceNoYAxis3D(plateItem.GetComponent<Rigidbody>(), 5);
            }
            originalSmeltItem = null;
            smeltingItemReward = null;
            smelter.isSmeltingItem = false;
            audio.Play("KilnOut", transform.position, gameObject);
        }
        else
        {
            obj.inventory.SetValue(smeltingItemReward);
            if (originalSmeltItem.itemSO.isBowl && !smeltingItemReward.itemSO.isBowl)
            {
                obj.inventory.SetValue(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 });
            }
            if (originalSmeltItem.itemSO.isPlate && !smeltingItemReward.itemSO.isPlate)
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
            obj.inventory.SetValue(new Item { amount = 1, itemSO = ItemObjectArray.Instance.SearchItemList("Clay") });
        }

        obj.inventory.DropAllItems(obj.transform.position, false, true);
        obj.inventory.ClearArray();

        originalSmeltItem = null;
        smeltingItemReward = null;
        smelter.isSmeltingItem = false;
        smelter.isClosed = false;
        obj.saveData.isOpen = !smelter.isClosed;
        OnOpened?.Invoke(this, EventArgs.Empty);
        audio.Stop("KilnRunning");
        audio.Play("KilnOut", transform.position, gameObject);
        logsToReplace = 0;
        GetComponent<TemperatureEmitter>().StopAllCoroutines();
    }

    public void LightKiln()
    {
        if (smelter.currentFuel > 0 && !smelter.isSmelting)
        {
            smelter.StartSmelting();
            StartCoroutine(GetComponent<TemperatureEmitter>().EmitTemperature());
            PlayRandomLightSound();
            audio.Play("KilnRunning", transform.position, gameObject);
        }
    }

    private void EmitLight()
    {
        obj.light.intensity = ((float)smelter.currentFuel / (float)smelter.maxFuel * 100) + 10;
    }

    public void PlayRandomLightSound()
    {
        int randVal = Random.Range(1, 4);
        if (randVal == 1)
        {
            audio.Play("KilnLight1", transform.position, gameObject);
        }
        else if (randVal == 2)
        {
            audio.Play("KilnLight2", transform.position, gameObject);
        }
        else if (randVal == 3)
        {
            audio.Play("KilnLight3", transform.position, gameObject);
        }
    }

    private void OnSave(object sender, System.EventArgs e)
    {
        obj.saveData.currentFuel = smelter.currentFuel;
        obj.saveData.currentTemp = smelter.currentTemperature;
        obj.saveData.isOpen = !smelter.isClosed;
        obj.saveData.temperatureTarget = smelter.targetTemperature;
        obj.saveData.timerProgress = smelter.smeltingProgress;

        if (originalSmeltItem != null)
        {
            obj.saveData.heldItemType = originalSmeltItem.itemSO.itemType;
        }
        else
        {
            obj.saveData.heldItemType = null;
        }
    }

    private void OnLoad(object sender, System.EventArgs e)
    {
        smelter.SetMaxFuel(obj.woso.maxFuel);
        smelter.SetMintemperature(obj.obj.woso.minTemp);
        smelter.targetTemperature = obj.saveData.temperatureTarget;
        smelter.currentFuel = obj.saveData.currentFuel;
        smelter.currentTemperature = obj.saveData.currentTemp;
        smelter.isClosed = !obj.saveData.isOpen;
        if (obj.saveData.currentTemp > 0)
        {
            LightKiln();
        }

        if (obj.saveData.heldItemType != null)
        {
            originalSmeltItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(obj.saveData.heldItemType), amount = 1 };
            smeltingItemReward = new Item { itemSO = originalSmeltItem.itemSO.smeltReward, amount = 1 };
            smelter.isSmeltingItem = true;
            smelter.smeltingProgress = obj.saveData.timerProgress;
            StartCoroutine(smelter.SmeltItem(originalSmeltItem));
        }

        foreach (var log in obj.saveData.invItemTypes)
        {
            logsToReplace++;
        }
    }

    private void OnDestroy()
    {
        audio.Stop("KilnRunning");
        obj.receiveEvent.RemoveListener(ReceiveItem);
        obj.hoverBehavior.specialCaseModifier.RemoveListener(CheckPlayerItems);
    }
}
