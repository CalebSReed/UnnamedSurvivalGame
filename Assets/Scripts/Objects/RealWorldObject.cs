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
    private GameManager gameManager;
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
    private bool containerOpen = false;
    private bool loaded = false;
    public Component objComponent;

    private HomeArrow hArrow;

    private WorldGeneration world;

    public WorldObject.worldObjectType objType { get; private set; }

    private UI_Inventory uiInv;

    public static RealWorldObject SpawnWorldObject(Vector3 position, WorldObject worldObject, bool _loaded = false, float _loadedUses = 0)
    {
        Transform transform = Instantiate(WosoArray.Instance.pfWorldObject, position, Quaternion.identity);
        RealWorldObject realWorldObj = transform.GetComponent<RealWorldObject>();
        realWorldObj.SetObject(worldObject);
        //change polygon collider to fit sprite
        if (_loaded == true)
        {
            realWorldObj.actionsLeft = _loadedUses;
        }
        else
        {
            realWorldObj.actionsLeft = worldObject.woso.maxUses;
        }
        return realWorldObj;
    }

    public WorldObject obj;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer storedItemRenderer;
    private GameObject attachmentObj;

    private void Awake()
    {
        world = GameObject.FindGameObjectWithTag("World").GetComponent<WorldGeneration>();
        //hArrow = GameObject.FindGameObjectWithTag("Home").GetComponent<HomeArrow>();
        //hArrow.gameObject.SetActive(false);
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        //gameManager.onLoad += ClearObject;
        if (GetComponent<Collider2D>().IsTouchingLayers(10))
        {
            Debug.LogError("TOO CLOSE TO THIS BITCH GOTTA GO GUYS");
            Destroy(gameObject);
        }



        player = GameObject.FindGameObjectWithTag("Player");
        mouse = GameObject.FindGameObjectWithTag("Mouse");
        playerMain = player.GetComponent<PlayerMain>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        attachmentObj = this.gameObject.transform.GetChild(0).gameObject;
        attachmentObj.GetComponent<SpriteRenderer>().sprite = null;
        attachmentObj.SetActive(false);

        storedItemRenderer = this.gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>();
        storedItemRenderer.sprite = null;

        transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = null;//plant sprite

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
        //actionsLeft = obj.woso.maxUses;
        if (this.obj.woso.isContainer)
        {
            inventory = new Inventory(9);
            uiInv = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().chestUI.GetComponent<UI_Inventory>();
            //OpenContainer();
        }
        else
        {
            inventory = new Inventory(64);
        }

        lootTable = obj.woso.lootTable;
        lootAmounts = obj.woso.lootAmounts;
        lootChances = obj.woso.lootChances;
        acceptedFuelItems = obj.woso.acceptableFuels;
        //inventory.AddLootItems(lootTable, lootAmounts, lootChances);
        spriteRenderer.sprite = obj.woso.objSprite;
        //SetObjectComponent();
        SetObjectHitBox();
        objComponent = SetObjectComponent();
        if (obj.woso.burns)
        {
            light.intensity = obj.woso.lightRadius;
            StartCoroutine(Burn());
        }

        if (obj.woso.isInteractable)
        {
            SubscribeToEvent();
        }

        if (this.obj.woso == WosoArray.Instance.SearchWOSOList("DirtBeacon"))
        {
            playerMain.SetBeacon(this);
        }
        
        if (!obj.woso.isCollidable)
        {
            Destroy(GetComponent<CircleCollider2D>());
        }
    }

    public void SetObjectHitBox()
    {
        if (obj.woso.isCWall)
        {
            Destroy(gameObject.GetComponent<CircleCollider2D>());
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(6,6);//add new trigger for mouseover
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,3);


        }
        else if (obj.woso.isHWall)
        {
            Destroy(gameObject.GetComponent<CircleCollider2D>());
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(6,1.6f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(.1f,.9f);

            transform.position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);


        }
        else if (obj.woso.isVWall)
        {
            Destroy(gameObject.GetComponent<CircleCollider2D>());
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(.7f,6);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,3);


        }



        if (obj.woso.objType == "Tree")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(6.6f, 19f);//if tree
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0, 9);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Boulder")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(7.13f, 6.76f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0, 3.38f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;

        }
        else if (obj.woso.objType == "BirchTree")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(6,22);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,11);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "BrownShroom")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2.2f,3.3f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.15f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "BunnyHole")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2.2f,1);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,.5f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Campfire")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2.2f,2.3f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.3f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "HotCoals")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2.2f,1);//same as bunnyhole
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,.5f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Kiln")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(3.6f,5.6f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,3);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "MagicalTree")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(6.5f,20);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(-1.5f,10);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Milkweed")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2,3.7f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,2);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Pond")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(6.3f,2);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.2f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Sapling")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(2,3.7f);//same as milkweed
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,2);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "WildCarrot")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(3,3);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.7f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "WildParsnip")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(4.3f,4.6f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,2.5f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "ClayDeposit")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(4.5f,2);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.2f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Wheat")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(3.5f,4);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.5f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "CypressTree")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(8.5f,31);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,15);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Oven")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(3.7f,3.7f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.8f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "WoodenCrate")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(6,3.7f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.8f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Dead Log")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(8.5f,1.5f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,.9f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Dead Stump" || obj.woso.objType == "Stump")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(3.3f,3);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.6f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Tork Shroom")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1.6f,.7f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,.4f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Elderberry Bush" || obj.woso.objType == "Empty Elderberry Bush")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(6,5.7f);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(.2f,3);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
        else if (obj.woso.objType == "Mossy Rock")
        {
            gameObject.AddComponent<BoxCollider2D>().size = new Vector2(4.5f,3);
            GetComponents<BoxCollider2D>()[1].offset = new Vector2(0,1.6f);
            GetComponents<BoxCollider2D>()[1].isTrigger = true;
        }
    }

    /*     collider template
    else if (obj.woso.objType == "")
    {
        gameObject.AddComponent<BoxCollider2D>().size = new Vector2(,);
        GetComponents<BoxCollider2D>()[1].offset = new Vector2(,);
    GetComponents<BoxCollider2D>()[1].isTrigger = true;
    }
    */

    public void OpenContainer()
    {
        containerOpen = true;
        uiInv.SetInventory(this.inventory, 3, this);
        uiInv.gameObject.SetActive(true);//set true after setInv func runs...
        playerMain.SetContainerReference(this);
    }

    public void CloseContainer()
    {
        containerOpen = false;
        uiInv.gameObject.SetActive(false);
    }

    public void SubscribeToEvent()
    {
        if (obj.woso == WosoArray.Instance.SearchWOSOList("Kiln"))
        {
            KilnBehavior kiln = GetComponent<KilnBehavior>();
            kiln.OnClosed += OnClosed;
            kiln.OnOpened += OnOpened;
        }
        else if (obj.woso == WosoArray.Instance.SearchWOSOList("HotCoals"))
        {
            HotCoalsBehavior hotCoals = GetComponent<HotCoalsBehavior>();
            hotCoals.OnFinishedCooking += UpdateStoredItemSprite;
        }
        else if (obj.woso.objType == "Oven")
        {
            KilnBehavior kiln = GetComponent<KilnBehavior>();
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
        if (obj.woso == WosoArray.Instance.SearchWOSOList("Kiln"))
        {
            return gameObject.AddComponent<KilnBehavior>();
        }
        else if (obj.woso == WosoArray.Instance.SearchWOSOList("HotCoals"))
        {
            return gameObject.AddComponent<HotCoalsBehavior>();
        }
        else if (obj.woso == WosoArray.Instance.SearchWOSOList("Oven"))
        {
            return gameObject.AddComponent<KilnBehavior>();
        }
        else if (obj.woso == WosoArray.Instance.SearchWOSOList("Tilled Row"))
        {
            return gameObject.AddComponent<FarmingManager>();
        }
        else if (obj.woso == WosoArray.Instance.SearchWOSOList("BunnyHole"))
        {
            return gameObject.AddComponent<BunnyHole>();
        }

        return null;
    }

    public Component AddItemComponent(Item _item)
    {
        switch (_item.itemSO)
        {
            default: return null;
            //case ItemObjectArray.Instance.KilnKit: return 
        }
    }

    public void AttachItem(Item _item)
    {
        int i = 0;
        if (obj.woso.hasAttachments)
        {
            foreach (ItemSO _itemSO in obj.woso.itemAttachments)
            {
                if (_itemSO.isAttachable && _itemSO == obj.woso.itemAttachments[i])
                {
                    AddAttachment(_item.itemSO.itemType);
                    playerMain.heldItem = null;
                    playerMain.StopHoldingItem();
                }
            }
        }  
    }

    public void CheckBroken()
    {
        if (actionsLeft <= 0)
        {
            if (obj.woso.objAction == Action.ActionType.Default)
            {
                Debug.Log("poo");
                inventory.DropAllItems(player.transform.position);
                inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                inventory.DropAllItems(player.transform.position);
                txt.text = "";
                if (!obj.woso.isPlayerMade)
                {
                    int i = 0;
                    Cell cell = GetComponentInParent<Cell>();
                    if (obj.woso.willTransition)//if transition, spawn then replace tiledata, if not then just delete
                    {
                        var newObj = SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
                        newObj.transform.parent = this.transform.parent;

                        foreach (string tileObj in cell.tileData.objTypes)
                        {
                            if (tileObj == obj.woso.objType)
                            {
                                cell.tileData.objTypes[i] = newObj.obj.woso.objType;
                                cell.tileData.objLocations[i] = newObj.transform.position;
                                break;
                            }
                            i++;
                        }

                    }
                    else
                    {
                        foreach (string tileObj in cell.tileData.objTypes)
                        {
                            if (tileObj == obj.woso.objType)
                            {
                                cell.tileData.objTypes.RemoveAt(i);
                                cell.tileData.objLocations.RemoveAt(i);
                                break;
                            }
                            i++;
                        }
                    }             
                }
                Destroy(gameObject);
            }
            else if (obj.woso.willTransition)//if natural objects transition, this will break world saving..... none do for now
            {
                inventory.DropAllItems(gameObject.transform.position);
                inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                inventory.DropAllItems(gameObject.transform.position);

                if (!obj.woso.isPlayerMade)
                {
                    int i = 0;
                    Cell cell = GetComponentInParent<Cell>();
                    if (obj.woso.willTransition)//if transition, spawn then replace tiledata, if not then just delete
                    {
                        var newObj = SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
                        newObj.transform.parent = this.transform.parent;

                        foreach (string tileObj in cell.tileData.objTypes)
                        {
                            if (tileObj == obj.woso.objType)
                            {
                                cell.tileData.objTypes[i] = newObj.obj.woso.objType;
                                cell.tileData.objLocations[i] = newObj.transform.position;
                                break;
                            }
                            i++;
                        }

                    }
                    else
                    {
                        foreach (string tileObj in cell.tileData.objTypes)
                        {
                            if (tileObj == obj.woso.objType)
                            {
                                cell.tileData.objTypes.RemoveAt(i);
                                cell.tileData.objLocations.RemoveAt(i);
                                break;
                            }
                            i++;
                        }
                    }
                }
                else
                {
                    SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
                }

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
                if (!obj.woso.isPlayerMade)
                {                    
                    int i = 0;
                    Cell cell = GetComponentInParent<Cell>();
                    foreach (string tileObj in cell.tileData.objTypes)
                    {
                        if (tileObj == obj.woso.objType)
                        {
                            cell.tileData.objTypes.RemoveAt(i);
                            cell.tileData.objLocations.RemoveAt(i);
                            break;
                        }
                        i++;
                    }
                }

                if (obj.woso.isContainer)
                {
                    CloseContainer();
                }

                Destroy(gameObject);
            }
        }
    }

    public void AddAttachment(string _itemType)
    {
        if (_itemType == "BagBellows")
        {
            attachmentObj.SetActive(true);
            attachmentObj.AddComponent<Bellows>();
            attachmentObj.GetComponent<SpriteRenderer>().sprite = WorldObject_Assets.Instance.bellowAttachment;
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

    public void GetActionedOn(float _multiplier)
    {
        actionsLeft -= 1*_multiplier;
        Debug.Log(actionsLeft);
        CheckBroken();
    }

    public void OnMouseDown() //FOR THESE MOUSE EVENTS ENTITIES WITH COLLIDERS AS VISION ARE SET TO IGNORE RAYCAST LAYER SO THEY ARENT CLICKABLE BY MOUSE, CHANGE IF WE WANT TO CHANGE THAT??
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (rayHit.collider.CompareTag("WorldObject")/* && playerMain.doAction != Action.ActionType.Melee && playerMain.doAction != Action.ActionType.Shoot && playerMain.doAction != Action.ActionType.Throw */)
        {
            Debug.Log("i was clicked lol");
            player.GetComponent<PlayerMain>().OnObjectSelected(objectAction, this.transform, obj, gameObject);
        }
        else if (rayHit.collider.CompareTag("Attachment") && playerMain.doAction != Action.ActionType.Melee && playerMain.doAction != Action.ActionType.Shoot && playerMain.doAction != Action.ActionType.Throw)
        {
            attachmentObj.GetComponent<Bellows>().OnClicked();
        }

    }

    public void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject())//this is so goddamn convoluted wtf refactor all of this PLEASE
        {
            return;
        }

        RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (rayHit.collider.CompareTag("WorldObject"))
        {
            if (playerMain.doAction == objectAction && objectAction != 0)
            {
                txt.text = $"{obj.woso.objAction} {obj.woso.objType}";
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
            else if (playerMain.isHoldingItem && obj.woso == WosoArray.Instance.SearchWOSOList("Kiln"))
            {
                if (IsSmeltingItem())
                {
                    txt.text = "Smelt";
                }
                else if (IsFuelItem())
                {
                    txt.text = "Add Fuel";
                }
                else if (playerMain.heldItem.itemSO.itemType == ItemObjectArray.Instance.SearchItemList("Clay").itemType)
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
            else if (obj.woso.isInteractable && playerMain.doAction == Action.ActionType.Burn && obj.woso == WosoArray.Instance.SearchWOSOList("Kiln"))
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
            else if (obj.woso.isContainer && !playerMain.isHoldingItem)
            {
                if (IsContainerOpen())
                {
                    txt.text = $"LMB: Close {obj.woso.objType}";
                }
                else
                {
                    txt.text = $"LMB: Open {obj.woso.objType}";
                }
            }
            else if (obj.woso.isContainer && playerMain.isHoldingItem)
            {
                txt.text = $"LMB: Store {playerMain.heldItem.itemSO.itemName}";
            }
            else
            {
                txt.text = obj.woso.objType.ToString();//convert to proper name with new function    
            }
        }
        else
        {
            OnMouseExit();
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

    public bool IsContainerOpen()
    {
        return containerOpen;
    }
}
