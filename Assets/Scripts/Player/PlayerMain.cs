using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using System.IO;
using Unity.Netcode;


public class PlayerMain : NetworkBehaviour
{
    public static PlayerMain Instance;

    public int maxHealth;
    public int maxHunger;
    public int baseAtkDmg;
    public float atkRange;
    public float atkCooldown;
    public float collectRange;
    [SerializeField] int maxInvSpace = 32;
    public NetworkVariable<int> playerId = new NetworkVariable<int>();
    public bool idIsAssigned;
    public Animator animator;
    public Animator playerAnimator;
    public Animator playerSideAnimator;
    public Animator playerBackAnimator;
    public Animator meleeAnimator;
    public Animator swingAnimator;
    public Animator testAnimator;
    public HealthBar healthBar;
    public HungerBar hungerBar;
    internal HungerManager hungerManager;
    internal HealthManager hpManager;
    public Action.ActionType doAction = Action.ActionType.Default;
    public bool godMode;
    public bool freeCrafting;
    internal Inventory inventory;
    public bool isHoldingItem = false;
    public List<GameObject> enemyList = new List<GameObject>();

    public Transform origin;
    public Transform originPivot;
    public Light light2D;
    public Light headLight;

    [SerializeField] Transform objectSelector;
    public GameObject selectedObject;
    private Hoverable selectedObjectHover;

    public bool isHandItemEquipped { get; set; }
    public bool isHeadItemEquipped { get; set; }
    public bool isChestItemEquipped { get; set; }
    public bool isLeggingItemEquipped { get; set; }
    public bool isFootItemEquipped { get; set; }

    public Item equippedHandItem = null;
    public Item heldItem = null;
    public bool isAiming = false;
    public Transform aimingTransform;
    public GameObject pfProjectile;
    public bool hasTongs;
    public AudioManager audio;
    [SerializeField] private UI_CraftMenu_Controller uiCrafter;
    [SerializeField] internal Crafter crafter;
    public UI_Inventory uiInventory;
    public EquipmentManager equipmentManager;
    public UI_EquipSlot handSlot;
    public UI_EquipSlot headSlot;
    public UI_EquipSlot chestSlot;
    public UI_EquipSlot leggingsSlot;
    public UI_EquipSlot feetSlot;
    [SerializeField] public SpriteRenderer aimingSprite;

    public Transform pointer;
    public SpriteRenderer deploySprite;
    public SpriteRenderer deployOutlineSprite;
    public SpriteRenderer containedSprite;
    public Image pointerImage;
    [SerializeField] private GameObject homeArrow;
    private Transform chest;
    public RealWorldObject chestObj;
    public AnimatorEventReceiver eventReceiver;//for footsteps
    public TextMeshProUGUI amountTxt;//remove
    public int[] cellPosition;

    public GameObject starveVign;
    public GameObject freezeVign;
    public GameObject overheatVign;

    public SpriteRenderer meleeHand;

    public Transform body;
    public Transform cam;
    public Camera mainCam;

    [SerializeField] public float speed;
    public float speedMult = 1;
    public readonly float normalSpeed = 20;
    [SerializeField] public Rigidbody rb;
    public Transform bodyHolder;

    public bool currentlyRiding;
    public MobSaveData mobRide;
    public bool etherTarget;
    public Transform homeArrowRef;

    public Coroutine speedRoutine;

    public PlayerInputActions playerInput;

    public PlayerInteractUnityEvent InteractEvent = new PlayerInteractUnityEvent();
    public PlayerInteractUnityEvent SpecialInteractEvent = new PlayerInteractUnityEvent();
    public PlayerInteractUnityEvent CancelEvent = new PlayerInteractUnityEvent();

    public PlayerStateMachine StateMachine { get; private set; }
    public DefaultState defaultState { get; private set; }
    public DeployState deployState { get; private set; }
    public SwingingState swingingState { get; private set; }
    public HoldingItemState holdingItemState { get; private set; }
    public TillingState tillingState { get; private set; }
    public AimingState aimingState { get; private set; }
    public DeadState deadState { get; private set; }
    public WaitingState waitingState { get; private set; }
    public RollingState rollingState { get; private set; }

    private void Awake()
    {
        Instance = this;
        playerInput = new PlayerInputActions();
        playerInput.PlayerDefault.Enable();

        StateMachine = GetComponent<PlayerStateMachine>();
        defaultState = new DefaultState(this, StateMachine);
        deployState = new DeployState(this, StateMachine);
        swingingState = new SwingingState(this, StateMachine);
        holdingItemState = new HoldingItemState(this, StateMachine);
        tillingState = new TillingState(this, StateMachine);
        aimingState = new AimingState(this, StateMachine);
        deadState = new DeadState(this, StateMachine);
        waitingState = new WaitingState(this, StateMachine);
        rollingState = new RollingState(this, StateMachine);

        cellPosition = new int[] { 0,0 };

        playerId.Value = -1;
    }

    void Start()
    {
        StateMachine.StartState(defaultState);
        eventReceiver.eventInvoked += PlayFootStep;

        homeArrow = SceneReferences.Instance.HomeArrow;

        if (IsLocalPlayer)
        {
            homeArrow.gameObject.SetActive(false);
        }
        hpManager = GetComponent<HealthManager>();
        hpManager.SetHealth(maxHealth);


        hpManager.OnDamageTaken += GetHit;
        hpManager.OnHealed += Healed;
        hpManager.OnDeath += Die;

        hungerManager = GetComponent<HungerManager>();
        hungerManager.SetMaxHunger(maxHunger);
        StartCoroutine(hungerManager.DecrementHunger());
        hungerManager.onStarvation += Starve;
        hungerManager.onAlmostStarving += CloseToStarving;
        hungerManager.onNotStarving += NotStarving;

        inventory = new Inventory(maxInvSpace);

        //starveVign = GameObject.Find("StarvationStatus");

        //starveVign.SetActive(false);

        playerAnimator.keepAnimatorStateOnDisable = false;
        playerSideAnimator.keepAnimatorStateOnDisable = false;
        playerBackAnimator.keepAnimatorStateOnDisable = false;

        headLight.intensity = 0;
        light2D.intensity = 0;

        audio = SceneReferences.Instance.audio;
        starveVign = SceneReferences.Instance.starveVign;
        freezeVign = SceneReferences.Instance.freezeVign;
        overheatVign = SceneReferences.Instance.overheatVign;
        pointerImage = SceneReferences.Instance.heldItemImage;
        amountTxt = SceneReferences.Instance.heldItemTxt;
        deploySprite = SceneReferences.Instance.deploySprite;
        deployOutlineSprite = SceneReferences.Instance.deployOutlineSprite;

        cam = GameObject.Find("CameraContainer").transform;
        mainCam = cam.GetChild(0).GetComponent<Camera>();

        handSlot = SceneReferences.Instance.handSlot;
        headSlot = SceneReferences.Instance.headSlot;
        chestSlot = SceneReferences.Instance.chestSlot;
        leggingsSlot = SceneReferences.Instance.legsSlot;
        feetSlot = SceneReferences.Instance.feetSlot;

        if (GameManager.Instance.multiplayerEnabled && IsOwner || !GameManager.Instance.multiplayerEnabled)
        {
            GameManager.Instance.NewPlayerSpawned(this, true);
            GetComponent<Hoverable>().Name = GameManager.Instance.playerName;
            Debug.Log("Local player spawned");
        }
        else
        {
            GameManager.Instance.NewPlayerSpawned(this, false);
            Debug.Log("Non local player spawned");
        }
        
        if (GameManager.Instance.multiplayerEnabled && IsOwner || !GameManager.Instance.multiplayerEnabled)
        {
            SetUpLocalUI();
        }
        bodyHolder.eulerAngles = new Vector3(0, -180, 0);
    }

    private void SetUpLocalUI()
    {
        healthBar = GameObject.Find("HealthSlider").GetComponent<HealthBar>();
        healthBar.SetMaxHealth(maxHealth);

        hungerBar = GameObject.Find("HungerSlider").GetComponent<HungerBar>();
        hungerBar.SetMaxHunger(maxHunger);

        uiInventory = SceneReferences.Instance.uiInventory;

        uiInventory.SetInventory(inventory);

        crafter = GetComponent<Crafter>();
        uiCrafter = SceneReferences.Instance.uiCrafter;

        crafter.SetInventory(inventory);
        uiCrafter.SetInventory(inventory);

        uiInventory.SelectFirstSlot();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            SetPlayerNameRPC(GameManager.Instance.playerName);
            UpdateAllPlayerNamesRPC();
        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateAllPlayerNamesRPC()
    {
        foreach (var player in GameManager.Instance.playerList)
        {
            player.SetPlayerNameForOtherClientsRPC();
        }
    }

    [Rpc(SendTo.Owner)]
    public void SetPlayerNameForOtherClientsRPC()
    {
        SetPlayerNameRPC(GetComponent<Hoverable>().Name);
    }

    [Rpc(SendTo.NotOwner)]
    private void SetPlayerNameRPC(string name)
    {
        GetComponent<Hoverable>().Name = name;
    }

    private void Update()
    {
        cellPosition = new int[] { Mathf.RoundToInt(transform.position.x / 25), Mathf.RoundToInt(transform.position.z / 25) };

        if (!IsLocalPlayer)//run direction code and thats it!
        {
            float angle = Vector3.SignedAngle(bodyHolder.forward, SceneReferences.Instance.mainCamBehavior.rotRef.forward, Vector3.up);

            if (Mathf.Abs(angle) == 180 || Mathf.Abs(angle) == 0)//If we are running perfectly straight with the camera, DONT FLIP!!!!!!
            {
                body.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                if (angle > 0)//idk how i got it backwards but ok
                {
                    body.localScale = new Vector3(-1, 1, 1);
                }
                else if (angle < 0)
                {
                    body.localScale = new Vector3(1, 1, 1);
                }
            }

            // 0-44 is back, 45-134 is side 135-180 is front
            if (Mathf.Abs(angle) < 45)
            {
                playerBackAnimator.transform.localScale = Vector3.one;
                playerSideAnimator.transform.localScale = Vector3.zero;
                playerAnimator.transform.localScale = Vector3.zero;
            }
            else if (Mathf.Abs(angle) >= 45 && Mathf.Abs(angle) < 135)//If we take off the = sign we'll actually retain the original direction before turning diagonal!! pretty cool huh? Change to that if u want to ever.
            {
                playerSideAnimator.transform.localScale = Vector3.one;
                playerAnimator.transform.localScale = Vector3.zero;
                playerBackAnimator.transform.localScale = Vector3.zero;
            }
            else if (Mathf.Abs(angle) >= 135)
            {
                playerAnimator.transform.localScale = Vector3.one;
                playerBackAnimator.transform.localScale = Vector3.zero;
                playerSideAnimator.transform.localScale = Vector3.zero;
            }
            return;
        }

        StateMachine.currentPlayerState.FrameUpdate();

        if (StateMachine.currentPlayerState != swingingState)
        {
            RotateEquippedItemAroundMouse();
        }

        if (doAction == Action.ActionType.Burn)
        {
            //StartCoroutine(DoBurnAction());
        }

        if (IsOwner)
        {
            hungerBar.SetHunger(hungerManager.currentHunger);
        }
        if (doAction == Action.ActionType.Burn)
        {
            light2D.intensity = 100;
        }
        else
        {
            light2D.intensity = 0;
        }

        float scrollVal = playerInput.PlayerDefault.ScrollHotBar.ReadValue<float>();
        if (scrollVal != 0)
        {
            uiInventory.ScrollSlot(scrollVal);
        }

        Aim();

        SelectObjects();
    }

    private void FixedUpdate()
    {
        StateMachine.currentPlayerState.PhysicsUpdate();
        //CheckIfMoving();
    }

    //should we be putting these in a separate class?
    public void OnInteractButtonDown(InputAction.CallbackContext context)//when interact button is pressed. Invoke events subscribed. In default state this should be swinging your tool 
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        if (context.performed && !EventSystem.current.IsPointerOverGameObject())//check if object can be interacted without swinging
        {
            Ray ray = mainCam.ScreenPointToRay(playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());
            RaycastHit[] rayHitList = Physics.RaycastAll(ray);
        }

        if (context.performed && !EventSystem.current.IsPointerOverGameObject() && !currentlyRiding)//this rly should be fireevent or sumn
        {
            InteractEvent?.Invoke();
        }
    }

    public void OnSpecialInteractButtonDown(InputAction.CallbackContext context)//RMB or sumn else for controller
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        if (context.performed && !EventSystem.current.IsPointerOverGameObject())
        {
            /*Ray ray = mainCam.ScreenPointToRay(playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());//this might cause bugs calling in physics update
            RaycastHit[] rayHitList = Physics.RaycastAll(ray);

            foreach (RaycastHit rayHit in rayHitList)
            {
                if (rayHit.collider.isTrigger && rayHit.collider.GetComponentInParent<RealWorldObject>() != null && rayHit.collider.GetComponentInParent<RealWorldObject>().hasSpecialInteraction && Vector3.Distance(rayHit.transform.position, transform.position) <= collectRange)
                {
                    rayHit.collider.GetComponentInParent<RealWorldObject>().OnInteract();
                    return;
                }
                else if (rayHit.collider.isTrigger && rayHit.collider.GetComponentInParent<RealMob>() != null && rayHit.collider.GetComponentInParent<RealMob>().hasSpecialInteraction && Vector3.Distance(rayHit.transform.position, transform.position) <= collectRange)
                {
                    rayHit.collider.GetComponentInParent<RealMob>().OnInteract();
                    return;
                }
            }*/

            if (selectedObject != null)
            {
                if (selectedObject.GetComponent<RealWorldObject>() != null)
                {
                    selectedObject.GetComponent<RealWorldObject>().OnInteract();
                }
                else if (selectedObject.GetComponent<RealItem>() != null)
                {
                    selectedObject.GetComponent<RealItem>().OnInteract();
                }
                else if (selectedObject.GetComponent<RealMob>() != null)
                {
                    selectedObject.GetComponent<RealMob>().OnInteract();
                }
            }

            SpecialInteractEvent?.Invoke();
        }
    }

    public void OnCancelButtonDown(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        if (context.performed && !EventSystem.current.IsPointerOverGameObject())
        {
            CancelEvent?.Invoke();
        }
    }

    private void SelectObjects()
    {
        var objList = Physics.OverlapSphere(objectSelector.position, 5).ToList();

        for (int i = 0; i < objList.Count; i++)
        {
            if (objList[i].transform.root == transform || !objList[i].isTrigger || objList[i].gameObject.layer == 0)
            {
                objList.Remove(objList[i]);
                i--;
            }
        }

        var newList = CalebUtils.SortListByDistance(objList);

        if (newList.Count > 0)
        {
            var collRef = newList[0].Item1.GetComponent<CollisionReferences>();
            if (newList[0].Item1.gameObject != selectedObject)//change object
            {
                selectedObject = collRef.rootObj;
                selectedObjectHover = selectedObject.GetComponent<Hoverable>();
                selectedObjectHover.DoSpecialCase();
                MouseHoverBehavior.Instance.DisplaySelectedObjectText(selectedObjectHover);
            }
            else
            {
                selectedObjectHover.DoSpecialCase();
                MouseHoverBehavior.Instance.DisplaySelectedObjectText(selectedObjectHover);
            }
        }
        else
        {
            MouseHoverBehavior.Instance.RemoveObjectText();
            selectedObject = null;
        }
    }

    private void Healed(object sender, EventArgs e)
    {
        healthBar.SetHealth(hpManager.currentHealth);
    }

    public void PlayFailedActionSound()
    {
        int _rand = Random.Range(1, 4);
        audio.Play($"FailedAction{_rand}", transform.position, gameObject, true);
    }

    public GameObject SpawnProjectile()
    {
        return Instantiate(pfProjectile, meleeHand.transform.position, meleeHand.transform.rotation);
    }

    private void RotateEquippedItemAroundMouse()
    {
        if (Camera_Behavior.Instance.targetLocked)
        {
            origin.LookAt(Camera_Behavior.Instance.enemyTarget);
        }
        else
        {
            origin.rotation = bodyHolder.rotation;
        }
    }

    public void PlayFootStep(AnimationEvent animationEvent)
    {
        int i = Random.Range(1, 7);
        audio.Play($"Step{i}", transform.position, gameObject, true);
    }

    public void Die(object sender, EventArgs e)
    {
        Debug.Log("poof");
        playerAnimator.Play("Front_Idle", 0, 0f);
        playerSideAnimator.Play("Side_Idle", 0, 0f);
        playerSideAnimator.Play("Back_Idle", 0, 0f);

        UnequipItem(Item.EquipType.HandGear, true);
        UnequipItem(Item.EquipType.HeadGear, true);
        UnequipItem(Item.EquipType.ChestGear, true);
        UnequipItem(Item.EquipType.LegGear, true);
        UnequipItem(Item.EquipType.FootGear, true);
        inventory.DropAllItems(transform.position);

        uiInventory.RefreshInventoryItems();

        Announcer.SetText("GAME OVER!", Color.red);
        enemyList.Clear();
        MusicManager.Instance.ForceEndMusic();
        //body.GetChild(0).GetComponent<SpriteRenderer>().color = new Vector4(0,0,0,0);
        StateMachine.ChangeState(deadState);

        if (Application.isEditor)
        {
            if (Directory.Exists(Application.persistentDataPath + "/SaveFiles/EDITORSAVES"))
            {
                Directory.Delete(Application.persistentDataPath + "/SaveFiles/EDITORSAVES", true);
            }
        }
        else
        {
            if (Directory.Exists(Application.persistentDataPath + "/SaveFiles"))
            {
                Directory.Delete(Application.persistentDataPath + "/SaveFiles", true);
            }
        }

        if (etherTarget)
        {
            etherTarget = false;
            GameManager.Instance.localPlayer.GetComponent<EtherShardManager>().ReturnToReality();
        }
        DieRPC();
    }

    public void Revive()
    {
        //StateMachine.SetPrevStateNull();
        if (EtherShardManager.inEther)
        {
            GetComponent<EtherShardManager>().ReturnToReality();
        }
        hpManager.SetCurrentHealth(10, true);
        StateMachine.ChangeState(defaultState, true);
        transform.position = GameManager.Instance.playerHome;
        ReviveRPC();
    }

    [Rpc(SendTo.NotMe)]
    private void DieRPC()
    {
        StateMachine.ChangeState(deadState, true);
    }

    [Rpc(SendTo.NotMe)]
    public void ReviveRPC()
    {
        StateMachine.ChangeState(defaultState, true);
    }

    public void Starve(object sender, System.EventArgs e)
    {
        if (hungerManager.currentHunger <= 0 && !godMode)
        {
            starveVign.SetActive(true);
            starveVign.GetComponent<Image>().color = new Color(starveVign.GetComponent<Image>().color.r, starveVign.GetComponent<Image>().color.g, starveVign.GetComponent<Image>().color.b, .5f);
            hpManager.TakeDamage(Time.deltaTime / 2, "starvation", gameObject, DamageType.Light);
            healthBar.SetHealth(hpManager.currentHealth);
            //CheckDeath();
        }
    }

    public void CloseToStarving(object sender, System.EventArgs e)
    {
        starveVign.SetActive(true);
        starveVign.GetComponent<Image>().color = new Color(starveVign.GetComponent<Image>().color.r, starveVign.GetComponent<Image>().color.g, starveVign.GetComponent<Image>().color.b, .1f);
    }

    public void NotStarving(object sender, EventArgs e)
    {
        starveVign.SetActive(false);
    }

    public void DisableTemperatureVignettes()
    {
        freezeVign.SetActive(false);
        overheatVign.SetActive(false);
    }

    public void GetHit(object sender, DamageArgs e)
    {
        if (StateMachine.currentPlayerState == deadState || !IsLocalPlayer)
        {
            return;
        }
        Debug.Log(e.damageSenderTag);

        if (e.damageSenderTag != "Freezing" && e.damageSenderTag != "Overheating" && e.damageSenderTag != "starvation")
        {
            testAnimator.Play("Hurt");
        }
        swingAnimator.Rebind();
        hpManager.isParrying = false;

        if (currentlyRiding)
        {
            UnrideCreature();
        }
        if (e.damageSenderTag == "Freezing")
        {
            freezeVign.SetActive(true);
        }
        else if (e.damageSenderTag == "Overheating")
        {
            overheatVign.SetActive(true);
        }
        else if (StateMachine.currentPlayerState != deadState && e.damageSenderTag != "starvation")
        {
            StartCoroutine(Yelp());
        }
        DamageArmor(Mathf.RoundToInt(e.damageAmount));
        if (IsOwner)
        {
            healthBar.SetHealth(hpManager.currentHealth);
        }
        if (!godMode)
        {
            //CheckDeath();
        }
    }

    [Rpc(SendTo.Owner)]
    public void SetPositionRPC(Vector3 newPos)
    {
        transform.position = newPos;
    }

    [Rpc(SendTo.Owner)]
    public void ForceTakeDamageRPC(float dmg, string tag)
    {
        hpManager.TakeDamage(dmg, tag, null, DamageType.Light);
    }

    /*[Rpc(SendTo.Owner)]
    public void AssignIdRPC(int id)
    {
        Debug.Log($"taking the id of {id}");
        playerId = id;
    }*/

    [Rpc(SendTo.Owner)]
    public void TakeDamageFromOtherPlayerRPC(float dmg, int dmgType, int playerId)
    {
        GetComponent<HealthManager>().TakeDamage(dmg, "otherplayer", gameObject, (DamageType)dmgType, playerId);
    }

    private void DamageArmor(int damageVal)
    {
        int _amountOfArmorWorn = 0;
        if (equipmentManager.headItem != null && equipmentManager.headItem.itemSO.armorValue > 0)
        {
            _amountOfArmorWorn++;
        }
        if (equipmentManager.chestItem != null && equipmentManager.chestItem.itemSO.armorValue > 0)
        {
            _amountOfArmorWorn++;
        }
        if (equipmentManager.legsItem != null && equipmentManager.legsItem.itemSO.armorValue > 0)
        {
            _amountOfArmorWorn++;
        }
        if (equipmentManager.feetItem != null && equipmentManager.feetItem.itemSO.armorValue > 0)
        {
            _amountOfArmorWorn++;
        }

        if (equipmentManager.headItem != null && equipmentManager.headItem.itemSO.armorValue > 0)
        {
            equipmentManager.headItem.uses -= damageVal / _amountOfArmorWorn;
            equipmentManager.UpdateDurability(equipmentManager.headItem);
        }
        if (equipmentManager.chestItem != null && equipmentManager.chestItem.itemSO.armorValue > 0)
        {
            equipmentManager.chestItem.uses -= damageVal / _amountOfArmorWorn;
            equipmentManager.UpdateDurability(equipmentManager.chestItem);
        }
        if (equipmentManager.legsItem != null && equipmentManager.legsItem.itemSO.armorValue > 0)
        {
            equipmentManager.legsItem.uses -= damageVal / _amountOfArmorWorn;
            equipmentManager.UpdateDurability(equipmentManager.legsItem);
        }
        if (equipmentManager.feetItem != null && equipmentManager.feetItem.itemSO.armorValue > 0)
        {
            equipmentManager.feetItem.uses -= damageVal / _amountOfArmorWorn;
            equipmentManager.UpdateDurability(equipmentManager.feetItem);
        }
    }

    private IEnumerator Yelp()
    {
        int i = Random.Range(1, 4);
        audio.Play($"Hurt{i}", transform.position, gameObject, true);
        starveVign.SetActive(true);
        starveVign.GetComponent<Image>().color = new Color(starveVign.GetComponent<Image>().color.r, starveVign.GetComponent<Image>().color.g, starveVign.GetComponent<Image>().color.b, 1f);
        yield return new WaitForSeconds(.5f);
        starveVign.SetActive(false);
    }

    public void LoadHandItem(Item _item1, Item _item2)//item1 is equipped, item2 is helditem also rename this to loadwithammo ngl
    {
        if (_item1.itemSO.needsAmmo && _item2.itemSO.isAmmo && _item1.itemSO.maxAmmo > _item1.ammo && _item2.itemSO == _item1.itemSO.validAmmo)//if needs ammo, is ammo, item max ammo is bigger than current ammo, and held itemtype is valid ammo for equippeditem
        {
            _item1.ammo++;
            UseHeldItem();
            if (_item1.itemSO.isEquippable)
            {
                if (equippedHandItem == _item1)
                {
                    meleeHand.sprite = equippedHandItem.itemSO.loadedHandSprite;
                    handSlot.UpdateSprite(equippedHandItem.itemSO.loadedSprite);
                    //handSlot.ResetHoverText();
                }
            }
        }
        uiInventory.RefreshInventoryItems();
    }

    public void DropItem(Item item)
    {
        if (equipmentManager.handItem == item)
        {
            UnequipItem(Item.EquipType.HandGear, false);
        }

        int[] containedItemTypes = null;
        int[] containedItemAmounts = null;

        if (item.containedItems != null)
        {
            containedItemAmounts = RealItem.ConvertContainedItemAmounts(item.containedItems);
            containedItemTypes = RealItem.ConvertContainedItemTypes(item.containedItems);
        }

        string heldItemType = null;
        if (item.heldItem != null)
        {
            heldItemType = item.heldItem.itemSO.itemType;
        }

        if (!IsServer)
        {
            ClientHelper.Instance.AskToSpawnItemSpecificRPC(transform.position, false, false, item.itemSO.itemType, item.amount, item.uses, item.ammo, (int)item.itemSO.equipType, item.isHot, item.remainingTime, containedItemTypes, containedItemAmounts, heldItemType);
        }
        else
        {
            RealItem.SpawnRealItem(transform.position, item, true, true, item.ammo, false, false, false);
        }
        //heldItem = null;
        StopHoldingItem();
    }

    public void RideCreature(RealMob mob)
    {
        mob.SaveData();
        mobRide = mob.mobSaveData;
        var HOLDIT = mobRide.mobType;
        currentlyRiding = true;
        mob.Die(false);
        mobRide.mobType = HOLDIT;
        speedMult += 1f;
        UnequipItem(Item.EquipType.HandGear);
        SpecialInteractEvent.AddListener(TryToUnride);
    }

    private void TryToUnride()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            UnrideCreature();
        }
    }

    public void UnrideCreature()
    {
        currentlyRiding = false;
        speedMult -= 1f;
        if (GameManager.Instance.isServer)
        {
            var mob = RealMob.SpawnMob(transform.position, new Mob { mobSO = MobObjArray.Instance.SearchMobList(mobRide.mobType) });
            mob.hpManager.currentHealth = mobRide.currentHealth;
            mob.GetComponent<Ridable>().GetSaddled(ItemObjectArray.Instance.SearchItemList(mobRide.saddle));
        }
        else
        {
            ClientHelper.Instance.AskToSpawnMobRPC(transform.position, mobRide.mobType, mobRide.currentHealth, true, mobRide.saddle);
        }

        mobRide = null;
        SpecialInteractEvent.RemoveListener(TryToUnride);
    }

    public void SetContainerReference(RealWorldObject realObj)
    {
        chest = realObj.transform;
        chestObj = realObj;
    }


    public void UseItem(Item item)
    {
        if (item.itemSO.isEquippable)
        {
            EquipItem(item);
            return;
        }
        else if (item.itemSO.isEatable)
        {
            EatItem(item);
            return;
        }
        else if (item.itemSO.isDeployable && !currentlyRiding)
        {
            deployState.deployItem = item;
            StateMachine.ChangeState(deployState);
            return;
        }
        Debug.Log("not usable");
    }

    public void HoldItem(Item _item)
    {
        if (!isHoldingItem)
        {
            pointerImage.color = Color.white;
            isHoldingItem = true;
            heldItem = _item;           
            UpdateHeldItemStats();
            StateMachine.ChangeState(holdingItemState);
        }
    }

    public void UpdateHeldItemStats()
    {
        if (heldItem.amount <= 0 || heldItem.uses <= 0 && heldItem.itemSO.maxUses > 0)
        {
            heldItem = null;
            StopHoldingItem();
            Debug.Log("item was 0 or negative amount");
            return;
        }

        pointerImage.sprite = heldItem.itemSO.itemSprite;
        if (heldItem.ammo > 0)
        {
            pointerImage.sprite = heldItem.itemSO.loadedSprite;
        }
        if (heldItem.itemSO.isEquippable)
        {
            int newUses = Mathf.RoundToInt((float)heldItem.uses / heldItem.itemSO.maxUses * 100);
            amountTxt.text = $"{newUses}%";
        }
        else if (!heldItem.itemSO.isEquippable && heldItem.amount == 1)
        {
            amountTxt.text = "";
        }
        else
        {
            amountTxt.text = heldItem.amount.ToString();
        }
    }

    public void StopHoldingItem()
    {
        if (isHoldingItem)
        {
            Debug.Log("not holding anymore");
            if (heldItem != null)
            {
                if (heldItem.itemSO.itemType != "Null")
                {
                    inventory.AddItem(heldItem, transform.position, false);
                }
            }
            pointerImage.color = Color.clear;
            amountTxt.text = "";
            isHoldingItem = false;
            heldItem = null;
            StateMachine.ChangeState(StateMachine.previousPlayerState);//Go back to old state, this should hopefully work every time
        }
    }

    public void UseHeldItem(bool returnItem = false)
    {
        if (heldItem.itemSO.maxAmmo > 0)
        {
            heldItem.ammo--;
            UpdateHeldItemStats();
        }
        else if (heldItem.itemSO.maxUses > 0)
        {
            heldItem.uses--;
            UpdateHeldItemStats();
            amountTxt.text = heldItem.uses.ToString();
            return;
        }
        else
        {
            if (heldItem.itemSO.isBowl && returnItem)
            {
                heldItem.itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl");
                UpdateHeldItemStats();
                return;
            }
            heldItem.amount--;
            if (heldItem.amount <= 0)
            {
                heldItem = null;
                StopHoldingItem();
                return;
            }

            if (heldItem.amount == 1 && !heldItem.itemSO.isEquippable)
            {
                amountTxt.text = "";
            }
            else
            {
                amountTxt.text = heldItem.amount.ToString();
            }
        }
    }

    public void UseEquippedItemDurability()//rename this
    {
        equippedHandItem.uses--;
        equipmentManager.UpdateDurability(equipmentManager.handItem);
    }

    public void UpdateContainedItem(Item item)
    {
        containedSprite.sprite = item.itemSO.itemSprite;
        equippedHandItem.heldItem = item;
        if (equippedHandItem.heldItem.isHot)
        {
            containedSprite.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WorldObject_Assets.Instance.hotItem;
            StartCoroutine(CheckHotItem());
        }
    }

    private IEnumerator CheckHotItem()
    {
        yield return null;
        if (equippedHandItem == null || equippedHandItem.heldItem == null || !equippedHandItem.heldItem.isHot)
        {
            containedSprite.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
            yield break;
        }
        else
        {
            StartCoroutine(CheckHotItem());
        }
    }


    public void RemoveContainedItem()
    {
        containedSprite.sprite = null;
    }

    public void EquipItem(Item item)
    {
        if (currentlyRiding && item.equipType == Item.EquipType.HandGear)
        {
            return;
        }

        if (equipmentManager.ItemAlreadyEquipped(item))
        {
            UnequipItem(item.equipType);
            return;
        }
        if (item.itemSO.itemType == "tongs")
        {
            hasTongs = true;
        }

        if (item.heldItem != null)
        {
            UpdateContainedItem(item);
        }
        else
        {
            RemoveContainedItem();
        }

        if (equipmentManager.SlotAlreadyFull(item.equipType))
        {
            if (item.equipType != Item.EquipType.HandGear)
            {
                inventory.AddItem(equipmentManager.ReturnItemByEquipType(item.equipType), transform.position, false);
            }

            equipmentManager.UpdateSlotBool(true, item.equipType);
            GetComponent<TemperatureReceiver>().ChangeRainProtection(-equipmentManager.ReturnItemByEquipType(item.equipType).itemSO.rainProtectionValue);
            GetComponent<TemperatureReceiver>().ChangeTemperatureValue(-equipmentManager.ReturnItemByEquipType(item.equipType).itemSO.temperatureValue);
            UpdateEquippedItem(item);
        }
        else
        {
            equipmentManager.UpdateSlotBool(true, item.equipType);
            UpdateEquippedItem(item);
        }

        if (item.equipType == Item.EquipType.Null)
        {
            Announcer.SetText("ERROR: SET THE DAMN EQUIP BOOL YOU FOOL", Color.red);
        }

        if (item.equipType == Item.EquipType.HandGear)
        {
            playerAnimator.SetLayerWeight(1, 1);
        }

        inventory.RefreshInventory();
    }

    public void UpdateEquippedItem(Item item)
    {
        StateMachine.ChangeState(defaultState);

        if (item == null)
        {
            UnequipItem(item.equipType, false);
            return;
        }

        //aimingSprite.sprite = null;
        //meleeHand.sprite = null;
        //deploySprite.sprite = null;
        //isAiming = false;

        equipmentManager.TurnOffLight(item);

        if (item != null && item.itemSO.itemType != "tongs" && item.equipType == Item.EquipType.HandGear)
        {
            hasTongs = false;
        }

        equipmentManager.SetItem(item);
        //equipSlot.UpdateSprite(item.itemSO.itemSprite); FIX!!
        hpManager.CheckPlayerArmor();
        GetComponent<TemperatureReceiver>().ChangeInsulation(item.itemSO.insulationValue);
        GetComponent<TemperatureReceiver>().ChangeRainProtection(item.itemSO.rainProtectionValue);
        GetComponent<TemperatureReceiver>().ChangeTemperatureValue(item.itemSO.temperatureValue);

        if (item.amount <= 0)
        {
            if (item.itemSO.equipType == Item.EquipType.HandGear)
            {
                doAction = 0;
            }
            equipmentManager.RemoveItem(item);
            return;
        }

        if (item.equipType == Item.EquipType.HandGear)//lighting is handled in equip slot now
        {
            equippedHandItem = item;
            doAction = item.itemSO.doActionType;

            if (item.itemSO.aimingSprite != null)
            {
                meleeHand.sprite = item.itemSO.aimingSprite;
            }
            if (item.ammo > 0 && item.itemSO.loadedHandSprite != null)
            {
                meleeHand.sprite = item.itemSO.loadedHandSprite;
            }
            else
            {
                meleeHand.sprite = item.itemSO.itemSprite;
            }
        }
        if (item.itemSO.doActionType == Action.ActionType.Shoot || item.itemSO.doActionType == Action.ActionType.Throw)
        {
            StateMachine.ChangeState(aimingState);
        }
        if (item.ammo > 0)
        {
            UpdateEquippedItemsOnServerRPC(playerId.Value, item.itemSO.itemType, true, item.equipType);
        }
        else
        {
            UpdateEquippedItemsOnServerRPC(playerId.Value, item.itemSO.itemType, false, item.equipType);
        }

    }

    [Rpc(SendTo.Everyone)]
    public void UpdateEquippedItemsOnServerRPC(int playerID, string itemType, bool isLoaded, Item.EquipType equipSlot)
    {
        SpriteRenderer sprRenderer = null;
        
        if (itemType == null)
        {
            GameManager.Instance.FindPlayerById(playerID).GetComponent<PlayerMain>().equipmentManager.UpdateSprite(null, equipSlot);
        }
        else
        {
            GameManager.Instance.FindPlayerById(playerID).GetComponent<PlayerMain>().equipmentManager.UpdateSprite(ItemObjectArray.Instance.SearchItemList(itemType).itemSprite, equipSlot);
        }

        if (equipSlot == Item.EquipType.HandGear)
        {
            sprRenderer = GameManager.Instance.FindPlayerById(playerID).GetComponent<PlayerMain>().meleeHand;
        }

        if (itemType == null)
        {
            if (equipSlot == Item.EquipType.HandGear)
            {
                meleeHand.sprite = null;
                if (equipmentManager.handLight.intensity != 0)
                {
                    equipmentManager.handLight.intensity = 0;
                }
            }
            if (equipmentManager.headLight.intensity != 0 && equipSlot == Item.EquipType.HeadGear)
            {
                equipmentManager.headLight.intensity = 0;
            }
            return;
        }

        ItemSO newSO = ItemObjectArray.Instance.SearchItemList(itemType);

        if (newSO.doActionType == Action.ActionType.Burn)
        {
            if (newSO.equipType == Item.EquipType.HandGear)
            {
                equipmentManager.handLight.intensity = 100;
            }
            else
            {
                equipmentManager.headLight.intensity = 100;
            }
        }

        if (newSO.loadedHandSprite != null && isLoaded)
        {
            sprRenderer.sprite = newSO.loadedHandSprite;
        }
        else if (newSO.aimingSprite != null)
        {
            sprRenderer.sprite = newSO.aimingSprite;
        }
        else if (equipSlot == Item.EquipType.HandGear)
        {
            sprRenderer.sprite = newSO.itemSprite;
        }
    }

    private void Aim()
    {
        if (isAiming)
        {
            Ray ray = mainCam.ScreenPointToRay(playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());
            RaycastHit rayHit;
            Physics.Raycast(ray, out rayHit);
            rayHit.point = new Vector3(rayHit.point.x, aimingTransform.position.y, rayHit.point.z);//set height to aim transform so it doesnt go crazy when aiming close by
            aimingTransform.LookAt(rayHit.point);
        }
        else
        {
            aimingTransform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }

    public void UnequipItem(Item.EquipType equipType, bool dropItem = true)//drop by default, only dont drop if unequipping by lmb-ing the equip slot
    {
        Item item = null;
        switch (equipType)
        {
            case Item.EquipType.HandGear:
                item = equipmentManager.handItem;
                break;
            case Item.EquipType.HeadGear:
                item = equipmentManager.headItem;
                break;
            case Item.EquipType.ChestGear:
                item = equipmentManager.chestItem;
                break;
            case Item.EquipType.LegGear:
                item = equipmentManager.legsItem;
                break;
            case Item.EquipType.FootGear:
                item = equipmentManager.feetItem;
                break;
        }

        if (item != null)
        {
            if (item.equipType == Item.EquipType.HandGear)
            {
                playerAnimator.SetLayerWeight(1, 0);
            }

            if (item.itemSO.itemType == "tongs")
            {
                hasTongs = false;
            }

            if (item.heldItem != null)
            {
                if (item.heldItem.isHot)
                {
                    if (GameManager.Instance.isServer)
                    {
                        RealItem.DropItem(item.heldItem, transform.position);
                    }
                    else
                    {
                        ClientHelper.Instance.AskToSpawnItemSpecificRPC(transform.position, true, false, item.itemSO.itemType, 1, 0, 0, 0, true, item.remainingTime, null, null, null, true);
                    }
                }
                else
                {
                    inventory.AddItem(new Item { itemSO = item.heldItem.itemSO, amount = 1}, transform.position);
                }
                item.heldItem = null;
                containedSprite.sprite = null;
            } 

            if (dropItem)
            {
                if (item.equipType != Item.EquipType.HandGear)
                {
                    inventory.AddItem(item, transform.position, false);
                }
            }

            if (doAction == Action.ActionType.Till)
            {
                deploySprite.sprite = null;
            }

            if (item.equipType == Item.EquipType.HandGear)
            {
                meleeHand.sprite = null;
                isHandItemEquipped = false;
                aimingSprite.sprite = null;
                equippedHandItem = null;
                isAiming = false;
                if (item.itemSO.doActionType != 0)//so that unequipping clothes dont break u
                {
                    doAction = 0;
                }
            }

            equipmentManager.UpdateSlotBool(false, item.equipType);
            equipmentManager.UpdateSprite(null, item.equipType);

            GetComponent<TemperatureReceiver>().ChangeInsulation(-item.itemSO.insulationValue);
            GetComponent<TemperatureReceiver>().ChangeRainProtection(-item.itemSO.rainProtectionValue);
            GetComponent<TemperatureReceiver>().ChangeTemperatureValue(-item.itemSO.temperatureValue);

            equipmentManager.TurnOffLight(item);

            equipmentManager.RemoveItem(item);
            //_equipSlot.ResetHoverText();
            hpManager.CheckPlayerArmor();
        }
        else
        {
            Debug.Log("null");
        }

        inventory.RefreshInventory();
        UpdateEquippedItemsOnServerRPC(playerId.Value, null, false, equipType);
    }


    public void EatItem(Item _item)
    {
        int randVal = Random.Range(1, 11);
        audio.Play($"Eat{randVal}", transform.position, gameObject, true);
        if (_item.itemSO.restorationValues[0] < 0)
        {
            hpManager.TakeDamage(-_item.itemSO.restorationValues[0], "Food", gameObject);
            healthBar.SetHealth(hpManager.currentHealth);
        }
        else
        {
            hpManager.RestoreHealth(_item.itemSO.restorationValues[0]);
            healthBar.SetHealth(hpManager.currentHealth);
        }
        hungerManager.AddHunger(_item.itemSO.restorationValues[1]);//add function to "barf" out hunger if we lose hunger
        //sanityManager.addsanity
        starveVign.SetActive(false);

        if (_item.itemSO.isPlate)
        {
            inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 1 }, transform.position);
        }

        if (_item.itemSO.isBowl)
        {
            inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 }, transform.position);
        }

        //CheckSpecialItem(_item);

        Debug.Log("ate " + _item);
    }

    public void DodgeRoll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (StateMachine.currentPlayerState == defaultState || StateMachine.currentPlayerState == aimingState
                || StateMachine.currentPlayerState == tillingState || StateMachine.currentPlayerState == deployState
                || StateMachine.currentPlayerState == holdingItemState) 
            {
                testAnimator.Play("Dodge");                
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void DeployObjectRPC(Vector3 pos, string objType, bool isDoor = false, float yRot = 0)
    {
        Debug.Log($"{pos} and {objType}");
        if (isDoor)
        {
            deployState.DeployObjectAsRequestedByClient(pos, WosoArray.Instance.SearchWOSOList(objType), yRot);
        }
        else
        {
            deployState.DeployObjectAsRequestedByClient(pos, WosoArray.Instance.SearchWOSOList(objType));
        }
    }

    public void SetBeacon(RealWorldObject _home)
    {
        if (IsLocalPlayer)
        {
            GameManager.Instance.playerHome = _home.transform.position;
            homeArrow.SetActive(true);
            homeArrow.GetComponent<HomeArrow>().SetHome(_home.transform);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(originPivot.position, atkRange);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 1, transform.position.z + 2.5f), collectRange);
        //Gizmos.DrawWireSphere(deploySprite.transform.position, .5f);
    }

    public override void OnDestroy()
    {
        GameManager.Instance.RemovePlayerFromPlayerList(playerId.Value);
    }
}
