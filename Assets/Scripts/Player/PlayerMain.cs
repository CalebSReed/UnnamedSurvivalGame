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
    public Light2D light2D;
    public int maxHealth;
    public int maxHunger;
    public Animator animator;
    public Animator playerAnimator;
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

        inventory = new Inventory(maxInvSpace);
        inventory.OnItemListChanged += OnItemPickedUp;
        uiInventory.SetInventory(inventory);
        crafter.SetInventory(inventory);
        uiCrafter.SetInventory(inventory);

        starveVign.SetActive(false);

        //RealMob.SpawnMob(new Vector3(4, 4), new Mob { mobType = Mob.MobType.Bunny });
        //RealMob.SpawnMob(new Vector3(-40, 0), new Mob { mobType = Mob.MobType.Wolf });
    }

    private void Update()
    {
        cellPosition = new int[] { Mathf.RoundToInt(transform.position.x / 25), Mathf.RoundToInt(transform.position.y / 25)};

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

    public void PlayFootStep(AnimationEvent animationEvent)
    {
        int i = Random.Range(1, 7);
        audio.Play($"Step{i}", gameObject);
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
            hpManager.TakeDamage(2);
            healthBar.SetHealth(hpManager.currentHealth);
            CheckDeath();
        }
    }

    public void GetHit(object sender, EventArgs e)
    {
        healthBar.SetHealth(hpManager.currentHealth);
        if (!godMode)
        {
            CheckDeath();
        }
    }

    public void CombineHandItem(Item _item1, Item _item2)//item1 is equipped, item2 is helditem also rename this to loadwithammo ngl
    {
        //Debug.Log(equippedHandItem.ammo);
        if (_item1.itemSO.needsAmmo && _item2.itemSO.isAmmo && _item1.itemSO.maxAmmo > _item1.ammo && _item2.itemSO == _item1.itemSO.validAmmo)//if needs ammo, is ammo, item max ammo is bigger than current ammo, and held itemtype is valid ammo for equippeditem
        {
            _item1.ammo++;
            _item2.amount--;
            if (_item1.itemSO.isEquippable)
            {
                rightHandSprite.sprite = null;
                aimingSprite.sprite = equippedHandItem.itemSO.loadedHandSprite;
                handSlot.UpdateSprite(equippedHandItem.itemSO.loadedSprite);
                handSlot.ResetHoverText();
                isAiming = true;
            }
        }
        if (_item2.amount <= 0)
        {
            isHoldingItem = false;//might be bad doing all of these but idk, gotta be safe
            holdingFuel = false;
            holdingValidSmeltItem = false;
            pointerImage.sprite = null;
            givingItem = false;
            heldItem = null;
        }
        uiInventory.RefreshInventoryItems();
        //UpdateEquippedItem(equippedHandItem);
    }

    public void Shoot()//add another mirror check here before instantiating for both shoot and throw funcs
    {
        if (isAiming && equippedHandItem.ammo > 0)//should not be while holding item BUT, I need to change bow loading because current system is NOT FUN
        {
            equippedHandItem.ammo--;
            UseItemDurability();
            UpdateEquippedItem(equippedHandItem, handSlot);
            var _projectile = Instantiate(pfProjectile, aimingTransform.transform.position, aimingTransform.rotation);
            _projectile.GetComponent<ProjectileManager>().SetProjectile(new Item { itemSO = equippedHandItem.itemSO.validAmmo, amount = 1 }, transform.position, false);
            if (isMirrored)
            {
                _projectile.GetComponent<Rigidbody2D>().velocity = -aimingTransform.right * 100;//arrow is flipped when mirrored and im p sure the arrow doesnt even have velocity so this code makes no sense
            }
            else
            {
                _projectile.GetComponent<Rigidbody2D>().velocity = aimingTransform.right * 100;
            }
            playerController.txt.text = "";
        }
    }

    public void Throw()
    {
        if (doAction == Action.ActionType.Throw && isAiming && !isHoldingItem)
        {
            var _projectile = Instantiate(pfProjectile, aimingTransform.transform.position, aimingTransform.rotation);
            _projectile.GetComponent<ProjectileManager>().SetProjectile(equippedHandItem, Camera.main.WorldToScreenPoint(Input.mousePosition), true);
            if (isMirrored)
            {
                _projectile.GetComponent<Rigidbody2D>().velocity = -aimingTransform.right * 100;
            }
            else
            {
                _projectile.GetComponent<Rigidbody2D>().velocity = aimingTransform.right * 100;
            }
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
            animator.Play("Melee");
            playerController.target = transform.position;
            isAttacking = true;

            yield return new WaitForSecondsRealtime(.25f);
            Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(origin.position, atkRange);
            foreach (Collider2D _enemy in _hitEnemies)
            {
                Debug.Log(_enemy);
                if (_enemy.CompareTag("Enemy") && _enemy != null)//if becomes null and sends error, script stops and we never set attacking to false???? no i have no idea actually
                {
                    _enemy.GetComponent<HealthManager>().TakeDamage(equippedHandItem.itemSO.damage);
                    UseItemDurability();
                    break;
                }
                else if (_enemy.CompareTag("Mob") && _enemy != null)
                {
                    _enemy.GetComponent<HealthManager>().TakeDamage(equippedHandItem.itemSO.damage);
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
        goingToItem = true;
        StartCoroutine(SeekItem(_realItem.transform));
    }

    public IEnumerator SeekItem(Transform _transform)
    {
        if (isHandItemEquipped)
        {
            Debug.Log("LOOKING FOR ITEM");
            Collider2D[] _itemList = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y + 2.5f), collectRange);

            foreach (Collider2D _item in _itemList)
            {
                if (_item.transform == _transform)
                {
                    if (_item.gameObject.GetComponent<RealItem>().isHot && _item.gameObject.GetComponent<RealItem>().item.itemSO.needsToBeHot && equippedHandItem.itemSO.actionType == Action.ActionType.Hammer)
                    {
                        //_item.gameObject.GetComponent<RealItem>().item.itemSO = _item.gameObject.GetComponent<RealItem>().item.itemSO.actionReward[0];
                        _item.gameObject.GetComponent<RealItem>().SetItem(new Item { itemSO = _item.gameObject.GetComponent<RealItem>().item.itemSO.actionReward[0], amount = 1 }, false);//change to true at some point maybe
                        goingToItem = false;
                        UseItemDurability();
                    }
                }
            }
        }
        else
        {
            goingToItem = false;
        }
        if (_transform = null)
        {
            goingToItem = false;
        }
        yield return new WaitForSeconds(.25f);
        if (goingToItem)
        {
            StartCoroutine(SeekItem(_transform));
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
            else if (objAction == Action.ActionType.Scoop && heldItem.itemSO.actionType == objAction)
            {
                StartCoroutine(MoveToTarget(worldObj, "give", realObj));
            }
            else if (objAction == Action.ActionType.Water && heldItem.itemSO.actionType == objAction)
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

    private IEnumerator MoveToTarget(Transform _target, string action, GameObject _objTarget)
    {
        yield return new WaitForSeconds(.001f);
        if (_target == null)
        {
            Debug.Log("object is null");
            yield break;
        }
        playerController.ChangeTarget(_target.position);
        if (action == "action")
        {
            Debug.Log("CORRECT OR DEFAULT ACTION");
            doingAction = true;
            goingToCollect = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "give")
        {
            Debug.Log("GIVING ITEM");
            givingItem = true;
            goingToCollect = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "light")
        {
            Debug.Log("GOING TO LIGHT");
            goingToLight = true;
            goingToCollect = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "smelt")
        {
            Debug.Log("GOING TO SMELT");
            givingItem = true;
            holdingValidSmeltItem = true;
            goingToCollect = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "fuel")
        {
            Debug.Log("GOING TO ADD FUEL");
            givingItem = true;
            holdingFuel = true;
            goingToCollect = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "attach")
        {
            Debug.Log("GOING TO ATTACH");
            givingItem = true;
            attachingItem = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "store")
        {
            Debug.Log("GOING TO STORE ITEM");
            givingItem = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "open")
        {
            Debug.Log("GOING TO OPEN CONTAINER");
            goingToToggle = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "plant")
        {
            Debug.Log("GOING TO PLANT ITEM");
            givingItem = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
        }
        else if (action == "harvest")
        {
            Debug.Log("GOING TO HARVEST ITEM");
            takingItem = true;
            StartCoroutine(CheckItemCollectionRange(_objTarget));
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
            Collider2D[] _objectList = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y + 2.5f), collectRange);
            foreach (Collider2D _object in _objectList)
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
                        if (_object.gameObject.CompareTag("WorldObject"))
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
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (realObj.obj.woso == WosoArray.Instance.SearchWOSOList("Tilled Row") && heldItem.itemSO.isSeed && !realObj.GetComponent<FarmingManager>().IsGrowing())
                                        {
                                            realObj.GetComponent<FarmingManager>().PlantItem(heldItem);
                                            UseHeldItem();
                                            givingItem = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (heldItem.itemSO.isSmeltable && !realObj.GetComponent<KilnBehavior>().isSmeltingItem)//if smeltable and not currently smelting something else
                                        {
                                            GiveItem(_object);
                                            playerController.target = transform.position;
                                            goingToCollect = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (heldItem.itemSO.isFuel)//if fuel
                                        {
                                            GiveItem(_object);
                                            playerController.target = transform.position;
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
                                            break;
                                        }
                                        else if (heldItem.itemSO == ItemObjectArray.Instance.SearchItemList("Clay") && realObj.GetComponent<Smelter>().isSmelting && realObj.obj.woso.objType == "Kiln")//change to sealing item, also make it so we can seal and unseal whenever we want, cuz game design ya know?
                                        {
                                            GiveItem(_object);
                                            playerController.target = transform.position;
                                            goingToCollect = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (heldItem.itemSO.isCookable && realObj.objectAction == Action.ActionType.Cook && !realObj.GetComponent<HotCoalsBehavior>().isCooking)
                                        {
                                            realObj.Cook(heldItem);
                                            GiveItem(_object);
                                            playerController.target = transform.position;
                                            goingToCollect = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (realObj.objectAction == Action.ActionType.Scoop && heldItem.itemSO.actionType == realObj.objectAction)
                                        {
                                            realObj.actionsLeft--;
                                            if (realObj.obj.woso.objType == WosoArray.Instance.SearchWOSOList("Pond").objType)
                                            {
                                                heldItem.amount--;
                                                if (heldItem.amount <= 0)
                                                {
                                                    heldItem = null;
                                                    StopHoldingItem();
                                                }
                                                Item _item = new Item { amount = 1, itemSO = ItemObjectArray.Instance.SearchItemList("BowlOfWater") };
                                                RealItem.SpawnRealItem(transform.position, _item, false);
                                                UpdateHeldItemStats(heldItem);
                                            }
                                            realObj.CheckBroken();
                                            goingToCollect = false;
                                            stillSearching = false;
                                            break;
                                        }
                                        else if (realObj.objectAction == Action.ActionType.Water && heldItem.itemSO.actionType == realObj.objectAction)
                                        {
                                            heldItem.itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl");
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

    public void StoreItem(Collider2D _realObj)
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

    public void GiveItem(Collider2D _realObj)//only do this if object accepts item
    {
        Inventory objInv = _realObj.GetComponent<RealWorldObject>().inventory;

        //actually i dont think i need to do anything here reguarding fuel and smelting....

        //objInv.GetItemList().Add(heldItem);//adds full stack but is removed and turned into a single item anyways so maybe change in future
        Debug.Log(" held item amount is " + heldItem.amount);
        Item tempItem = new Item() { amount = heldItem.amount, itemSO = heldItem.itemSO};//must create new item, if we dont then both variables share same memory location and both values change at same time
        tempItem.amount = 1;
        Debug.Log(" held item amount is " + heldItem.amount);
        //objInv.SimpleAddItem(tempItem);

        _realObj.GetComponent<KilnBehavior>().ReceiveItem(tempItem);

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

        pointerImage.sprite = _item.itemSO.itemSprite;
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
        if (heldItem.amount <= 0)
        {
            heldItem = null;
            StopHoldingItem();
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
                    RealItem.SpawnRealItem(transform.position, heldItem, false, true, heldItem.ammo);
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
        handSlot.UpdateDurability(equippedHandItem.uses);
        if (equippedHandItem.uses <= 0)
        {
            BreakItem(equippedHandItem);
        }
    }

    public void EquipItem(Item item)
    {
        if (handSlot.currentItem != null && item.equipType == Item.EquipType.HandGear)//hand + hand = swap, null + hand = equip hand
        {
            RealItem.SpawnRealItem(transform.position, handSlot.currentItem, false, true, handSlot.currentItem.ammo);
            isHandItemEquipped = true;
            UpdateEquippedItem(item, handSlot);
        }
        else if (handSlot.currentItem == null && item.equipType == Item.EquipType.HandGear)
        {
            isHandItemEquipped = true;
            UpdateEquippedItem(item, handSlot);
        }
        else if (headSlot.currentItem != null && item.equipType == Item.EquipType.HeadGear)
        {
            RealItem.SpawnRealItem(transform.position, headSlot.currentItem, false, true, headSlot.currentItem.ammo);
            isHeadItemEquipped = true;
            hpManager.currentArmor -= item.itemSO.armorValue;
            GetComponent<TemperatureReceiver>().ChangeInsulation(-item.itemSO.insulationValue);
            UpdateEquippedItem(item, headSlot);
        }
        else if (headSlot.currentItem == null && item.equipType == Item.EquipType.HeadGear)
        {
            isHeadItemEquipped = true;
            UpdateEquippedItem(item, headSlot);
        }
        else if (chestSlot.currentItem != null && item.equipType == Item.EquipType.ChestGear)
        {
            RealItem.SpawnRealItem(transform.position, chestSlot.currentItem, false, true, chestSlot.currentItem.ammo);
            isChestItemEquipped = true;
            hpManager.currentArmor -= item.itemSO.armorValue;
            GetComponent<TemperatureReceiver>().ChangeInsulation(-item.itemSO.insulationValue);
            UpdateEquippedItem(item, chestSlot);
        }
        else if (chestSlot.currentItem == null && item.equipType == Item.EquipType.ChestGear)
        {
            isChestItemEquipped = true;
            UpdateEquippedItem(item, chestSlot);
        }
        else if (leggingsSlot.currentItem != null && item.equipType == Item.EquipType.LegGear)
        {
            RealItem.SpawnRealItem(transform.position, leggingsSlot.currentItem, false, true, leggingsSlot.currentItem.ammo);
            isLeggingItemEquipped = true;
            hpManager.currentArmor -= item.itemSO.armorValue;
            GetComponent<TemperatureReceiver>().ChangeInsulation(-item.itemSO.insulationValue);
            UpdateEquippedItem(item, leggingsSlot);
        }
        else if (leggingsSlot.currentItem == null && item.equipType == Item.EquipType.LegGear)
        {
            isLeggingItemEquipped = true;
            UpdateEquippedItem(item, leggingsSlot);
        }
        else if (feetSlot.currentItem != null && item.equipType == Item.EquipType.FootGear)
        {
            RealItem.SpawnRealItem(transform.position, feetSlot.currentItem, false, true, feetSlot.currentItem.ammo);
            isFootItemEquipped = true;
            hpManager.currentArmor -= item.itemSO.armorValue;
            GetComponent<TemperatureReceiver>().ChangeInsulation(-item.itemSO.insulationValue);
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
        hpManager.currentArmor += _equipSlot.currentItem.itemSO.armorValue;
        GetComponent<TemperatureReceiver>().ChangeInsulation(_item.itemSO.insulationValue);

        if (_item.equipType == Item.EquipType.HandGear)
        {
            aimingSprite.sprite = null;

            if (doAction == Action.ActionType.Till)//previous doAction
            {
                deploySprite.sprite = null;
            }

            if (_item.itemSO.actionType != 0)
            {
                doAction = _item.itemSO.actionType;
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
        }
        else if (_item.equipType == Item.EquipType.HeadGear)//change bonuses like defense, insulation etc
        {
            headSpr.sprite = _item.itemSO.itemSprite;//change to equipSprite later
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
            Vector3 _look = playerController.body.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));//change from transform to body transform cuz player transform no longer mirrors
            _look = new Vector3(_look.x-1.1f, _look.y-5, _look.z);//fixing origin point for rotation
            float _angle = Mathf.Atan2(_look.y, _look.x) * Mathf.Rad2Deg;
            aimingTransform.rotation = Quaternion.Euler(new Vector3(0, 0, _angle));
            if (isMirrored)
            {
                aimingTransform.localRotation = new Quaternion(aimingTransform.localRotation.x * -1.0f,//setting w to -1 will mirror the rotation because quaternions... gotta study those ngl
                                            aimingTransform.localRotation.y,
                                            aimingTransform.localRotation.z,
                                            aimingTransform.localRotation.w * -1.0f);
                //aimingTransform.position = new Vector3(-aimingTransform.position.x, aimingTransform.position.y, aimingTransform.position.z);
            }
            else
            {
                //aimingTransform.localScale = new Vector3(1, 1, 1);
                //aimingTransform.position = new Vector3(-aimingTransform.position.x, aimingTransform.position.y, aimingTransform.position.z);
            }
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
                isHandItemEquipped = false;
                rightHandSprite.sprite = null;
                aimingSprite.sprite = null;
                equippedHandItem = null;
                isAiming = false;
            }
            else if (_equipSlot.currentItem.equipType == Item.EquipType.HeadGear)
            {
                isHeadItemEquipped = false;
                headSpr.sprite = null;
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

            hpManager.currentArmor -= _equipSlot.currentItem.itemSO.armorValue;
            GetComponent<TemperatureReceiver>().ChangeInsulation(-_equipSlot.currentItem.itemSO.insulationValue);
            if (_equipSlot.currentItem.itemSO.actionType != 0)//so that unequipping clothes dont fuck u
            {
                doAction = 0;
            }
            _equipSlot.RemoveItem();
            _equipSlot.ResetHoverText();
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
            hpManager.TakeDamage(-_item.itemSO.restorationValues[0]);
        }
        else
        {
            hpManager.RestoreHealth(_item.itemSO.restorationValues[0]);
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
        RealItem.SpawnRealItem(transform.position, itemToDeploy, false);
        itemToDeploy = null;
        deploySprite.color = new Color(1, 1, 1, 1);
        //pointerImage.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        deploySprite.sprite = null;
    }

    public IEnumerator DeployItem(Item _item)
    {
        if (!currentlyDeploying && itemToDeploy != null)
        {
            playerAnimator.SetBool("isDeploying", true);
            deploySprite.sprite = null;
            currentlyDeploying = true;
            if (isDeploying && _item.itemSO.isWall)
            {
                Vector3 newPos = transform.position;
                newPos = new Vector3(Mathf.Round(newPos.x / 6.25f) * 6.25f, Mathf.Round(newPos.y / 6.25f) * 6.25f, 1);
                RealWorldObject obj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = _item.itemSO.deployObject });
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
                currentlyDeploying = false;
                isDeploying = false;
                playerAnimator.SetBool("isDeploying", false);
                yield return null;
            }

            int x = 0;
            while (x < 20)
            {
                yield return new WaitForSeconds(.1f);
                if (!isDeploying)
                {
                    Debug.LogError("STOPPED DEPLOYING");
                    isDeploying = false;
                    currentlyDeploying = false;
                    deploySprite.sprite = itemToDeploy.itemSO.itemSprite;
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
        homeArrow.GetComponent<HomeArrow>().SetHome(_home.transform.position);
    }

    public void TillLand()
    {
        Vector3 newPos = transform.position;
        newPos = new Vector3(Mathf.Round(newPos.x / 6.25f) * 6.25f, Mathf.Round(newPos.y / 6.25f) * 6.25f, 1);
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
            while (obj.actionsLeft > 0 && doingAction && doAction == obj.objectAction && _item.uses > 0 && obj != null)//if u click at right timing u can chop twice... maybe keep it its kinda cool like a mini rthym game to work faster
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
                    light2D.pointLightOuterRadius = (20f / equippedHandItem.itemSO.maxUses * equippedHandItem.uses) + 5f;
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
        Gizmos.DrawWireSphere(origin.position, atkRange);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y+2.5f), collectRange);

    }
}
