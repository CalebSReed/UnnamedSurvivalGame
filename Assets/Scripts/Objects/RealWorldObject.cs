﻿using System;
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
    public WorldObjectData saveData;
    public EventHandler onSaved;
    public EventHandler onLoaded;
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

    public static RealWorldObject SpawnWorldObject(Vector3 position, WorldObject worldObject, bool loaded = false)
    {
        Transform transform = Instantiate(WosoArray.Instance.pfWorldObject, position, Quaternion.identity);
        RealWorldObject realWorldObj = transform.GetComponent<RealWorldObject>();
        realWorldObj.actionsLeft = worldObject.woso.maxUses;
        realWorldObj.SetObject(worldObject, loaded);
        return realWorldObj;
    }

    public WorldObject obj;
    public WOSO woso;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer shadowCaster;
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
        playerMain = player.GetComponent<PlayerMain>();
        hp = GetComponent<HealthManager>();
        hp.OnDeath += Die;
        hp.OnDamageTaken += TakeDamage;

        attachmentObj.GetComponent<SpriteRenderer>().sprite = null;
        attachmentObj.SetActive(false);

        storedItemRenderer.sprite = null;

        plantSpr.sprite = null;//plant sprite
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

    public void SetObject(WorldObject obj, bool isLoaded)
    {
        this.obj = obj;
        woso = obj.woso;
        hp.SetHealth(woso.maxHealth);

        hoverBehavior.Name = obj.woso.objName;
        objectAction = obj.woso.objAction;
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

        if (!isLoaded)
        {
            saveData = new WorldObjectData();
            saveData.objType = woso.objType;
            saveData.currentHealth = hp.currentHealth;
            saveData.actionsLeft = actionsLeft;
            saveData.pos = transform.position;
        }

        lootTable = obj.woso.lootTable;
        lootAmounts = obj.woso.lootAmounts;
        lootChances = obj.woso.lootChances;
        acceptedFuelItems = obj.woso.acceptableFuels;
        spriteRenderer.sprite = obj.woso.objSprite;
        shadowCaster.sprite = obj.woso.objSprite;
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

        if (!woso.isPlayerMade && !woso.isParasiteMade)
        {
            SetParentTile();
        }
    }

    private void SetParentTile()
    {
        var cellPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x / 25) + world.worldSize, Mathf.RoundToInt(transform.position.z / 25) + world.worldSize);
        transform.parent = world.existingTileDictionary[cellPosition].transform;
        transform.localScale = new Vector3(1, 1, 1);
    }

    public void ReplenishUses(int uses)
    {
        actionsLeft += uses;
        if (actionsLeft > woso.maxUses)
        {
            actionsLeft = woso.maxUses;
        }
    }

    public void SetObjectHitBox()
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

            Destroy(spriteRenderer);//instantiate 3D object
            Destroy(transform.GetChild(0).gameObject.GetComponent<BillBoardBehavior>());

            var _col = gameObject.AddComponent<BoxCollider>();
            GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
            _col.size = new Vector3(7, 7, 7);
            _col.center = new Vector3(0, 3, 0);
            var _col2 = transform.GetChild(0).gameObject.AddComponent<BoxCollider>();
            _col2.size = new Vector3(7, 7, 7);
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
        else if (obj.woso.objType == "SulfurPool" || obj.woso.objType == "LavaDeposit" || obj.woso.objType == "sulfurpuddle" || woso.objType == "parasiticpuddle" || woso.objType == "Pond" || woso.objType == "Ice Pond")
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
             || woso.objType == "funnyfungus" || woso.objType == "crystalsapling2")
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
        else if (obj.woso.objType == "Cerulean Fern" || obj.woso.objType == "depthtotem" || woso.objType == "buddinglush")
        {
            transform.GetChild(0).gameObject.AddComponent<BoxCollider>().size = new Vector2(2,4.7f);
            transform.GetChild(0).GetComponent<BoxCollider>().center = new Vector2(0,2);
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
        else if (woso.objType == "Empty Elderberry Bush")
        {
            return gameObject.AddComponent<BerryBushBehavior>();
        }

        return null;
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
        saveData.currentHealth = hp.currentHealth;
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
                DropAttachment();
                if (!DestroyedByEnemy)
                {
                    inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                    inventory.DropAllItems(gameObject.transform.position, true);
                }
            }
            else
            {
                inventory.DropAllItems(gameObject.transform.position, false, !DestroyedByEnemy);
                DropAttachment();
                if (!DestroyedByEnemy)
                {
                    inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                    inventory.DropAllItems(gameObject.transform.position);
                }
            }
            if (!obj.woso.isPlayerMade && !obj.woso.isParasiteMade)
            {
                RemoveFromWorldObjList();
                int i = 0;
                Cell cell = GetComponentInParent<Cell>();
                if (obj.woso.willTransition && !DestroyedByEnemy)
                {
                    var newObj = SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
                    newObj.transform.parent = this.transform.parent;
                    newObj.transform.localScale = new Vector3(1, 1, 1);
                }
            }
            else if (!DestroyedByEnemy)
            {
                var realObj = SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
                realObj.transform.localScale = new Vector3(1, 1, 1);
            }

            Destroy(gameObject);
        }
        else if (obj.woso.objAction == Action.ActionType.Default)
        {
            Debug.Log("poo");
            inventory.DropAllItems(player.transform.position, !DestroyedByEnemy, !DestroyedByEnemy);
            DropAttachment();
            if (!DestroyedByEnemy)
            {
                inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                inventory.DropAllItems(player.transform.position, true);
            }
            if (!obj.woso.isPlayerMade && !obj.woso.isParasiteMade)
            {
                RemoveFromWorldObjList();
                int i = 0;
                Cell cell = GetComponentInParent<Cell>();
                if (obj.woso.willTransition)
                {
                    var newObj = SpawnWorldObject(transform.position, new WorldObject { woso = obj.woso.objTransitions[0] });
                    newObj.transform.parent = this.transform.parent;
                }
            }
            Destroy(gameObject);
        }
        else
        {
            RemoveFromWorldObjList();
            Debug.Log("poo");
            inventory.DropAllItems(gameObject.transform.position, false, !DestroyedByEnemy);
            DropAttachment();
            if (!DestroyedByEnemy)
            {
                inventory.AddLootItems(lootTable, lootAmounts, lootChances);//add them now so we can change sprite when not empty
                inventory.DropAllItems(gameObject.transform.position);
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

    private void RemoveFromWorldObjList()
    {
        for (int i = 0; i < world.naturalObjectSaveList.Count; i++)
        {
            if (world.naturalObjectSaveList[i] == saveData)
            {
                world.naturalObjectSaveList.RemoveAt(i);
            }
        }
    }

    private void DropAttachment()
    {
        if (hasAttachment)
        {
            RealItem.DropItem(new Item { amount = 1, itemSO = woso.itemAttachments[0] }, transform.position, true);
        }
    }

    public void AddAttachment(string itemType)
    {
        hasAttachment = true;
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
        saveData.actionsLeft = actionsLeft;
        if (obj.woso.glows)
        {
            light.intensity = (100 / woso.maxUses * actionsLeft) + 25;
        }
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
            playerMain.audio.Play($"{objectAction}{randVal}", transform.position, gameObject, true);
            playerMain.UseEquippedItemDurability();
            actionsLeft -= args.workEffectiveness;
            saveData.actionsLeft = actionsLeft;
            Debug.Log(actionsLeft);
            CheckBroken();
        }
        else
        {
            player.GetComponent<PlayerMain>().PlayFailedActionSound();
        }
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
    }

    public bool IsContainerOpen()
    {
        return containerOpen;
    }

    public void SaveData()
    {
        saveData.dictKey = new Vector2Int(Mathf.RoundToInt(transform.position.x / 25) + world.worldSize, Mathf.RoundToInt(transform.position.z / 25) + world.worldSize);
        saveData.rotation = transform.rotation;
        if (hasAttachment)
        {
            saveData.attachments.Add(attachmentIndex);
        }

        onSaved?.Invoke(this, EventArgs.Empty);
    }

    public void LoadData(WorldObjectData newSave)
    {
        saveData = newSave;

        actionsLeft = saveData.actionsLeft;
        hp.currentHealth = saveData.currentHealth;
        transform.rotation = saveData.rotation;
        if (saveData.attachments.Count > 0)
        {
            AddAttachment(woso.itemAttachments[saveData.attachments[0]].itemType);
        }

        onLoaded?.Invoke(this, EventArgs.Empty);
    }
}
