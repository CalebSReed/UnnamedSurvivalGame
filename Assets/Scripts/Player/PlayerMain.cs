using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class PlayerMain : MonoBehaviour
{
    //[SerializeField]
    //internal PlayerCollision playerCollision;
    [SerializeField]
    internal PlayerController playerController;
    public bool isMirrored;
    public Light light2D;
    public Light headLight;
    public int maxHealth;
    public int maxHunger;
    public Animator animator;
    public Animator playerAnimator;
    public Animator meleeAnimator;
    //public Animation actionAnim;

    public HealthBar healthBar;
    public HungerBar hungerBar;
    internal HungerManager hungerManager;
    internal HealthManager hpManager;
    //internal int currentHealth;
    public bool godMode = false;
    internal GameObject player;
    internal Inventory inventory;

    public bool doingAction = false;
    public bool isHoldingItem = false;
    public bool animateWorking = false;

    public int damage;
    public float atkRange;
    public float atkCooldown;
    public bool isAttacking = false;
    public Transform origin;
    public Transform originPivot;

    public bool isHandItemEquipped { get; private set; }
    public bool isHeadItemEquipped { get; private set; }
    public bool isChestItemEquipped { get; private set; }
    public bool isLeggingItemEquipped { get; private set; }
    public bool isFootItemEquipped { get; private set; }

    public bool itemJustUnequipped = false;
    public Item equippedHandItem = null;
    public bool hoveringOverSlot = false;
    public bool isAiming = false;
    public Transform aimingTransform;

    public float collectRange;
    public Item heldItem = null;
    public bool givingItem = false;
    public bool holdingFuel = false;
    public bool holdingValidSmeltItem = false;
    public bool currentlyWorking = false;

    public bool deployMode = false;
    public bool isDeploying = false;
    public bool currentlyDeploying = false;
    public Item itemToDeploy = null;
    public bool tillMode = false;

    public GameObject pfProjectile;

    public bool goingToLight = false;
    public bool goingToCollect = false;
    public bool goingToItem = false;
    private bool goingToToggle = false;
    public bool attachingItem = false;
    private bool isBurning = false;

    [SerializeField] int maxInvSpace = 32;

    public GameObject starveVign;

    public AudioManager audio;

    public Action.ActionType doAction = Action.ActionType.Default;

    [SerializeField] Transform rightHand;

    [SerializeField] private UI_CraftMenu_Controller uiCrafter;
    public UI_Inventory uiInventory;
    [SerializeField] internal Crafter crafter;
    public UI_EquipSlot handSlot;
    public UI_EquipSlot headSlot;
    public UI_EquipSlot chestSlot;
    public UI_EquipSlot leggingsSlot;
    public UI_EquipSlot feetSlot;
    [SerializeField] private SpriteRenderer rightHandSprite;
    [SerializeField] private SpriteRenderer aimingSprite;

    [SerializeField] private SpriteRenderer headSpr;
    [SerializeField] private SpriteRenderer chestSpr;
    [SerializeField] private SpriteRenderer legSpr;
    [SerializeField] private SpriteRenderer feetSpr;

    public Transform pointer;
    public SpriteRenderer deploySprite;
    public Image pointerImage;
    [SerializeField] private GameObject homeArrow;

    private Transform chest;
    private RealWorldObject chestObj;

    public AnimatorEventReceiver eventReceiver;

    public TextMeshProUGUI amountTxt;
    public int[] cellPosition;
    private bool takingItem;

    public GameObject freezeVign;
    public GameObject overheatVign;

    public bool goingtoDropItem;
    public SpriteRenderer meleeHand;

    // Start is called before the first frame update
    void Start()
    {
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
        inventory.OnItemListChanged += OnItemPickedUp;
        uiInventory.SetInventory(inventory);
        crafter.SetInventory(inventory);
        uiCrafter.SetInventory(inventory);

        starveVign.SetActive(false);

        headLight.intensity = 0;

        //RealMob.SpawnMob(new Vector3(4, 4), new Mob { mobType = Mob.MobType.Bunny });
        //RealMob.SpawnMob(new Vector3(-40, 0), new Mob { mobType = Mob.MobType.Wolf });
    }

    private void Update()
    {
        RotateEquippedItemAroundMouse();

        cellPosition = new int[] { Mathf.RoundToInt(transform.position.x / 25), Mathf.RoundToInt(transform.position.z / 25)};

        if (doAction == Action.ActionType.Burn)
        {
            StartCoroutine(DoBurnAction());
        }

        animator.SetBool("isWorking", animateWorking);
        hungerBar.SetHunger(hungerManager.currentHunger);
        if (doAction == Action.ActionType.Burn)
        {
            light2D.intensity = 1;
        }
        else
        {
            light2D.intensity = 0;
        }

        if (chest != null)
        {
            Debug.Log("CHECKING");
            float dist = Vector3.Distance(transform.position, chest.position);
            if (dist > 10 && chestObj.IsContainerOpen())
            {
                chestObj.CloseContainer();
            }
        }


        Aim();
    }

    private void RotateEquippedItemAroundMouse()
    {
        Ray ray = playerController.mainCam.ScreenPointToRay(Input.mousePosition);
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

    private void OnItemPickedUp(object sender, System.EventArgs e)//oh shit i dont know how to fix this
    {
        /*Item _item = inventory.GetItemList().Last();
        int i = inventory.GetItemList().Count();
        if (_item.itemSO.isEquippable && !isItemEquipped )
        {
            EquipItem(_item);
            inventory.RemoveItemBySlot(i);
        }*/
    }

    public void CheckDeath()
    {
        if (hpManager.currentHealth <= 0)
        {
            Debug.Log("poof");
            inventory.DropAllItems(transform.position);
            uiInventory.RefreshInventoryItems();
            handSlot.RemoveItem();
            Announcer.SetText("Whoops! You Died! Hit F9 To Try Again!");
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

    public void CombineHandItem(Item _item1, Item _item2)//item1 is equipped, item2 is helditem also rename this to loadwithammo ngl
    {
        //Debug.Log(equippedHandItem.ammo);
        if (_item1.itemSO.needsAmmo && _item2.itemSO.isAmmo && _item1.itemSO.maxAmmo > _item1.ammo && _item2.itemSO == _item1.itemSO.validAmmo)//if needs ammo, is ammo, item max ammo is bigger than current ammo, and held itemtype is valid ammo for equippeditem
        {
            _item1.ammo++;
            UseHeldItem();
            if (_item1.itemSO.isEquippable)
            {
                if (equippedHandItem == _item1)
                {
                    rightHandSprite.sprite = null;
                    aimingSprite.sprite = equippedHandItem.itemSO.loadedHandSprite;
                    handSlot.UpdateSprite(equippedHandItem.itemSO.loadedSprite);
                    handSlot.ResetHoverText();
                    isAiming = true;
                }
            }
        }
        /*if (_item2.amount <= 0)
        {
            isHoldingItem = false;//might be bad doing all of these but idk, gotta be safe
            holdingFuel = false;
            holdingValidSmeltItem = false;
            pointerImage.sprite = null;
            givingItem = false;
            heldItem = null;
        }*/
        uiInventory.RefreshInventoryItems();
        //UpdateEquippedItem(equippedHandItem);
    }

    public void Shoot()//add another mirror check here before instantiating for both shoot and throw funcs
    {
        if (isAiming && equippedHandItem.ammo > 0)//should not be while holding item BUT, I need to change bow loading because current system is NOT FUN
        {
            playerController.CanMoveAgain = false;
            equippedHandItem.ammo--;
            UseItemDurability();
            UpdateEquippedItem(equippedHandItem, handSlot);
            var _projectile = Instantiate(pfProjectile, aimingSprite.transform.position, aimingSprite.transform.rotation);
            var vel = _projectile.transform.right * 100;
            vel.y = 0;
            _projectile.GetComponent<Rigidbody>().velocity = vel;
            _projectile.GetComponent<ProjectileManager>().SetProjectile(new Item { itemSO = equippedHandItem.itemSO.validAmmo, amount = 1 }, transform.position, gameObject, vel, false);
            playerController.txt.text = "";
        }
    }

    public void Throw()
    {
        if (doAction == Action.ActionType.Throw && isAiming && !isHoldingItem)
        {
            Ray ray = playerController.mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            Physics.Raycast(ray, out rayHit);

            playerController.CanMoveAgain = false;
            var _projectile = Instantiate(pfProjectile, aimingSprite.transform.position, aimingSprite.transform.rotation);
            _projectile.transform.position = new Vector3(_projectile.transform.position.x, _projectile.transform.position.y + 1, _projectile.transform.position.z);
            //_projectile.transform.rotation = aimingSprite.transform.rotation;
            var vel = _projectile.transform.right * 100;
            _projectile.GetComponent<Rigidbody>().velocity = vel;
            _projectile.GetComponent<ProjectileManager>().SetProjectile(equippedHandItem, transform.position, gameObject, vel, true);
            isAiming = false;
            isHandItemEquipped = false;
            handSlot.RemoveItem();
            doAction = 0;
            rightHandSprite.sprite = null;
            aimingSprite.sprite = null;
            equippedHandItem = null;
            playerController.txt.text = "";
        }

    }

    public IEnumerator Attack()//how does this keep getting called rly fast??? omg u were yielding and setting bool to false in the goddamn foreach loop lol
    {
        if (!isAttacking && !doingAction && !isHoldingItem)//how did i break this? mustve been with player gameobject stuff???
        {
            Debug.Log("player attacking");
            meleeAnimator.Play("Melee");
            playerController.target = transform.position;
            isAttacking = true;

            yield return new WaitForSecondsRealtime(.2f);
            Collider[] _hitEnemies = Physics.OverlapSphere(originPivot.position, atkRange);
            foreach (Collider _enemy in _hitEnemies)
            {
                //Debug.Log(_enemy);
                if (_enemy.CompareTag("Enemy") && _enemy != null)//if becomes null and sends error, script stops and we never set attacking to false???? no i have no idea actually
                {
                    _enemy.GetComponent<HealthManager>().TakeDamage(equippedHandItem.itemSO.damage, gameObject.tag, gameObject);
                    UseItemDurability();
                    break;
                }
                else if (_enemy.CompareTag("Mob") && _enemy != null)
                {
                    _enemy.GetComponent<HealthManager>().TakeDamage(equippedHandItem.itemSO.damage, gameObject.tag, gameObject);
                    UseItemDurability();
                    break;
                }
                //custom made components are so goddamn good i can attach this to any gameobject and its super easy to find like this
                //make a component for animation stuff?? OWO like when u take damage, attack, die, etc. or i should use events to 
            }
            //yield return new WaitForSecondsRealtime(atkCooldown);//add input buffer 
            isAttacking = false;
        }
        else
        {
            //Debug.Log("busy attacking");
        }
    }

    public void OnItemSelected(RealItem _realItem)
    {
        playerController.ChangeTarget(_realItem.transform.position);
        playerController.CanMoveAgain = false;
        goingToItem = true;
        StartCoroutine(SeekItem(_realItem.gameObject));
    }

    public void DropItem(Item item)
    {
        RealItem.SpawnRealItem(transform.position, item, true, true, item.ammo, false, false, false);
        heldItem = null;
        StopHoldingItem();
        goingtoDropItem = false;
    }

    public IEnumerator SeekItem(GameObject _object)
    {
        if (_object == null)
        {
            Debug.Log("Item to pick up is null");
            goingToItem = false;
        }
        if (_object != null && Vector2.Distance(_object.transform.position, transform.position) <= 2f)
        {
            goingToItem = false;
            inventory.AddItem(_object.GetComponent<RealItem>().GetItem(), transform.position, true);
            _object.GetComponent<RealItem>().DestroySelf();
        }
        yield return new WaitForSeconds(.1f);
        if (goingToItem)
        {
            StartCoroutine(SeekItem(_object));
        }

    }

    public void OnObjectSelected(Action.ActionType objAction, Transform worldObj, WorldObject obj, GameObject realObj)//bro pls fucking clean this shit up ;-;
    {
        //RealWorldObject realWorldObj = realObj.GetComponent<RealWorldObject>();
        if (worldObj == null)
        {
            Debug.Log("object is null");
            return;
        }

        if (obj.woso.isInteractable && isHoldingItem)
        {
            if (obj.woso.isContainer)
            {
                StartCoroutine(MoveToTarget(worldObj, "store", realObj));
            }
            else if (obj.woso == WosoArray.Instance.SearchWOSOList("Tilled Row") && heldItem.itemSO.isSeed)
            {
                StartCoroutine(MoveToTarget(worldObj, "plant", realObj));
            }
            else if (obj.woso.objType == "Tilled Row" && heldItem.itemSO.doActionType == Action.ActionType.Water)
            {
                StartCoroutine(MoveToTarget(worldObj, "give", realObj));
            }
            foreach (ItemSO _item in obj.woso.acceptableFuels)
            {
                if (_item.itemType == heldItem.itemSO.itemType)
                {
                    StartCoroutine(MoveToTarget(worldObj, "fuel", realObj));
                    break;
                }
            }
            foreach (ItemSO _item in obj.woso.acceptableSmeltItems)
            {
                if (_item.itemType == heldItem.itemSO.itemType)
                {
                    StartCoroutine(MoveToTarget(worldObj, "smelt", realObj));
                    break;
                }
            }
            foreach (ItemSO _item in obj.woso.itemAttachments)
            {
                if (_item.itemType == heldItem.itemSO.itemType)
                {
                    StartCoroutine(MoveToTarget(worldObj, "attach", realObj));
                    break;
                }
            }
            if (heldItem.itemSO == ItemObjectArray.Instance.SearchItemList("Clay"))//change to item.getSealingItem()
            {
                StartCoroutine(MoveToTarget(worldObj, "give", realObj));
            }
            else if (objAction == Action.ActionType.Cook && !realObj.GetComponent<HotCoalsBehavior>().isCooking)
            {
                if (heldItem.itemSO.isCookable)
                {
                    StartCoroutine(MoveToTarget(worldObj, "give", realObj));
                }
            }
            else if (objAction == Action.ActionType.Scoop && heldItem != null && heldItem.itemSO.doActionType == objAction || objAction == Action.ActionType.Scoop && equippedHandItem != null && equippedHandItem.itemSO.doActionType == Action.ActionType.Water )
            {
                StartCoroutine(MoveToTarget(worldObj, "give", realObj));
            }
            else if (objAction == Action.ActionType.Water && heldItem.itemSO.doActionType == objAction)
            {
                StartCoroutine(MoveToTarget(worldObj, "give", realObj));
            }
        }
        else if (objAction == 0)
        {
            StartCoroutine(MoveToTarget(worldObj, "action", realObj));
        }
        else if (obj.woso == WosoArray.Instance.SearchWOSOList("Kiln") && doAction == Action.ActionType.Burn || obj.woso == WosoArray.Instance.SearchWOSOList("Oven") && doAction == Action.ActionType.Burn)
        {
            StartCoroutine(MoveToTarget(worldObj, "light", realObj));
        }
        else if (obj.woso.objType == "Tilled Row" && equippedHandItem != null && equippedHandItem.itemSO.doActionType == Action.ActionType.Water)
        {
            StartCoroutine(MoveToTarget(worldObj, "action", realObj));
        }
        else if (objAction == Action.ActionType.Scoop && equippedHandItem != null && equippedHandItem.itemSO.doActionType == Action.ActionType.Water)
        {
            StartCoroutine(MoveToTarget(worldObj, "give", realObj));
        }
        else if (objAction == doAction)//make it so if were clicking same object, dont spazz around lol
        {
            StartCoroutine(MoveToTarget(worldObj, "action", realObj));
        }
        else if (obj.woso.isContainer && !isHoldingItem)
        {
            StartCoroutine(MoveToTarget(worldObj, "open", realObj));
        }
        else if (obj.woso == WosoArray.Instance.SearchWOSOList("Tilled Row"))
        {
            if (realObj.GetComponent<FarmingManager>().GetHarvestability())
            {
                StartCoroutine(MoveToTarget(worldObj, "harvest", realObj));
            }
        }
        //else if ()
        else//if action mismatch or default action
        {
            //Debug.LogError("THIS WASN'T SUPPOSED TO HAPPEN");
        }
    }

    private Coroutine checkOBJ;

    private IEnumerator MoveToTarget(Transform _target, string action, GameObject _objTarget)
    {
        yield return new WaitForSeconds(.001f);
        if (_target == null)
        {
            Debug.Log("object is null");
            yield break;
        }
        if (checkOBJ != null)
        {
            givingItem = false;
            doingAction = false;
            goingToLight = false;
            attachingItem = false;
            goingToToggle = false;
            takingItem = false;
            StopCoroutine(checkOBJ);
        }

        playerController.ChangeTarget(_target.position);
        Debug.Log(_target.position);

        if (action == "action")
        {
            Debug.Log("CORRECT OR DEFAULT ACTION");
            doingAction = true;
            goingToCollect = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "give")
        {
            Debug.Log("GIVING ITEM");
            givingItem = true;
            goingToCollect = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "light")
        {
            Debug.Log("GOING TO LIGHT");
            goingToLight = true;
            goingToCollect = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "smelt")
        {
            Debug.Log("GOING TO SMELT");
            givingItem = true;
            holdingValidSmeltItem = true;
            goingToCollect = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "fuel")
        {
            Debug.Log("GOING TO ADD FUEL");
            givingItem = true;
            holdingFuel = true;
            goingToCollect = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "attach")
        {
            Debug.Log("GOING TO ATTACH");
            givingItem = true;
            attachingItem = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "store")
        {
            Debug.Log("GOING TO STORE ITEM");
            givingItem = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "open")
        {
            Debug.Log("GOING TO OPEN CONTAINER");
            goingToToggle = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "plant")
        {
            Debug.Log("GOING TO PLANT ITEM");
            givingItem = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "harvest")
        {
            Debug.Log("GOING TO HARVEST ITEM");
            takingItem = true;
            checkOBJ = StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else
        {
            Debug.LogError("INCORRECT ACTION CHECK YOUR SPELLING");
        }
    }

    private IEnumerator CheckItemCollectionRange(GameObject _targetObj)//bruv u rly gotta fix this shit
    {
        bool stillSearching = true;
        if (givingItem || doingAction || goingToLight || attachingItem || goingToToggle || takingItem)
        {
            Collider[] _objectList = Physics.OverlapSphere(new Vector3(transform.position.x, 1, transform.position.z + 2.5f), collectRange);
            foreach (Collider _object in _objectList)
            {
                if (_object == null)
                {
                    Debug.Log("object is null");
                    stillSearching = false;//this line is probably not necessary
                    yield break;
                }

                if (!_object.isTrigger || _object.gameObject.GetComponent<CircleCollider2D>() is null)//make sure is not trigger boi, bruh fix this later... use a distance check? this whole thing sucks
                {
                    if (_object.gameObject == null)
                    {
                        Debug.Log("object is null");
                        stillSearching = false;//this line is probably not necessary
                        yield break;
                    }

                    if (_object.gameObject.transform.position == _targetObj.transform.position)
                    {
                        if (_object.gameObject.CompareTag("WorldObject") && _object.gameObject.GetComponent<RealWorldObject>() != null)
                        {
                            RealWorldObject realObj = _object.gameObject.GetComponent<RealWorldObject>();

                            if (realObj.obj.woso.isInteractable || realObj.obj.woso.isContainer)
                            {
                                if (!realObj.isClosed)//if open
                                {
                                    if (givingItem)
                                    {
                                        if (realObj.obj.woso.isContainer)
                                        {
                                            StoreItem(_object);
                                            givingItem = false;
                                            goingtoDropItem = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (realObj.obj.woso == WosoArray.Instance.SearchWOSOList("Tilled Row") && heldItem != null && heldItem.itemSO.isSeed && !realObj.GetComponent<FarmingManager>().isPlanted)
                                        {
                                            realObj.GetComponent<FarmingManager>().PlantItem(heldItem);
                                            UseHeldItem();
                                            givingItem = false;
                                            goingtoDropItem = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (realObj.obj.woso.objType == "Tilled Row" && realObj.GetComponent<FarmingManager>().isPlanted && !realObj.GetComponent<FarmingManager>().isGrowing && heldItem != null && heldItem.itemSO.doActionType == Action.ActionType.Water && !heldItem.itemSO.needsAmmo)
                                        {
                                            StartCoroutine(realObj.GetComponent<FarmingManager>().GrowPlant());
                                            if (heldItem.itemSO.isBowl)
                                            {
                                                heldItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 1 };
                                            }
                                            pointerImage.sprite = heldItem.itemSO.itemSprite;
                                            givingItem = false;
                                            goingtoDropItem = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (realObj.objectAction == Action.ActionType.Scoop && heldItem != null && heldItem.itemSO.doActionType == realObj.objectAction || realObj.objectAction == Action.ActionType.Scoop && equippedHandItem != null && equippedHandItem.itemSO.doActionType == Action.ActionType.Water)
                                        {
                                            if (heldItem == null)
                                            {
                                                while (realObj.actionsLeft > 0 && equippedHandItem.ammo < equippedHandItem.itemSO.maxAmmo)
                                                {
                                                    equippedHandItem.ammo++;
                                                    realObj.GetActionedOn(1);
                                                    UpdateEquippedItem(handSlot.currentItem, handSlot);
                                                }
                                                goingToCollect = false;
                                                stillSearching = false;
                                                break;
                                            }
                                            realObj.actionsLeft--;
                                            if (realObj.obj.woso.objType == WosoArray.Instance.SearchWOSOList("Pond").objType)
                                            {
                                                heldItem.amount--;
                                                if (heldItem.amount <= 0)
                                                {
                                                    heldItem = null;
                                                    StopHoldingItem();
                                                }
                                                Item _item = new Item { amount = 1, itemSO = ItemObjectArray.Instance.SearchItemList("BowlOfWater"), ammo = 0, equipType = 0, uses = 0 };
                                                inventory.AddItem(_item, transform.position, false);
                                                UpdateHeldItemStats(heldItem);
                                            }
                                            realObj.CheckBroken();
                                            goingToCollect = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (heldItem.itemSO.isSmeltable && !realObj.GetComponent<KilnBehavior>().isSmeltingItem)//if smeltable and not currently smelting something else
                                        {
                                            GiveItem(_object);
                                            playerController.target = transform.position;
                                            goingToCollect = false;
                                            goingtoDropItem = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (heldItem.itemSO.isFuel)//if fuel
                                        {
                                            GiveItem(_object);
                                            playerController.target = transform.position;
                                            goingtoDropItem = false;
                                            goingToCollect = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (heldItem.itemSO.isAttachable)
                                        {
                                            //GiveItem(_object);
                                            playerController.target = transform.position;
                                            goingToCollect = false;
                                            attachingItem = false;
                                            stillSearching = false;
                                            realObj.AttachItem(heldItem);
                                            goingtoDropItem = false;
                                            break;
                                        }
                                        else if (heldItem.itemSO == ItemObjectArray.Instance.SearchItemList("Clay") && realObj.GetComponent<Smelter>().isSmelting && realObj.obj.woso.objType == "Kiln")//change to sealing item, also make it so we can seal and unseal whenever we want, cuz game design ya know?
                                        {
                                            GiveItem(_object);
                                            goingtoDropItem = false;
                                            playerController.target = transform.position;
                                            goingToCollect = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (heldItem.itemSO.isCookable && realObj.objectAction == Action.ActionType.Cook && !realObj.GetComponent<HotCoalsBehavior>().isCooking)
                                        {
                                            realObj.Cook(heldItem);
                                            //GiveItem(_object);
                                            UseHeldItem();
                                            goingtoDropItem = false;
                                            playerController.target = transform.position;
                                            goingToCollect = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (realObj.objectAction == Action.ActionType.Water && heldItem.itemSO.doActionType == realObj.objectAction)
                                        {
                                            heldItem.itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl");
                                            goingtoDropItem = false;
                                            realObj.actionsLeft = 0;
                                            realObj.CheckBroken();
                                            pointerImage.sprite = heldItem.itemSO.itemSprite;
                                            goingToCollect = false;
                                            stillSearching = false;
                                            break;
                                        }
                                    }
                                    else if (takingItem)
                                    {
                                        if (realObj.obj.woso == WosoArray.Instance.SearchWOSOList("Tilled Row"))
                                        {
                                            if (realObj.GetComponent<FarmingManager>().GetHarvestability())
                                            {
                                                doingAction = true;
                                                StartCoroutine(Wait(.5f));
                                                playerController.target = transform.position;
                                                if (doingAction) realObj.GetComponent<FarmingManager>().Harvest();
                                                takingItem = false;
                                                doingAction = false;
                                                stillSearching = false;
                                                break;
                                            }
                                        }
                                    }
                                    else if (goingToToggle)//need to close other chest when we open a new one
                                    {
                                        if (realObj.IsContainerOpen())
                                        {
                                            realObj.CloseContainer();
                                        }
                                        else
                                        {
                                            realObj.OpenContainer();
                                        }
                                        goingToToggle = false;
                                        stillSearching = false;
                                        break;
                                    }
                                    else if (!givingItem && goingToLight)//going to light smelter
                                    {
                                        realObj.StartSmelting();
                                        goingToLight = false;
                                        goingToCollect = false;
                                        stillSearching = false;
                                        break;
                                    }
                                    else if (realObj.objectAction == doAction && doingAction)
                                    {
                                        StartCoroutine(DoAction(doAction, realObj, equippedHandItem));
                                        playerController.target = transform.position;
                                        goingToCollect = false;
                                        stillSearching = false;
                                        break;
                                    }
                                    else if (realObj.obj.woso.objType == "Tilled Row" && realObj.GetComponent<FarmingManager>().isPlanted && !realObj.GetComponent<FarmingManager>().isGrowing && equippedHandItem != null && equippedHandItem.itemSO.doActionType == Action.ActionType.Water && doingAction)
                                    {
                                        if (equippedHandItem.itemSO.needsAmmo && equippedHandItem.ammo > 0)
                                        {
                                            equippedHandItem.ammo--;
                                            UpdateEquippedItem(equippedHandItem, handSlot);
                                            StartCoroutine(realObj.GetComponent<FarmingManager>().GrowPlant());
                                            goingToCollect = false;
                                            doingAction = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (!equippedHandItem.itemSO.needsAmmo)
                                        {
                                            equippedHandItem.uses--;
                                            UpdateEquippedItem(equippedHandItem, handSlot);
                                            StartCoroutine(realObj.GetComponent<FarmingManager>().GrowPlant());
                                            goingToCollect = false;
                                            doingAction = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (equippedHandItem.ammo <= 0 && equippedHandItem.itemSO.needsAmmo)
                                        {
                                            goingToCollect = false;
                                            doingAction = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (realObj.objectAction == doAction && doingAction)
                            {
                                StartCoroutine(DoAction(doAction, realObj, equippedHandItem));
                                playerController.target = transform.position;
                                goingToCollect = false;
                                stillSearching = false;
                                break;
                            }
                            else if (realObj.objectAction == 0 && doingAction)
                            {
                                StartCoroutine(DoAction(doAction, realObj, equippedHandItem));
                                playerController.target = transform.position;
                                goingToCollect = false;
                                stillSearching = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //Debug.LogError("Guess bro couldn't collect... sadge");
                }
            }
        }
        yield return new WaitForSeconds(.25f);
        if (stillSearching)
        {
            StartCoroutine(CheckItemCollectionRange(_targetObj));
        }
    }

    private IEnumerator Wait(float _time)
    {
        yield return new WaitForSeconds(_time);
    }

    public void SetContainerReference(RealWorldObject realObj)
    {
        chest = realObj.transform;
        chestObj = realObj;
    }

    public void StoreItem(Collider _realObj)
    {
        Inventory objInv = _realObj.GetComponent<RealWorldObject>().inventory;

        Item tempItem = new Item() { amount = heldItem.amount, itemSO = heldItem.itemSO, equipType = heldItem.equipType, ammo = heldItem.ammo, uses = heldItem.uses };//must create new item, if we dont then both variables share same memory location and both values change at same time
        objInv.AddItem(tempItem, _realObj.transform.position);
        heldItem = null;
        StopHoldingItem();
        givingItem = false;

        if (!_realObj.GetComponent<RealWorldObject>().IsContainerOpen())
        {
            _realObj.GetComponent<RealWorldObject>().OpenContainer();
        }
    }

    public void GiveItem(Collider _realObj)//only do this if object accepts item
    {
        Inventory objInv = _realObj.GetComponent<RealWorldObject>().inventory;

        //actually i dont think i need to do anything here reguarding fuel and smelting....

        //objInv.GetItemList().Add(heldItem);//adds full stack but is removed and turned into a single item anyways so maybe change in future
        Debug.Log(" held item amount is " + heldItem.amount);
        Item tempItem = new Item() { amount = heldItem.amount, itemSO = heldItem.itemSO};//must create new item, if we dont then both variables share same memory location and both values change at same time
        tempItem.amount = 1;
        Debug.Log(" held item amount is " + heldItem.amount);
        //objInv.SimpleAddItem(tempItem);

        if (_realObj.GetComponent<KilnBehavior>() != null)
        {
            _realObj.GetComponent<KilnBehavior>().ReceiveItem(tempItem);
        }
        else
        {
            _realObj.GetComponent<RealWorldObject>().ReceiveItem(tempItem);
        }

        heldItem.amount--;
        UpdateHeldItemStats(heldItem);
        givingItem = false;
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
            SetDeployItem(item);
            return;
        }
        Debug.LogError("not used");
    }

    public void HoldItem(Item _item)
    {
        if (!isHoldingItem && !deployMode)
        {
            pointerImage.color = Color.white;
            isHoldingItem = true;
            heldItem = _item;           
            UpdateHeldItemStats(_item);
        }
    }

    public void UpdateHeldItemStats(Item _item)
    {
        if (heldItem.amount <= 0)
        {
            heldItem = null;
            StopHoldingItem();
            return;
            amountTxt.text = "";
        }

        pointerImage.sprite = _item.itemSO.itemSprite;
        if (_item.ammo > 0)
        {
            pointerImage.sprite = _item.itemSO.loadedSprite;
        }
        if (_item.itemSO.isEquippable)
        {
            int newUses = Mathf.RoundToInt((float)_item.uses / _item.itemSO.maxUses * 100);
            amountTxt.text = $"{newUses}%";
        }
        else if (!_item.itemSO.isEquippable && _item.amount == 1)
        {
            amountTxt.text = "";
        }
        else
        {
            amountTxt.text = _item.amount.ToString();
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
            holdingFuel = false;
            holdingValidSmeltItem = false;
            pointerImage.sprite = null;
            givingItem = false;
            heldItem = null;
        }
    }

    public void UseHeldItem()
    {
        if (heldItem.itemSO.maxUses > 0)
        {
            heldItem.uses--;
            if (heldItem.uses <= 0)
            {
                heldItem = null;
                StopHoldingItem();
                return;
            }
            amountTxt.text = heldItem.uses.ToString();
        }
        else
        {
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

    public void UseItemDurability()
    {
        equippedHandItem.uses--;
        handSlot.UpdateDurability();
    }

    public void EquipItem(Item item)
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

        if (_item.equipType == Item.EquipType.HandGear)
        {
            aimingSprite.sprite = null;
            meleeHand.sprite = null;

            if (doAction == Action.ActionType.Till)//previous doAction
            {
                deploySprite.sprite = null;
            }

            if (_item.itemSO.doActionType != 0 || doAction == Action.ActionType.Burn && _item.itemSO.doActionType == 0)//this could break sumn idk
            {
                doAction = _item.itemSO.doActionType;
            }
            rightHandSprite.sprite = _item.itemSO.itemSprite;
            equippedHandItem = _item;
            if (_item.ammo > 0)
            {
                handSlot.UpdateSprite(_item.itemSO.loadedSprite);
                rightHandSprite.sprite = null;
                aimingSprite.sprite = equippedHandItem.itemSO.loadedHandSprite;
            }
            else if (doAction == Action.ActionType.Shoot || doAction == Action.ActionType.Throw)//wait why r there 2?
            {
                rightHandSprite.sprite = null;
                aimingSprite.sprite = equippedHandItem.itemSO.aimingSprite;
            }
            if (doAction == Action.ActionType.Shoot || doAction == Action.ActionType.Throw)
            {
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

            if (doAction == Action.ActionType.Melee)
            {
                rightHandSprite.sprite = null;
                meleeHand.sprite = _item.itemSO.aimingSprite;
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

    public IEnumerator JustUnequipped()
    {
        itemJustUnequipped = true;
        yield return new WaitForSeconds(.1f);
        itemJustUnequipped = false;
    }

    private void Aim()
    {
        if (isAiming)
        {
            Ray ray = playerController.mainCam.ScreenPointToRay(Input.mousePosition);
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
                rightHandSprite.sprite = null;
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
        Debug.Log("ate " + _item);
    }

    public void SetDeployItem(Item _item)
    {
        deploySprite.color = new Color(.5f, 1f, 1f, .5f);
        //pointerImage.transform.localScale = new Vector3(1f, 1f, 1f);
        deployMode = true;
        itemToDeploy = _item;
        deploySprite.sprite = itemToDeploy.itemSO.itemSprite;//change to object sprite because items will have diff sprites blah blah blah
    }

    public void UnDeployItem()
    {
        isDeploying = false;
        deployMode = false;
        inventory.AddItem(itemToDeploy, transform.position, false);
        itemToDeploy = null;
        deploySprite.color = new Color(1, 1, 1, 1);
        //pointerImage.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        deploySprite.sprite = null;
    }

    public IEnumerator DeployItem(Item _item)
    {
        if (!currentlyDeploying && itemToDeploy != null)
        {
            Debug.Log("deploying!");
            playerAnimator.SetBool("isDeploying", true);
            deploySprite.sprite = null;
            currentlyDeploying = true;
            if (isDeploying && _item.itemSO.isWall)
            {
                Vector3 newPos = transform.position;
                newPos = new Vector3(Mathf.Round(newPos.x / 6.25f) * 6.25f, 0, Mathf.Round(newPos.z / 6.25f) * 6.25f);
                RealWorldObject obj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = _item.itemSO.deployObject });
                if (obj.woso.isHWall)
                {
                    obj.transform.position = new Vector3(obj.transform.position.x, 0, obj.transform.position.z);
                }
                if (_item.itemSO.itemType == "BeaconKit")
                {
                    //SetBeacon(obj);
                }
                itemToDeploy.amount--;
                if (itemToDeploy.amount <= 0)
                {
                    itemToDeploy = null;
                    deploySprite.sprite = null;
                    deployMode = false;
                }
                else
                {
                    deploySprite.sprite = itemToDeploy.itemSO.itemSprite;
                }
                currentlyDeploying = false;
                isDeploying = false;
                playerAnimator.SetBool("isDeploying", false);
                yield break;
            }

            int x = 0;
            while (x < 20)
            {
                yield return new WaitForSeconds(.1f);
                if (!isDeploying)
                {
                    Debug.Log("STOPPED DEPLOYING");
                    isDeploying = false;
                    currentlyDeploying = false;
                    if (itemToDeploy != null)
                    {
                        deploySprite.sprite = itemToDeploy.itemSO.itemSprite;
                    }
                    playerAnimator.SetBool("isDeploying", false);
                    yield break;
                }
                x++;
            }

            if (isDeploying)
            {
                //Vector3 newPos = transform.position;
                //newPos = new Vector3(Mathf.Round(newPos.x / 6.25f) * 6.25f, Mathf.Round(newPos.y / 6.25f) * 6.25f, 1);
                RealWorldObject obj = RealWorldObject.SpawnWorldObject(playerController.deployPos, new WorldObject { woso = _item.itemSO.deployObject });
                if (_item.itemSO.itemType == "BeaconKit")
                {
                    //SetBeacon(obj);
                }
                itemToDeploy.amount--;
                if (itemToDeploy.amount <= 0)
                {
                    itemToDeploy = null;
                    deploySprite.sprite = null;
                    deployMode = false;
                }
            }
            deploySprite.transform.localPosition = Vector3.forward;
            isDeploying = false;
            currentlyDeploying = false;
            deploySprite.color = new Color(1, 1, 1, 0);
            playerAnimator.SetBool("isDeploying", false);
            //pointerImage.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
    }

    public void SetBeacon(RealWorldObject _home)
    {
        homeArrow.SetActive(true);
        homeArrow.GetComponent<HomeArrow>().SetHome(_home.transform);
    }

    public void TillLand()
    {
        Vector3 newPos = transform.position;
        newPos = new Vector3(Mathf.Round(newPos.x / 6.25f) * 6.25f, 0, Mathf.Round(newPos.z / 6.25f) * 6.25f);
        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Tilled Row") });
        deploySprite.sprite = null;
        tillMode = false;
        deploySprite.color = new Color(1, 1, 1, 0);
        UseItemDurability();
    }

    public void BreakItem(Item _item)
    {
        equippedHandItem = null;
        rightHandSprite.sprite = null;
        isHandItemEquipped = false;
        isBurning = false;
        doAction = 0;
        handSlot.RemoveItem();
        Debug.Log("broke tool");
    }

    public IEnumerator DoAction(Action.ActionType action, RealWorldObject obj, Item _item)
    {
        if (!currentlyWorking)
        {
            currentlyWorking = true;
            if (obj.objectAction == 0)
            {
                while (obj.actionsLeft > 0 && doingAction)
                {
                    yield return new WaitForSeconds(.5f);
                    if (doingAction)
                    {
                        if (_item != null && _item.itemSO.actionEfficiency != 0 && obj != null)
                        {
                            obj.GetActionedOn(_item.itemSO.actionEfficiency);
                        }
                        else
                        {
                            obj.GetActionedOn(1);
                        }
                    }
                }
                currentlyWorking = false;
                animateWorking = false;
            }
            animateWorking = true;
            playerController.target = transform.position;
            while (obj.actionsLeft > 0 && doingAction && doAction == obj.objectAction && _item != null &&_item.uses > 0 && obj != null)//if u click at right timing u can chop twice... maybe keep it its kinda cool like a mini rthym game to work faster
            {
                yield return new WaitForSeconds(.5f);
                if (doingAction && equippedHandItem != null)
                {
                    animateWorking = true;
                    int randVal = Random.Range(1, 4);
                    if (randVal == 1)
                    {
                        audio.Play("Chop1", gameObject);
                    }
                    else if (randVal == 2)
                    {
                        audio.Play("Chop2", gameObject);
                    }
                    else if (randVal == 3)
                    {
                        audio.Play("Chop3", gameObject);
                    }

                    obj.GetActionedOn(_item.itemSO.actionEfficiency);
                    UseItemDurability();

                }
            }
            currentlyWorking = false;
            animateWorking = false;
        }
    }

    public void AnimateActionUse()
    {
        Debug.Log("animate");
        //actionAnim.Play(); brokey make a coroutine later but im lazy
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

    public void OnCollisionEnter2D(Collision2D collision)
    {
        //
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(originPivot.position, atkRange);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 1, transform.position.z + 2.5f), collectRange);
        //Gizmos.DrawWireSphere(deploySprite.transform.position, .5f);
    }
}
