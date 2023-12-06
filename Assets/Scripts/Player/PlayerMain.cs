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


public class PlayerMain : MonoBehaviour
{
    public int maxHealth;
    public int maxHunger;
    public int baseAtkDmg;
    public float atkRange;
    public float atkCooldown;
    public float collectRange;
    [SerializeField] int maxInvSpace = 32;
    public Animator animator;
    public Animator playerAnimator;
    public Animator meleeAnimator;
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

    public bool isHandItemEquipped { get; private set; }
    public bool isHeadItemEquipped { get; private set; }
    public bool isChestItemEquipped { get; private set; }
    public bool isLeggingItemEquipped { get; private set; }
    public bool isFootItemEquipped { get; private set; }

    public Item equippedHandItem = null;
    public Item heldItem = null;
    public bool isAiming = false;
    public Transform aimingTransform;
    public GameObject pfProjectile;
    private bool isBurning = false;
    public AudioManager audio;
    [SerializeField] private UI_CraftMenu_Controller uiCrafter;
    [SerializeField] internal Crafter crafter;
    public UI_Inventory uiInventory;
    public UI_EquipSlot handSlot;
    public UI_EquipSlot headSlot;
    public UI_EquipSlot chestSlot;
    public UI_EquipSlot leggingsSlot;
    public UI_EquipSlot feetSlot;
    [SerializeField] public SpriteRenderer aimingSprite;
    [SerializeField] private SpriteRenderer headSpr;//maybe move all of these equip stuff to another class?
    [SerializeField] private SpriteRenderer chestSpr;
    [SerializeField] private SpriteRenderer legSpr;
    [SerializeField] private SpriteRenderer feetSpr;

    public Transform pointer;
    public SpriteRenderer deploySprite;
    public Image pointerImage;
    [SerializeField] private GameObject homeArrow;
    private Transform chest;
    private RealWorldObject chestObj;
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

    [SerializeField] public float speed = 5f;
    [SerializeField] public Rigidbody rb;

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

    private void Awake()
    {
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
    }

    void Start()
    {
        StateMachine.StartState(defaultState);
        eventReceiver.eventInvoked += PlayFootStep;
        homeArrow.gameObject.SetActive(false);
        hpManager = GetComponent<HealthManager>();
        hpManager.SetHealth(maxHealth);
        healthBar.SetMaxHealth(maxHealth);
        hpManager.OnDamageTaken += GetHit;

        hungerBar.SetMaxHunger(maxHunger);
        hungerManager = GetComponent<HungerManager>();
        hungerManager.SetMaxHunger(maxHunger);
        StartCoroutine(hungerManager.DecrementHunger());
        hungerManager.onStarvation += Starve;
        hungerManager.onAlmostStarving += CloseToStarving;

        inventory = new Inventory(maxInvSpace);
        uiInventory.SetInventory(inventory);
        crafter.SetInventory(inventory);
        uiCrafter.SetInventory(inventory);

        starveVign.SetActive(false);

        headLight.intensity = 0;
    }

    private void Update()
    {
        StateMachine.currentPlayerState.FrameUpdate();

        RotateEquippedItemAroundMouse();

        cellPosition = new int[] { Mathf.RoundToInt(transform.position.x / 25), Mathf.RoundToInt(transform.position.z / 25)};

        if (doAction == Action.ActionType.Burn)
        {
            StartCoroutine(DoBurnAction());
        }

        hungerBar.SetHunger(hungerManager.currentHunger);
        if (doAction == Action.ActionType.Burn)
        {
            light2D.intensity = 1;
        }
        else
        {
            light2D.intensity = 0;
        }

        Aim();
    }

    private void FixedUpdate()
    {
        StateMachine.currentPlayerState.PhysicsUpdate();
        CheckIfMoving();
    }

    //should we be putting these in a separate class?
    public void OnInteractButtonDown(InputAction.CallbackContext context)//when interact button is pressed. Invoke events subscribed. In default state this should be swinging your tool 
    {
        if (context.performed && !EventSystem.current.IsPointerOverGameObject())//check if object can be interacted without swinging
        {
            Ray ray = mainCam.ScreenPointToRay(playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());//this might cause bugs calling in physics update
            RaycastHit[] rayHitList = Physics.RaycastAll(ray);
            foreach(RaycastHit rayHit in rayHitList)
            {
                if (rayHit.collider.isTrigger && rayHit.collider.GetComponentInParent<RealWorldObject>() != null && rayHit.collider.GetComponentInParent<RealWorldObject>().hasSpecialInteraction)
                {
                    rayHit.collider.GetComponentInParent<RealWorldObject>().OnInteract();
                    Debug.Log("Found");
                    return;
                }
            }

        }

        if (context.performed && !EventSystem.current.IsPointerOverGameObject())//this rly should be fireevent or sumn
        {
            InteractEvent?.Invoke();
        }
    }

    public void OnSpecialInteractButtonDown(InputAction.CallbackContext context)//RMB or sumn else for controller
    {
        if (context.performed && !EventSystem.current.IsPointerOverGameObject())
        {
            SpecialInteractEvent?.Invoke();
        }
    }

    public void OnCancelButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed && !EventSystem.current.IsPointerOverGameObject())
        {
            CancelEvent?.Invoke();
        }
    }

    private void CheckIfMoving()
    {
        if (transform.hasChanged)
        {
            playerAnimator.SetBool("isWalking", true);
            transform.hasChanged = false;
        }
        else
        {
            playerAnimator.SetBool("isWalking", false);
        }
    }

    public void PlayFailedActionSound()
    {
        int _rand = Random.Range(1, 4);
        audio.Play($"FailedAction{_rand}", gameObject, Sound.SoundType.SoundEffect, Sound.SoundMode.TwoDimensional);
    }

    public GameObject SpawnProjectile()
    {
        return Instantiate(pfProjectile, aimingSprite.transform.position, aimingSprite.transform.rotation);
    }

    private void RotateEquippedItemAroundMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());
        RaycastHit rayHit;
        Physics.Raycast(ray, out rayHit);
        rayHit.point = new Vector3(rayHit.point.x, origin.position.y, rayHit.point.z);//set height to aim transform so it doesnt go crazy when aiming close by
        origin.LookAt(rayHit.point);
    }

    public void PlayFootStep(AnimationEvent animationEvent)
    {
        int i = Random.Range(1, 7);
        audio.Play($"Step{i}", gameObject, Sound.SoundType.SoundEffect, Sound.SoundMode.ThreeDimensional);
    }

    public void CheckDeath()
    {
        if (hpManager.currentHealth <= 0)
        {
            Debug.Log("poof");
            inventory.DropAllItems(transform.position);
            uiInventory.RefreshInventoryItems();
            if (handSlot.currentItem != null)
            {
                handSlot.RemoveItem();
            }
            Announcer.SetText("Whoops! You Died! Hit F9 To Try Again!");
            enemyList.Clear();
            MusicManager.Instance.ForceEndMusic();
            //body.GetChild(0).GetComponent<SpriteRenderer>().color = new Vector4(0,0,0,0);
            //StateMachine.ChangeState(deadState);
            Destroy(gameObject);
        }
    }

    public void Starve(object sender, System.EventArgs e)
    {
        if (hungerManager.currentHunger <= 0 && !godMode)
        {
            starveVign.SetActive(true);
            starveVign.GetComponent<Image>().color = new Color(starveVign.GetComponent<Image>().color.r, starveVign.GetComponent<Image>().color.g, starveVign.GetComponent<Image>().color.b, .5f);
            hpManager.TakeDamage(2, "Hunger", gameObject);
            healthBar.SetHealth(hpManager.currentHealth);
            CheckDeath();
        }
    }

    public void CloseToStarving(object sender, System.EventArgs e)
    {
        starveVign.SetActive(true);
        starveVign.GetComponent<Image>().color = new Color(starveVign.GetComponent<Image>().color.r, starveVign.GetComponent<Image>().color.g, starveVign.GetComponent<Image>().color.b, .25f);
    }

    public void DisableTemperatureVignettes()
    {
        freezeVign.SetActive(false);
        overheatVign.SetActive(false);
    }

    public void GetHit(object sender, DamageArgs e)
    {
        Debug.Log(e.damageSenderTag);
        if (e.damageSenderTag == "Freezing")
        {
            freezeVign.SetActive(true);
        }
        else if (e.damageSenderTag == "Overheating")
        {
            overheatVign.SetActive(true);
        }
        else
        {
            StartCoroutine(Yelp());
        }
        DamageArmor(e.damageAmount);
        healthBar.SetHealth(hpManager.currentHealth);
        if (!godMode)
        {
            CheckDeath();
        }
    }

    private void DamageArmor(int damageVal)
    {
        int _amountOfArmorWorn = 0;
        if (headSlot.currentItem != null && headSlot.currentItem.itemSO.armorValue > 0)
        {
            _amountOfArmorWorn++;
        }
        if (chestSlot.currentItem != null && chestSlot.currentItem.itemSO.armorValue > 0)
        {
            _amountOfArmorWorn++;
        }
        if (leggingsSlot.currentItem != null && leggingsSlot.currentItem.itemSO.armorValue > 0)
        {
            _amountOfArmorWorn++;
        }
        if (feetSlot.currentItem != null && feetSlot.currentItem.itemSO.armorValue > 0)
        {
            _amountOfArmorWorn++;
        }

        if (headSlot.currentItem != null && headSlot.currentItem.itemSO.armorValue > 0)
        {
            headSlot.currentItem.uses -= damageVal / _amountOfArmorWorn;
            headSlot.UpdateDurability();
        }
        if (chestSlot.currentItem != null && chestSlot.currentItem.itemSO.armorValue > 0)
        {
            chestSlot.currentItem.uses -= damageVal / _amountOfArmorWorn;
            chestSlot.UpdateDurability();
        }
        if (leggingsSlot.currentItem != null && leggingsSlot.currentItem.itemSO.armorValue > 0)
        {
            leggingsSlot.currentItem.uses -= damageVal / _amountOfArmorWorn;
            leggingsSlot.UpdateDurability();
        }
        if (feetSlot.currentItem != null && feetSlot.currentItem.itemSO.armorValue > 0)
        {
            feetSlot.currentItem.uses -= damageVal / _amountOfArmorWorn;
            feetSlot.UpdateDurability();
        }
    }

    private IEnumerator Yelp()
    {
        int i = Random.Range(1, 4);
        audio.Play($"PlayerHurt{i}", gameObject, Sound.SoundType.SoundEffect, Sound.SoundMode.ThreeDimensional);
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
                    aimingSprite.sprite = equippedHandItem.itemSO.loadedHandSprite;
                    handSlot.UpdateSprite(equippedHandItem.itemSO.loadedSprite);
                    handSlot.ResetHoverText();
                    isAiming = true;
                }
            }
        }
        uiInventory.RefreshInventoryItems();
    }





    public void DropItem(Item item)
    {
        RealItem.SpawnRealItem(transform.position, item, true, true, item.ammo, false, false, false);
        heldItem = null;
        StopHoldingItem();
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
        else if (item.itemSO.isDeployable)
        {
            deployState.deployItem = item;
            StateMachine.ChangeState(deployState);
            return;
        }
        Debug.LogError("not used");
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
        if (heldItem.amount <= 0)
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

    public void StopHoldingItem(bool changeState = true)
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
            if (changeState)
            {
                StateMachine.ChangeState(defaultState);
            }
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

    public void UseItemDurability()//rename this
    {
        equippedHandItem.uses--;
        handSlot.UpdateDurability();
    }

    public void EquipItem(Item item)//this is unreasonably long..... refactor this later
    {
        if (handSlot.currentItem != null && item.equipType == Item.EquipType.HandGear)//hand + hand = swap, null + hand = equip hand
        {
            inventory.AddItem(handSlot.currentItem, transform.position, false);
            isHandItemEquipped = true;
            GetComponent<TemperatureReceiver>().ChangeRainProtection(-handSlot.currentItem.itemSO.rainProtectionValue);
            GetComponent<TemperatureReceiver>().ChangeTemperatureValue(-handSlot.currentItem.itemSO.temperatureValue);
            UpdateEquippedItem(item, handSlot);
        }
        else if (handSlot.currentItem == null && item.equipType == Item.EquipType.HandGear)
        {
            isHandItemEquipped = true;
            UpdateEquippedItem(item, handSlot);
        }
        else if (headSlot.currentItem != null && item.equipType == Item.EquipType.HeadGear)
        {
            inventory.AddItem(headSlot.currentItem, transform.position, false);
            isHeadItemEquipped = true;
            GetComponent<TemperatureReceiver>().ChangeInsulation(-headSlot.currentItem.itemSO.insulationValue);
            GetComponent<TemperatureReceiver>().ChangeTemperatureValue(-headSlot.currentItem.itemSO.temperatureValue);
            UpdateEquippedItem(item, headSlot);
        }
        else if (headSlot.currentItem == null && item.equipType == Item.EquipType.HeadGear)
        {
            isHeadItemEquipped = true;
            UpdateEquippedItem(item, headSlot);
        }
        else if (chestSlot.currentItem != null && item.equipType == Item.EquipType.ChestGear)
        {
            inventory.AddItem(chestSlot.currentItem, transform.position, false);
            isChestItemEquipped = true;
            GetComponent<TemperatureReceiver>().ChangeInsulation(-chestSlot.currentItem.itemSO.insulationValue);
            GetComponent<TemperatureReceiver>().ChangeTemperatureValue(-chestSlot.currentItem.itemSO.temperatureValue);
            UpdateEquippedItem(item, chestSlot);
        }
        else if (chestSlot.currentItem == null && item.equipType == Item.EquipType.ChestGear)
        {
            isChestItemEquipped = true;
            UpdateEquippedItem(item, chestSlot);
        }
        else if (leggingsSlot.currentItem != null && item.equipType == Item.EquipType.LegGear)
        {
            inventory.AddItem(leggingsSlot.currentItem, transform.position, false);
            isLeggingItemEquipped = true;
            GetComponent<TemperatureReceiver>().ChangeInsulation(-leggingsSlot.currentItem.itemSO.insulationValue);
            GetComponent<TemperatureReceiver>().ChangeTemperatureValue(-leggingsSlot.currentItem.itemSO.temperatureValue);
            UpdateEquippedItem(item, leggingsSlot);
        }
        else if (leggingsSlot.currentItem == null && item.equipType == Item.EquipType.LegGear)
        {
            isLeggingItemEquipped = true;
            UpdateEquippedItem(item, leggingsSlot);
        }
        else if (feetSlot.currentItem != null && item.equipType == Item.EquipType.FootGear)
        {
            inventory.AddItem(feetSlot.currentItem, transform.position, false);
            isFootItemEquipped = true;
            GetComponent<TemperatureReceiver>().ChangeInsulation(-feetSlot.currentItem.itemSO.insulationValue);
            GetComponent<TemperatureReceiver>().ChangeTemperatureValue(-feetSlot.currentItem.itemSO.temperatureValue);
            UpdateEquippedItem(item, feetSlot);
        }
        else if (feetSlot.currentItem == null && item.equipType == Item.EquipType.FootGear)
        {
            isFootItemEquipped = true;
            UpdateEquippedItem(item, feetSlot);
        }

        if (item.equipType == Item.EquipType.Null)
        {
            Announcer.SetText("ERROR: SET THE DAMN EQUIP BOOL YOU FOOL", Color.red);
        }

    }

    public void UpdateEquippedItem(Item _item, UI_EquipSlot _equipSlot)
    {
        _equipSlot.SetItem(_item);
        hpManager.CheckPlayerArmor();
        GetComponent<TemperatureReceiver>().ChangeInsulation(_item.itemSO.insulationValue);
        GetComponent<TemperatureReceiver>().ChangeRainProtection(_item.itemSO.rainProtectionValue);
        GetComponent<TemperatureReceiver>().ChangeTemperatureValue(_item.itemSO.temperatureValue);
        if (_item.amount <= 0)
        {
            doAction = 0;
            _equipSlot.RemoveItem();
            return;
        }

        if (_item.equipType == Item.EquipType.HandGear)
        {
            aimingSprite.sprite = null;
            if (_item.itemSO.aimingSprite != null && _item.itemSO.doActionType != Action.ActionType.Shoot)
            {
                meleeHand.sprite = _item.itemSO.aimingSprite;
            }
            else if (_item.itemSO.doActionType != Action.ActionType.Shoot)
            {
                meleeHand.sprite = _item.itemSO.itemSprite;
            }

            if (doAction == Action.ActionType.Till)//previous doAction
            {
                deploySprite.sprite = null;
            }

            if (_item.itemSO.doActionType != 0 || doAction == Action.ActionType.Burn && _item.itemSO.doActionType == 0)//this could break sumn idk
            {
                doAction = _item.itemSO.doActionType;
            }
            //rightHandSprite.sprite = _item.itemSO.itemSprite;
            equippedHandItem = _item;
            if (_item.ammo > 0 && doAction == Action.ActionType.Shoot || _item.ammo > 0 && doAction == Action.ActionType.Throw)
            {
                handSlot.UpdateSprite(_item.itemSO.loadedSprite);
                //rightHandSprite.sprite = null;
                aimingSprite.sprite = equippedHandItem.itemSO.loadedHandSprite;
            }
            else if (_item.ammo > 0 && doAction == Action.ActionType.Water)
            {
                meleeHand.sprite = equippedHandItem.itemSO.loadedHandSprite;
            }
            else if (doAction == Action.ActionType.Shoot || doAction == Action.ActionType.Throw)//wait why r there 2?
            {
                //rightHandSprite.sprite = null;
                meleeHand.sprite = null;
                aimingSprite.sprite = equippedHandItem.itemSO.aimingSprite;
            }
            if (doAction == Action.ActionType.Shoot || doAction == Action.ActionType.Throw)
            {
                StateMachine.ChangeState(aimingState);
                isAiming = true;
            }
            else if (doAction == Action.ActionType.Burn)
            {
                //StartCoroutine(DoBurnAction());
                isAiming = false;
            }
            else
            {
                isAiming = false;
            }
        }
        else if (_item.equipType == Item.EquipType.HeadGear)//change bonuses like defense, insulation etc
        {
            headSpr.sprite = _item.itemSO.itemSprite;//change to equipSprite later
            if (_item.itemSO.doActionType == Action.ActionType.Burn)
            {
                headLight.intensity = 1;
            }
            else
            {
                headLight.intensity = 0;
            }
        }
        else if (_item.equipType == Item.EquipType.ChestGear)
        {
            chestSpr.sprite = _item.itemSO.itemSprite;
        }
        else if (_item.equipType == Item.EquipType.LegGear)
        {
            legSpr.sprite = _item.itemSO.itemSprite;
        }
        else if (_item.equipType == Item.EquipType.FootGear)
        {
            feetSpr.sprite = _item.itemSO.itemSprite;            
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

    public void UnequipItem(UI_EquipSlot _equipSlot, bool dropItem = true)//drop by default, only dont drop if unequipping by lmb-ing the equip slot
    {
        if (_equipSlot.currentItem != null)
        {
            if (dropItem)
            {
                inventory.AddItem(_equipSlot.currentItem, transform.position, false);
            }

            if (doAction == Action.ActionType.Till)
            {
                deploySprite.sprite = null;
            }

            if (_equipSlot.currentItem.equipType == Item.EquipType.HandGear)
            {
                meleeHand.sprite = null;
                isHandItemEquipped = false;
                aimingSprite.sprite = null;
                equippedHandItem = null;
                isAiming = false;
                if (_equipSlot.currentItem.itemSO.doActionType != 0)//so that unequipping clothes dont fuck u
                {
                    doAction = 0;
                }
            }
            else if (_equipSlot.currentItem.equipType == Item.EquipType.HeadGear)
            {
                isHeadItemEquipped = false;
                headSpr.sprite = null;
                if (_equipSlot.currentItem.itemSO.doActionType == Action.ActionType.Burn)
                {
                    headLight.intensity = 0;
                }
            }
            else if (_equipSlot.currentItem.equipType == Item.EquipType.ChestGear)
            {
                isChestItemEquipped = false;
                chestSpr.sprite = null;
            }
            else if (_equipSlot.currentItem.equipType == Item.EquipType.LegGear)
            {
                isLeggingItemEquipped = false;
                legSpr.sprite = null;
            }
            else if (_equipSlot.currentItem.equipType == Item.EquipType.FootGear)
            {
                isFootItemEquipped = false;
                feetSpr.sprite = null;
            }

            GetComponent<TemperatureReceiver>().ChangeInsulation(-_equipSlot.currentItem.itemSO.insulationValue);
            GetComponent<TemperatureReceiver>().ChangeRainProtection(-_equipSlot.currentItem.itemSO.rainProtectionValue);
            GetComponent<TemperatureReceiver>().ChangeTemperatureValue(-_equipSlot.currentItem.itemSO.temperatureValue);

            _equipSlot.RemoveItem();
            _equipSlot.ResetHoverText();
            hpManager.CheckPlayerArmor();
        }
        else
        {
            Debug.Log("null");
        }
    }

    public void EatItem(Item _item)
    {
        int randVal = Random.Range(1, 11);
        audio.Play($"Eat{randVal}", gameObject);
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

        Debug.Log("ate " + _item);
    }

    public void SetBeacon(RealWorldObject _home)
    {
        homeArrow.SetActive(true);
        homeArrow.GetComponent<HomeArrow>().SetHome(_home.transform);
    }

    public IEnumerator DoBurnAction()
    {
        if (equippedHandItem != null && !isBurning)
        {
            isBurning = true;
            while (equippedHandItem != null && doAction == Action.ActionType.Burn)
            {
                if (doAction == Action.ActionType.Burn)
                {
                    UseItemDurability();
                    light2D.range = (20f / equippedHandItem.itemSO.maxUses * equippedHandItem.uses) + 200f;
                }
                if (doAction == Action.ActionType.Burn && equippedHandItem != null)
                {
                    yield return new WaitForSeconds(1f);
                    //StartCoroutine(DoBurnAction());
                }
                else
                {
                    isBurning = false;
                    yield break;
                }
            }
            isBurning = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(originPivot.position, atkRange);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 1, transform.position.z + 2.5f), collectRange);
        //Gizmos.DrawWireSphere(deploySprite.transform.position, .5f);
    }
}
