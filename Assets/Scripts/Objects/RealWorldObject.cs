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
    private List<Item> lootTable;
    public Item.ItemType[] acceptedFuelItems;
    //public bool isCooking;

    //public event EventHandler OnObjectClicked;

    public Action action;
    public Action.ActionType objectAction;
    public int actionsLeft;
    public bool isClosed = false;
    public WorldObject.worldObjectType objType { get; private set; }


    public static RealWorldObject SpawnWorldObject(Vector3 position, WorldObject worldObject)
    {
        Transform transform = Instantiate(WorldObject_Assets.Instance.pfWorldObjectSpawner, position, Quaternion.identity);
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
        objType = obj.objType;
        objectAction = obj.GetAction();
        actionsLeft = obj.GetMaxActionUses();
        inventory = new Inventory();
        lootTable = obj.GetLootTable();
        acceptedFuelItems = obj.GetAcceptableFuelGiven();
        //inventory.AddLootItems(lootTable);
        spriteRenderer.sprite = obj.GetSprite();
        SetObjectComponent();
        if (obj.IsBurnable())
        {
            light.intensity = obj.GetLightIntensity();
            StartCoroutine(Burn());
        }

        if (obj.IsInteractable())
        {
            SubscribeToEvent();
        }
    }

    public void SubscribeToEvent()
    {
        if (obj.objType == WorldObject.worldObjectType.Kiln)
        {
            KilnBehavior kiln = GetComponent<KilnBehavior>();
            kiln.OnClosed += OnClosed;
            kiln.OnOpened += OnOpened;
        }
        else if (obj.objType == WorldObject.worldObjectType.HotCoals)
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
        storedItemRenderer.sprite = _item.GetSprite();
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
        switch (objType)
        {
            default: return null;
            case WorldObject.worldObjectType.Kiln: return gameObject.AddComponent<KilnBehavior>();
            case WorldObject.worldObjectType.HotCoals: return gameObject.AddComponent<HotCoalsBehavior>();
        }
    }

    public void CheckBroken()
    {
        if (actionsLeft <= 0)
        {
            if (obj.WillTransition())
            {
                SpawnWorldObject(transform.position, new WorldObject { objType = obj.ObjectTransition() });
                txt.text = "";
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("poo");
                inventory.DropAllItems(gameObject.transform.position);
                inventory.AddLootItems(lootTable);//add them now so we can change sprite when not empty
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
        light.pointLightOuterRadius -= light.pointLightOuterRadius / obj.GetMaxActionUses();
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
            txt.text = obj.GetAction().ToString();
        }
        else if (playerMain.isHoldingItem && objectAction == Action.ActionType.Cook)
        {
            if (playerMain.heldItem.IsCookable() && !GetComponent<HotCoalsBehavior>().isCooking)
            {
                txt.text = obj.GetAction().ToString();
            }
            else
            {
                txt.text = objType.ToString();
            }
        }
        else if (playerMain.isHoldingItem && objType == WorldObject.worldObjectType.Kiln)
        {
            if (IsSmeltingItem())
            {
                txt.text = "Smelt";
            }
            else if (IsFuelItem())
            {
                txt.text = "Add Fuel";
            }
            else if (playerMain.heldItem.itemType == Item.ItemType.Clay)
            {
                txt.text = "Seal";
            }
            else
            {
                txt.text = "";
            }
        }
        else if (objectAction == 0 && !playerMain.isAiming)
        {
            txt.text = $"Pick {objType}";
        }
        else
        {
            txt.text = objType.ToString();//convert to proper name with new function    
        }             
    }

    private bool IsSmeltingItem()
    {
        foreach (Item.ItemType _itemType in obj.GetAcceptableSmeltingItems())
        {
            if (_itemType == playerMain.heldItem.itemType)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsFuelItem()
    {
        foreach (Item.ItemType _itemType in obj.GetAcceptableFuelGiven())
        {
            if (_itemType == playerMain.heldItem.itemType)
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
