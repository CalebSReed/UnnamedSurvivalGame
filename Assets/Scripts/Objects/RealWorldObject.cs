using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Experimental.Rendering.Universal;

public class RealWorldObject : MonoBehaviour
{
    private GameObject player;
    private PlayerMain playerMain;
    private TextMeshProUGUI txt;
    private GameObject mouse;
    public Light2D light;

    public Inventory inventory;
    private List<ItemSO> lootTable;
    private List<int> lootAmounts;
    private List<int> lootChances;

    public ItemSO[] acceptedFuelItems;
    //public bool isCooking;

    //public event EventHandler OnObjectClicked;

    public Action action;
    public Action.ActionType objectAction;
    public float actionsLeft;
    public bool isClosed = false;
    public WorldObject.worldObjectType objType { get; private set; }


    public static RealWorldObject SpawnWorldObject(Vector3 position, WorldObject worldObject)
    {
        Transform transform = Instantiate(WosoArray.Instance.pfWorldObject, position, Quaternion.identity);
        RealWorldObject realWorldObj = transform.GetComponent<RealWorldObject>();
        realWorldObj.SetObject(worldObject);
        //change polygon collider to fit sprite
        return realWorldObj;
    }

    public WorldObject obj;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer storedItemRenderer;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mouse = GameObject.FindGameObjectWithTag("Mouse");
        playerMain = player.GetComponent<PlayerMain>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        storedItemRenderer = this.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        storedItemRenderer.sprite = null;
        txt = mouse.GetComponentInChildren<TextMeshProUGUI>();
        light = GetComponent<Light2D>();
        light.intensity = 0;
        //gameObject.GetComponent<MonoBehaviour>().enabled = false; shit dont work AND lags the game bruh
        //gameObject.GetComponent<CircleCollider2D>().enabled = false;
    }

    public void SetObject(WorldObject obj)
    {
        this.obj = obj;
        //objType = obj.objType;
        objectAction = obj.woso.objAction;
        actionsLeft = obj.woso.maxUses;
        inventory = new Inventory();
        lootTable = obj.woso.lootTable;
        lootAmounts = obj.woso.lootAmounts;
        lootChances = obj.woso.lootChances;
        acceptedFuelItems = obj.woso.acceptableFuels;
        //inventory.AddLootItems(lootTable, lootAmounts, lootChances);
        spriteRenderer.sprite = obj.woso.objSprite;
        SetObjectComponent();
        if (obj.woso.burns)
        {
            light.intensity = obj.woso.lightRadius;
            StartCoroutine(Burn());
        }

        if (obj.woso.isInteractable)
        {
            SubscribeToEvent();
        }
    }

    public void SubscribeToEvent()
    {
        if (obj.woso == WosoArray.Instance.Kiln)
        {
            KilnBehavior kiln = GetComponent<KilnBehavior>();
            kiln.OnClosed += OnClosed;
            kiln.OnOpened += OnOpened;
        }
        else if (obj.woso == WosoArray.Instance.HotCoals)
        {
            HotCoalsBehavior hotCoals = GetComponent<HotCoalsBehavior>();
            hotCoals.OnFinishedCooking += UpdateStoredItemSprite;
        }
    }

    private void UpdateStoredItemSprite(object sender, System.EventArgs e)
    {
        storedItemRenderer.sprite = null;
    }

    public void Cook(Item _item)
    {
        GetComponent<HotCoalsBehavior>().StartCooking(_item, inventory);
        storedItemRenderer.sprite = _item.itemSO.itemSprite;
    }

    public void OnOpened(object sender, System.EventArgs e)
    {
        isClosed = false;
    }
    
    public void OnClosed(object sender, System.EventArgs e)
    {
        isClosed = true;
    }

    public Component SetObjectComponent()
    {
        if (obj.woso == WosoArray.Instance.Kiln)
        {
            return gameObject.AddComponent<KilnBehavior>();
        }
        else if (obj.woso == WosoArray.Instance.HotCoals)
        {
            return gameObject.AddComponent<HotCoalsBehavior>();
        }

        return null;
    }

    public void CheckBroken()
    {
        if (actionsLeft <= 0)
        {
            if (obj.woso.willTransition)
            {
                SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransition });
                txt.text = "";
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("poo");
                inventory.DropAllItems(gameObject.transform.position);
                inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                inventory.DropAllItems(gameObject.transform.position);
                txt.text = "";
                Destroy(gameObject);
            }
        }
    }

    public void StartSmelting()
    {
        GetComponent<KilnBehavior>().LightKiln();
    }

    private IEnumerator Burn()
    {
        yield return new WaitForSeconds(1f);
        actionsLeft--;
        light.pointLightOuterRadius -= light.pointLightOuterRadius / obj.woso.maxUses;
        //Debug.Log(light.intensity.ToString());
        CheckBroken();
        StartCoroutine(Burn());
    }

    public void GetActionedOn()
    {
        actionsLeft -= 1;
        Debug.Log(actionsLeft);
        CheckBroken();
    }

    public void OnMouseDown() //FOR THESE MOUSE EVENTS ENTITIES WITH COLLIDERS AS VISION ARE SET TO IGNORE RAYCAST LAYER SO THEY ARENT CLICKABLE BY MOUSE, CHANGE IF WE WANT TO CHANGE THAT??
    {
        Debug.Log("i was clicked lol"); 
        player.GetComponent<PlayerMain>().OnObjectSelected(objectAction, this.transform, obj, gameObject); 
    }

    public void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject())//this is so goddamn convoluted wtf refactor all of this PLEASE
        {
            return;
        }
        if (playerMain.doAction == objectAction && objectAction != 0)
        {
            txt.text = obj.woso.objAction.ToString();
        }
        else if (playerMain.isHoldingItem && objectAction == Action.ActionType.Cook)
        {
            if (playerMain.heldItem.itemSO.isCookable && !GetComponent<HotCoalsBehavior>().isCooking)
            {
                txt.text = obj.woso.objAction.ToString();
            }
            else
            {
                txt.text = obj.woso.objType.ToString();
            }
        }
        else if (playerMain.isHoldingItem && obj.woso == WosoArray.Instance.Kiln)
        {
            if (IsSmeltingItem())
            {
                txt.text = "Smelt";
            }
            else if (IsFuelItem())
            {
                txt.text = "Add Fuel";
            }
            else if (playerMain.heldItem.itemSO.itemType == ItemObjectArray.Instance.Clay.itemType)
            {
                txt.text = "Seal";
            }
            else
            {
                txt.text = $"{obj.woso.objType}";
            }
        }
        else if (objectAction == 0 && !playerMain.isAiming)
        {
            txt.text = $"Pick {obj.woso.objType}";
        }
        else if (obj.woso.isInteractable && playerMain.doAction == Action.ActionType.Burn && obj.woso == WosoArray.Instance.Kiln)
        {
            if (!GetComponent<Smelter>().isSmelting && GetComponent<Smelter>().currentFuel > 0)
            {
                txt.text = $"Light {obj.woso.objType}";
            }
            else
            {
                txt.text = $"{obj.woso.objType}";
            }
        }
        else
        {
            txt.text = objType.ToString();//convert to proper name with new function    
        }             
    }

    private bool IsSmeltingItem()
    {
        foreach (ItemSO _itemType in obj.woso.acceptableSmeltItems)
        {
            if (_itemType.itemType == playerMain.heldItem.itemSO.itemType)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsFuelItem()
    {
        foreach (ItemSO _itemType in obj.woso.acceptableFuels)
        {
            if (_itemType.itemType == playerMain.heldItem.itemSO.itemType)
            {
                return true;
            }
        }
        return false;
    }

    public void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        txt.text = "";
    }
}
