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
    public PlayerMain playerMain;
    private TextMeshProUGUI txt;
    private GameObject mouse;
    public Light light;
    public AudioManager audio;
    private Interactable interactable;
    public bool hasSpecialInteraction;

    public Inventory inventory;
    private List<ItemSO> lootTable;
    private List<int> lootAmounts;
    private List<int> lootChances;

    public ItemSO[] acceptedFuelItems;

    public Action.ActionType objectAction;
    public float actionsLeft;
    public bool isClosed = false;
    public bool containerOpen = false;
    private bool loaded = false;
    public bool hasAttachment = false;
    public int attachmentIndex = 0;

    private HomeArrow hArrow;

    private WorldGeneration world;

    private HealthManager hp;

    public WorldObject.worldObjectType objType { get; private set; }

    public UI_Inventory uiInv;

    [SerializeField] private GameObject cube;
    [SerializeField] private BoxCollider cubeHitBox;
    private GameObject threeDimensionalObject;

    private Coroutine doDmgCoroutine;

    public GameObject vfx;

    public Hoverable hoverBehavior;
    public PlayerInteractUnityEvent receiveEvent = new PlayerInteractUnityEvent();
    public PlayerInteractUnityEvent interactEvent = new PlayerInteractUnityEvent();

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
    public WOSO woso;
    public SpriteRenderer spriteRenderer;
    [SerializeField] public SpriteRenderer storedItemRenderer;
    [SerializeField] private GameObject attachmentObj;
    [SerializeField] private SpriteRenderer plantSpr;

    private void Awake()
    {
        audio = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        hoverBehavior = GetComponent<Hoverable>();
        interactable = GetComponent<Interactable>();
        world = GameObject.FindGameObjectWithTag("World").GetComponent<WorldGeneration>();

        player = GameObject.FindGameObjectWithTag("Player");
        mouse = GameObject.FindGameObjectWithTag("Mouse");
        playerMain = player.GetComponent<PlayerMain>();
        hp = GetComponent<HealthManager>();
        hp.OnDeath += Die;
        hp.OnDamageTaken += TakeDamage;

        attachmentObj.GetComponent<SpriteRenderer>().sprite = null;
        attachmentObj.SetActive(false);

        storedItemRenderer.sprite = null;

        plantSpr.sprite = null;//plant sprite

        txt = mouse.GetComponentInChildren<TextMeshProUGUI>();

        //gameObject.GetComponent<MonoBehaviour>().enabled = false; shit dont work AND lags the game bruh
        //gameObject.GetComponent<CircleCollider>().enabled = false;
    }

    private void Start()
    {
        interactable.OnInteractEvent.AddListener(GetActionedOn);
    }

    public void OnInteract()
    {
        interactEvent?.Invoke();
    }

    public void ReceiveItem()
    {
        receiveEvent?.Invoke();
    }

    public void SetObject(WorldObject obj)
    {
        this.obj = obj;
        woso = obj.woso;
        hp.SetHealth(woso.maxHealth);

        hoverBehavior.Name = obj.woso.objName;
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

        if (woso.temperatureBurn != 0)
        {
            var temp = gameObject.AddComponent<TemperatureEmitter>();
            temp.temp = woso.temperatureBurn;
            temp.tempRadius = woso.temperatureRadius;
            StartCoroutine(temp.EmitTemperature());
        }

        lootTable = obj.woso.lootTable;
        lootAmounts = obj.woso.lootAmounts;
        lootChances = obj.woso.lootChances;
        acceptedFuelItems = obj.woso.acceptableFuels;
        //inventory.AddLootItems(lootTable, lootAmounts, lootChances);
        spriteRenderer.sprite = obj.woso.objSprite;
        //SetObjectComponent();
        SetObjectHitBox();
        SetObjectComponent();
        if (obj.woso.burns)
        {
            StartCoroutine(Burn());
        }

        if (obj.woso.glows)
        {
            light.intensity = 100;
            if (obj.woso.isFloor)
            {
                light.transform.localPosition = new Vector3(0, 0, -6);
            }
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
            Destroy(GetComponent<SphereCollider>());
        }

        if (obj.woso.isFloor)
        {
            Destroy(transform.GetChild(0).GetComponent<BillBoardBehavior>());
            transform.eulerAngles = new Vector3(90, 0, 0);
            transform.position = new Vector3(transform.position.x, .01f, transform.position.z);
        }

        if (!obj.woso.isPlayerMade && !obj.woso.isParasiteMade)
        {
            SetParentTile();
        }
    }

    private void SetParentTile()
    {
        var cellPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x / 25) + world.worldSize, Mathf.RoundToInt(transform.position.z / 25) + world.worldSize);
        Debug.Log(cellPosition);
        transform.parent = world.tileDictionary[cellPosition].transform;
        transform.localScale = new Vector3(1, 1, 1);

        var cell = world.tileDictionary[cellPosition].GetComponent<Cell>();
        cell.tileData.objTypes.Add(obj.woso.objType);
        cell.tileData.objLocations.Add(transform.position);
    }

    public void ReceiveItem(Item item)
    {
        if (woso.burns)
        {
            ReplenishUses(item.itemSO.fuelValue * 2);//double value
            //light.pointLightOuterRadius = (25f / woso.maxUses * actionsLeft);//refreshing rn as well so change burnvalue down there if u change this
        }
    }

    public void ReplenishUses(int uses)
    {
        actionsLeft += uses;
        if (actionsLeft > woso.maxUses)
        {
            actionsLeft = woso.maxUses;
        }
    }

    public void SetObjectHitBox()//--------------------HIT BOXES!!!!!!!-----------------------
    {
        if (obj.woso.objectPrefab != "")
        {
            threeDimensionalObject = Instantiate(ThreeDimensionalObjectArray.Instance.SearchObjList(obj.woso.objectPrefab));
            threeDimensionalObject.transform.parent = this.transform;
            threeDimensionalObject.transform.localPosition = new Vector3(0, 3, 0);
        }

        if (obj.woso.isCWall)
        {
            Destroy(gameObject.GetComponent<SphereCollider>());
            /*gameObject.AddComponent<BoxCollider>().size = new Vector2(7,7);//add new collider
            GetComponents<BoxCollider>()[1].center = new Vector2(0,3);

            gameObject.AddComponent<BoxCollider>().size = new Vector2(7,12.5f);//adds new trigger for mouseover
            GetComponents<BoxCollider>()[2].center = new Vector2(0,6);
            GetComponents<BoxCollider>()[2].isTrigger = true; */

            Destroy(spriteRenderer);//instantiate 3D object!!!
            Destroy(transform.GetChild(0).gameObject.GetComponent<BillBoardBehavior>());
            /*threeDimensionalObject = Instantiate(cube);
            threeDimensionalObject.transform.parent = this.transform;
            threeDimensionalObject.transform.localPosition = new Vector3(0, 3, 0);*/
            var _col = gameObject.AddComponent<BoxCollider>();
            _col.size = new Vector3(6, 6, 6);
            _col.center = new Vector3(0, 3, 0);
            var _col2 = transform.GetChild(0).gameObject.AddComponent<BoxCollider>();
            _col2.size = new Vector3(6, 6, 6);
            _col2.center = new Vector3(0, 3, 0);
            _col2.isTrigger = true;
        }
        else if (obj.woso.isHWall)
        {
            Destroy(gameObject.GetComponent<SphereCollider>());
            /*gameObject.AddComponent<BoxCollider>().size = new Vector2(7,2.6f);
            GetComponents<BoxCollider>()[1].center = new Vector2(.1f,.9f);

            gameObject.AddComponent<BoxCollider>().size = new Vector2(7,8);
            GetComponents<BoxCollider>()[2].center = new Vector2(0,3.6f);
            GetComponents<BoxCollider>()[2].isTrigger = true; */

            var _col = gameObject.AddComponent<BoxCollider>();
            _col.size = new Vector3(6, 6, 1.25f);
            _col.center = new Vector3(0, 3, 0);
            var _col2 = transform.GetChild(0).gameObject.AddComponent<BoxCollider>();
            _col2.size = new Vector3(6, 6, 1.25f);
            _col2.center = new Vector3(0, 3, 0);
            _col2.isTrigger = true;

            Destroy(spriteRenderer);
            Destroy(transform.GetChild(0).gameObject.GetComponent<BillBoardBehavior>());
        }
        else if (obj.woso.isVWall)
        {
            Destroy(gameObject.GetComponent<SphereCollider>());
            /*gameObject.AddComponent<BoxCollider>().size = new Vector2(2.7f,7.5f);
            GetComponents<BoxCollider>()[1].center = new Vector2(0,3);

            gameObject.AddComponent<BoxCollider>().size = new Vector2(1.7f,12.5f);
            GetComponents<BoxCollider>()[2].center = new Vector2(0,6);
            GetComponents<BoxCollider>()[2].isTrigger = true;*/

            var _col = gameObject.AddComponent<BoxCollider>();
            _col.size = new Vector3(6, 6, 1.25f);
            _col.center = new Vector3(0, 3, 0);
            var _col2 = transform.GetChild(0).gameObject.AddComponent<BoxCollider>();
            _col2.size = new Vector3(6, 6, 1.25f);
            _col2.center = new Vector3(0, 3, 0);
            _col2.isTrigger = true;
            transform.Rotate(new Vector3(0, 90, 0));

            Destroy(spriteRenderer);
            Destroy(transform.GetChild(0).gameObject.GetComponent<BillBoardBehavior>());
        }

        if (obj.woso.isMirrored)
        {
            transform.Rotate(new Vector3(0, 0, 180));
            transform.position = new Vector3(transform.position.x, 6, transform.position.z);
        }


        if (obj.woso.objType == "Tree" || woso.objType == "glowinglog")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(6.6f, 19f);//if tree
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0, 9);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Boulder" || obj.woso.objType == "GoldBoulder" || obj.woso.objType == "Depth Pillar" || obj.woso.objType == "Well" || obj.woso.objType == "Empty Well" || obj.woso.objType == "Copper Deposit" || obj.woso.objType == "Cassiterite Deposit" || obj.woso.objType == "Crystal Geode" || obj.woso.objType == "Small Crystal Formation"
            || obj.woso.objType == "Sulfur Boulder" || woso.objType == "brickkiln" || woso.objType == "dryingrack" || woso.objType == "coolingrack" || woso.objType == "boulder2")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(7.13f, 6.76f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0, 3.38f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "SulfurPool" || obj.woso.objType == "LavaDeposit" || obj.woso.objType == "sulfurpuddle")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector3(7.13f, 6.76f, 10);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0, 0);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Gyre Tree" || obj.woso.objType == "Crag Formation" || obj.woso.objType == "Spike Formation" || obj.woso.objType == "Crystal Pillars" || obj.woso.objType == "Arch Formation" || obj.woso.objType == "Tall Sulfur Vent" || woso.objType == "Sulfur-Ridden Tree"
             || woso.objType == "birchtree" || woso.objType == "deciduoustree" || woso.objType == "spookytree")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(6,22);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,11);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "BrownShroom" || woso.objType == "crystalflower" || woso.objType == "gyreflower" || woso.objType == "opalflower" || woso.objType == "Gold Morel" || woso.objType == "fireweed"
             || woso.objType == "funnyfungus")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(2.2f,3.3f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.15f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "BunnyHole")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(4,2);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,.5f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Campfire")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(2.2f,2.3f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.3f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "HotCoals")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(4f,2);//same as bunnyhole
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,.5f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Kiln" || obj.woso.objType == "Crock Pot")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(3.6f,5.6f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,3);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "MagicalTree")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(6.5f,20);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(-1.5f,10);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Cerulean Fern" || obj.woso.objType == "depthtotem")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(2,4.7f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,2);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Pond" || obj.woso.objType == "Ice Pond")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(6.3f,2);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.2f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Sapling")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(2,3.7f);//same as milkweed
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,2);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "WildCarrot")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(3,3);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.7f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Wild Lumble")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(4.3f,4.6f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,2.5f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "ClayDeposit" || obj.woso.objType == "sanddeposit")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(4.5f,2);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.2f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Wheat" || obj.woso.objType == "Short Sulfur Vent")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(3.5f,4);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.5f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "CypressTree")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(8.5f,31);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,15);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Oven")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(3.7f,3.7f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.8f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "WoodenCrate" || obj.woso.objType == "Tilled Row" || obj.woso.objType == "Snowbank" || woso.objType == "stoneslab" || woso.objType == "firepit" || woso.objType == "firepitempty")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(6,3.7f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.8f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Dead Log")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(8.5f,1.5f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,.9f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Dead Stump" || obj.woso.objType == "Stump")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(3.3f,3);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.6f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Tork Shroom")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(1.6f,.7f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,.4f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Elderberry Bush" || obj.woso.objType == "Empty Elderberry Bush" || obj.woso.objType == "Empty Domestic Elderberry Bush" || obj.woso.objType == "Domestic Elderberry Bush")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(6,5.7f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(.2f,3);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
        else if (obj.woso.objType == "Mossy Rock")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(4.5f,3);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,1.6f);
            transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true;
        }
    }

    /*     collider template
    else if (obj.woso.objType == "")
    {
        gameObject.AddComponent<BoxCollider>().size = new Vector2(,);
        GetComponents<BoxCollider>()[1].center = new Vector2(,);
    GetComponents<BoxCollider>()[1].isTrigger = true;
    }
    */





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
        if (woso.seed != null)
        {
            gameObject.AddComponent<SeedTarget>();
        }

        if (obj.woso == WosoArray.Instance.SearchWOSOList("Kiln") || obj.woso.objType == "Crock Pot" || woso.objType == "brickkiln")
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
        else if (obj.woso.isDoor)
        {
            return gameObject.AddComponent<DoorBehavior>();
        }
        else if (obj.woso == WosoArray.Instance.SearchWOSOList("Pond") || obj.woso.objType == "Well")
        {
            return gameObject.AddComponent<WaterSource>();
        }
        else if (obj.woso.isContainer)
        {
            return gameObject.AddComponent<Storage>();
        }
        else if (obj.woso == WosoArray.Instance.SearchWOSOList("sanddeposit"))
        {
            return gameObject.AddComponent<SandSource>();
        }
        else if (obj.woso.isTemporary)
        {
            return gameObject.AddComponent<TemporaryObject>();
        }
        else if (obj.woso.objType == "dryingrack")
        {
            return gameObject.AddComponent<TanningRack>();
        }
        else if (obj.woso.objType == "stoneslab")
        {
            return gameObject.AddComponent<AnvilBehavior>();
        }
        else if (woso.objType == "coolingrack")
        {
            return gameObject.AddComponent<CoolingRackBehavior>();
        }
        else if (woso.objType == "Campfire" || woso.objType == "firepit")
        {
            return gameObject.AddComponent<CampfireBehavior>();
        }
        else if (woso.objType == "firepitempty")
        {
            return gameObject.AddComponent<FirePitBehavior>();
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

    public void AttachItem(Item _item, bool fromPlayer = true)
    {
        int i = 0;
        if (obj.woso.hasAttachments)
        {
            foreach (ItemSO _itemSO in obj.woso.itemAttachments)
            {
                if (_itemSO.isAttachable && _itemSO == obj.woso.itemAttachments[i])
                {
                    AddAttachment(_item.itemSO.itemType);
                    if (fromPlayer)
                    {
                        playerMain.heldItem = null;
                        playerMain.StopHoldingItem();
                    }
                    hasAttachment = true;
                    attachmentIndex = i;
                }
                i++;
            }
        }  
    }

    private void Die(object sender, DamageArgs e)
    {
        Break(true);
    }

    private void TakeDamage(object sender, DamageArgs e)
    {
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        if (spriteRenderer == null)
        {
            var _ren = threeDimensionalObject.GetComponent<Renderer>();
            var oldColor = _ren.material.color;
            _ren.material.color = new Vector4(1, 0, 0, 1);
            yield return new WaitForSeconds(.1f);
            _ren.material.color = oldColor;
            yield break;//change color of mesh idk
        }
        spriteRenderer.color = new Color(255, 0, 0);
        yield return new WaitForSeconds(.1f);
        spriteRenderer.color = new Color(255, 255, 255);
    }

    public void Break(bool DestroyedByEnemy = false)
    {
        if (obj.woso.willTransition)
        {
            if (obj.woso.objAction == Action.ActionType.Default)
            {
                inventory.DropAllItems(gameObject.transform.position, !DestroyedByEnemy, !DestroyedByEnemy);
                if (!DestroyedByEnemy)
                {
                    inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                    inventory.DropAllItems(gameObject.transform.position, true);
                }
            }
            else
            {
                inventory.DropAllItems(gameObject.transform.position, false, !DestroyedByEnemy);
                if (!DestroyedByEnemy)
                {
                    inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                    inventory.DropAllItems(gameObject.transform.position);
                }
            }
            if (!obj.woso.isPlayerMade && !obj.woso.isParasiteMade)//if not player made, change the tile save data
            {
                int i = 0;
                Cell cell = GetComponentInParent<Cell>();
                if (obj.woso.willTransition && !DestroyedByEnemy)//if transition, spawn then replace tiledata, if not then just delete. Also if destroyed by enemy dont transition lol. (campfire to coals)
                {
                    var newObj = SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
                    newObj.transform.parent = this.transform.parent;
                    newObj.transform.localScale = new Vector3(1, 1, 1);

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
            else if (!DestroyedByEnemy)
            {
                var realObj = SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
                realObj.transform.localScale = new Vector3(1, 1, 1);
            }

            txt.text = "";
            Destroy(gameObject);
        }
        else if (obj.woso.objAction == Action.ActionType.Default)
        {
            Debug.Log("poo");
            inventory.DropAllItems(player.transform.position, !DestroyedByEnemy, !DestroyedByEnemy);
            if (!DestroyedByEnemy)
            {
                inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                inventory.DropAllItems(player.transform.position, true);
            }
            txt.text = "";
            if (!obj.woso.isPlayerMade && !obj.woso.isParasiteMade)
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
        else
        {
            Debug.Log("poo");
            inventory.DropAllItems(gameObject.transform.position, false, !DestroyedByEnemy);
            if (!DestroyedByEnemy)
            {
                inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                inventory.DropAllItems(gameObject.transform.position);
            }
            txt.text = "";
            if (!obj.woso.isPlayerMade && !obj.woso.isParasiteMade)
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

            Destroy(gameObject);
        }
    }

    public void CheckBroken()
    {
        if (actionsLeft <= 0)
        {
            Break();
        }
    }

    public void AddAttachment(string itemType)
    {
        if (itemType == "BagBellows")
        {
            attachmentObj.SetActive(true);
            var attach = attachmentObj.AddComponent<Bellows>();
            attach.bellowPower = 300;
            attachmentObj.GetComponent<SpriteRenderer>().sprite = WorldObject_Assets.Instance.bellowAttachment;
        }
        else if (itemType == "bellowslvl2")
        {
            attachmentObj.SetActive(true);
            var attach = attachmentObj.AddComponent<Bellows>();
            attach.bellowPower = 600;
            attachmentObj.GetComponent<SpriteRenderer>().sprite = WorldObject_Assets.Instance.bellow2Attachment;
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
        //light.pointLightOuterRadius -= light.pointLightOuterRadius / obj.woso.maxUses;
        //light.pointLightOuterRadius = (25f / woso.maxUses * actionsLeft);//20 + 5 = max radius, 5 is smallest radius
        if (obj.woso.glows)
        {
            light.intensity = (100 / woso.maxUses * actionsLeft) + 25;
        }
        //Debug.Log(light.intensity.ToString());
        CheckBroken();
        StartCoroutine(Burn());
    }

    public void GetActionedOn(InteractArgs args)
    {
        if (args.hitTrigger && obj.woso.isCollidable)//ONLY hit the nontrigger UNLESS this object HAS NO collider!
        {
            return;
        }
        if (objectAction == 0)
        {
            int randVal = UnityEngine.Random.Range(1, 4);
            playerMain.audio.Play($"Collect{randVal}", transform.position, gameObject, true);
            Break();
            return;
        }

        if (objectAction == args.actionType)
        {
            int randVal = UnityEngine.Random.Range(1, 4);
            playerMain.audio.Play($"Chop{randVal}", transform.position, gameObject, true);
            playerMain.UseItemDurability();
            actionsLeft -= args.workEffectiveness;
            Debug.Log(actionsLeft);
            CheckBroken();
        }
        else
        {
            player.GetComponent<PlayerMain>().PlayFailedActionSound();
        }
    }

    public void OnMouseDown() //FOR THESE MOUSE EVENTS ENTITIES WITH COLLIDERS AS VISION ARE SET TO IGNORE RAYCAST LAYER SO THEY ARENT CLICKABLE BY MOUSE, CHANGE IF WE WANT TO CHANGE THAT??
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        Physics.Raycast(ray, out rayHit);
        if (rayHit.collider.CompareTag("WorldObject")/* && playerMain.doAction != Action.ActionType.Melee && playerMain.doAction != Action.ActionType.Shoot && playerMain.doAction != Action.ActionType.Throw */)
        {
            //Debug.Log("i was clicked lol");
            //player.GetComponent<PlayerMain>().OnObjectSelected(objectAction, this.transform, obj, gameObject);
        }
        else if (rayHit.collider.CompareTag("Attachment") && playerMain.doAction != Action.ActionType.Melee && playerMain.doAction != Action.ActionType.Shoot && playerMain.doAction != Action.ActionType.Throw)
        {
            //attachmentObj.GetComponent<Bellows>().OnClicked(); 
        }

    }

    public void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject())//this is so goddamn convoluted wtf refactor all of this PLEASE
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        Physics.Raycast(ray, out rayHit);
        if (rayHit.collider != null && rayHit.collider.CompareTag("WorldObject"))
        {
            if (playerMain.doAction == objectAction && objectAction != 0 || playerMain.isHoldingItem && playerMain.heldItem.itemSO.doActionType == objectAction && objectAction != 0)
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
            else if (obj.woso.isDoor && GetComponent<DoorBehavior>().isOpen)
            {
                txt.text = "LMB: Close Door";
            }
            else if (obj.woso.isDoor && !GetComponent<DoorBehavior>().isOpen)
            {
                txt.text = "LMB: Open Door";
            }
            else if (playerMain.isHoldingItem && obj.woso == WosoArray.Instance.SearchWOSOList("Kiln") || playerMain.isHoldingItem && obj.woso.burns)
            {
                if (IsSmeltingItem())
                {
                    txt.text = "LMB: Smelt";
                }
                else if (IsFuelItem())
                {
                    txt.text = "LMB: Add Fuel";
                }
                else if (playerMain.heldItem.itemSO.itemType == ItemObjectArray.Instance.SearchItemList("Clay").itemType)
                {
                    txt.text = "LMB: Seal";
                }
                else
                {
                    txt.text = $"{obj.woso.objType}";
                }
            }
            else if (GetComponent<FarmingManager>() != null && playerMain.isHoldingItem && playerMain.heldItem.itemSO.isSeed && !GetComponent<FarmingManager>().isPlanted)
            {
                txt.text = $"LMB: Plant {playerMain.heldItem.itemSO.itemName}";
            }
            else if (GetComponent<FarmingManager>() != null && playerMain.isHoldingItem && playerMain.heldItem.itemSO.doActionType == Action.ActionType.Water && GetComponent<FarmingManager>().isPlanted || GetComponent<FarmingManager>() != null && playerMain.equippedHandItem != null && playerMain.equippedHandItem.itemSO.doActionType == Action.ActionType.Water && GetComponent<FarmingManager>().isPlanted)
            {
                txt.text = $"LMB: Water {obj.woso.objType}";
            }
            else if (objectAction == 0 && !playerMain.isAiming)
            {
                txt.text = $"LMB: Pick {obj.woso.objType}";
            }
            else if (obj.woso.isInteractable && playerMain.doAction == Action.ActionType.Burn && obj.woso == WosoArray.Instance.SearchWOSOList("Kiln"))
            {
                if (!GetComponent<Smelter>().isSmelting && GetComponent<Smelter>().currentFuel > 0)
                {
                    txt.text = $"LMB: Light {obj.woso.objType}";
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

    private void OnTriggerEnter(Collider other)
    {
        if (woso.doesDamage && other.CompareTag("Player"))
        {
            other.GetComponentInParent<HealthManager>().TakeDamage(woso.damage, woso.objType, gameObject);
            doDmgCoroutine = StartCoroutine(CheckToDamageAgain());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (woso.doesDamage && other.CompareTag("Player"))
        {
            StopCoroutine(doDmgCoroutine);
        }
    }

    private IEnumerator CheckToDamageAgain()
    {
        yield return new WaitForSeconds(.5f);
        playerMain.GetComponent<HealthManager>().TakeDamage(woso.damage, woso.objType, gameObject);
        doDmgCoroutine = StartCoroutine(CheckToDamageAgain());
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
