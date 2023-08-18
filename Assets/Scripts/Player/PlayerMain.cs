using System;
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

    public HealthBar healthBar;
    public HungerBar hungerBar;
    internal HungerManager hungerManager;
    internal int currentHealth;
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

    private bool isItemEquipped = false;
    public Item equippedHandItem = null;
    public bool hoveringOverSlot = false;
    public bool isAiming = false;
    public Transform aimingTransform;

    public Item heldItem = null;
    public bool givingItem = false;
    public bool holdingFuel = false;
    public bool holdingValidSmeltItem = false;

    public bool deployMode = false;
    public bool isDeploying = false;
    public bool currentlyDeploying = false;
    public Item itemToDeploy = null;

    public GameObject pfProjectile;

    public bool goingToLight = false;

    public AudioManager audio;

    public Action.ActionType doAction = Action.ActionType.Default;


    [SerializeField] Transform rightHand;

    [SerializeField] private UI_CraftMenu_Controller uiCrafter;
    [SerializeField] private UI_Inventory uiInventory;
    [SerializeField] internal Crafter crafter;
    [SerializeField] private UI_HandSlot handSlot;
    [SerializeField] private SpriteRenderer rightHandSprite;
    [SerializeField] private SpriteRenderer aimingSprite;
    [SerializeField] public Transform pointer;
    [SerializeField] public SpriteRenderer pointerImage;
    [SerializeField] internal CombinationManager combinationManager;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        hungerBar.SetMaxHunger(maxHunger);
        hungerManager = GetComponent<HungerManager>();
        hungerManager.SetMaxHunger(maxHunger);
        StartCoroutine(hungerManager.DecrementHunger());
        hungerManager.onStarvation += Starve;

        inventory = new Inventory();
        uiInventory.SetInventory(inventory);
        crafter.SetInventory(inventory);
        uiCrafter.SetInventory(inventory);

        //RealMob.SpawnMob(new Vector3(4, 4), new Mob { mobType = Mob.MobType.Bunny });
        //RealMob.SpawnMob(new Vector3(-40, 0), new Mob { mobType = Mob.MobType.Wolf });
        /*
        RealWorldObject.SpawnWorldObject(new Vector3(2, 2), new WorldObject { objType = WorldObject.worldObjectType.HotCoals});

        RealItem.SpawnRealItem(new Vector3(2, 2), new Item { itemType = Item.ItemType.Charcoal, amount = 6 });
        RealItem.SpawnRealItem(new Vector3(-3, 2), new Item { itemType = Item.ItemType.Twig, amount = 1 });
        RealItem.SpawnRealItem(new Vector3(3, -2), new Item { itemType = Item.ItemType.Rock, amount = 7 });
        RealItem.SpawnRealItem(new Vector3(4, -2), new Item { itemType = Item.ItemType.Twig, amount = 11 });
        RealItem.SpawnRealItem(new Vector3(2, -4), new Item { itemType = Item.ItemType.Rock, amount = 15 });
        RealItem.SpawnRealItem(new Vector3(2, -2), new Item { itemType = Item.ItemType.Clay, amount = 20});
        RealItem.SpawnRealItem(new Vector3(-2, -2), new Item { itemType = Item.ItemType.Axe, amount = 1 });
        RealItem.SpawnRealItem(new Vector3(-4, 5), new Item { itemType = Item.ItemType.Twig, amount = 15 });
        RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemType = Item.ItemType.Arrow, amount = 13 });
        RealItem.SpawnRealItem(new Vector3(12, 2), new Item { itemType = Item.ItemType.WoodenClub, amount = 1 });
        RealItem.SpawnRealItem(new Vector3(10, -15), new Item { itemType = Item.ItemType.RawMeat, amount = 4 });
        RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemType = Item.ItemType.RawRabbit, amount = 6 });
        RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemType = Item.ItemType.Fiber, amount = 20 });
        RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemType = Item.ItemType.RawDrumstick, amount = 6 });
        RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemType = Item.ItemType.ClayBowl, amount = 6 });*/
    }

    private void Update()
    {
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
        Aim();
    }

    public void CheckDeath()
    {
        if (currentHealth <= 0)
        {
            Debug.Log("poof");
            inventory.DropAllItems(transform.position);
            uiInventory.RefreshInventoryItems();
            handSlot.RemoveItem();
            Destroy(gameObject);
        }
    }

    public void Starve(object sender, System.EventArgs e)
    {
            currentHealth--;
            healthBar.SetHealth(currentHealth);
            CheckDeath();
    }

    public void TakeDamage(int _damage)
    {
        currentHealth -= _damage;
        healthBar.SetHealth(currentHealth);
        CheckDeath();
    }

    public IEnumerator ThrowWeapon()
    {
        yield return new WaitForSeconds(1f);
    }

    public void CombineHandItem()
    {
       //Debug.Log(equippedHandItem.ammo);
       if (equippedHandItem.NeedsAmmo() && heldItem.isAmmo() && equippedHandItem.GetMaxAmmo() > equippedHandItem.ammo && heldItem.itemType == equippedHandItem.ValidAmmo())//if needs ammo, is ammo, item max ammo is bigger than current ammo, and held itemtype is valid ammo for equippeditem
        {
            equippedHandItem.ammo++;
            heldItem.amount--;
            rightHandSprite.sprite = null;
            aimingSprite.sprite = equippedHandItem.GetLoadedHandSprite();
            handSlot.UpdateSprite(equippedHandItem.GetLoadedSprite());
            handSlot.ResetHoverText();
            isAiming = true;
        }
       if (heldItem.amount == 0)
       {
           isHoldingItem = false;//might be bad doing all of these but idk, gotta be safe
           holdingFuel = false;
           holdingValidSmeltItem = false;
           pointerImage.sprite = null;
           givingItem = false;
           heldItem = null;
       }
       //UpdateEquippedItem(equippedHandItem);
    }

    public void Shoot()
    {
        if (isAiming && equippedHandItem.ammo > 0)
        {
            equippedHandItem.ammo--;
            UpdateEquippedItem(equippedHandItem);
            var _projectile = Instantiate(pfProjectile, aimingTransform.transform.position, aimingTransform.rotation);
            _projectile.GetComponent<ProjectileManager>().SetProjectile(new Item { itemType = equippedHandItem.ValidAmmo(), amount = 1 }, transform.position, false);
            if (isMirrored)
            {
                _projectile.GetComponent<Rigidbody2D>().velocity = -aimingTransform.right * 100;
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
        if (doAction == Action.ActionType.Throw && isAiming)
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
            isItemEquipped = false;
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
        if (!isAttacking && !doingAction)
        {
            Debug.Log("player attacking");
            animator.Play("Melee");
            playerController.target = transform.position;
            isAttacking = true;

            yield return new WaitForSecondsRealtime(.25f);
            Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(origin.position, atkRange);
            foreach (Collider2D _enemy in _hitEnemies)
            {
                if (_enemy.CompareTag("Enemy") && _enemy != null)//if becomes null and sends error, script stops and we never set attacking to false???? no i have no idea actually
                {
                    _enemy.GetComponent<HealthManager>().TakeDamage(damage);
                    UseItemDurability();
                    break;
                }
                else if (_enemy.CompareTag("Mob") && _enemy != null)
                {
                    _enemy.GetComponent<HealthManager>().TakeDamage(damage);
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
            Debug.Log("busy attacking");
        }
    }

    public IEnumerator OnObjectSelected(Action.ActionType objAction, Transform worldObj, WorldObject obj, GameObject realObj)
    {
        if (doAction != Action.ActionType.Throw && doAction != Action.ActionType.Shoot && doAction != Action.ActionType.Melee)//if not holding weapon
        {
            if (obj.IsInteractable() && isHoldingItem)
            {
                foreach (Item.ItemType _item in obj.GetAcceptableFuelGiven())
                {
                    if (_item == heldItem.itemType)
                    {
                        Debug.Log("giving item");
                        yield return new WaitForSeconds(.1f);
                        playerController.target = worldObj.position;
                        givingItem = true;
                        holdingFuel = true;
                        break;
                    }
                }
                foreach (Item.ItemType _item in obj.GetAcceptableSmeltingItems())
                {
                    if (_item == heldItem.itemType)
                    {
                        Debug.Log("giving item");
                        yield return new WaitForSeconds(.1f);
                        playerController.target = worldObj.position;
                        givingItem = true;
                        holdingValidSmeltItem = true;
                        break;
                    }
                }
                if (heldItem.itemType == Item.ItemType.Clay)//change to item.getSealingItem()
                {
                    Debug.Log("giving item");
                    yield return new WaitForSeconds(.1f);
                    playerController.target = worldObj.position;
                    givingItem = true;
                }
                else if (objAction == Action.ActionType.Cook && !realObj.GetComponent<HotCoalsBehavior>().isCooking)
                {
                    if (heldItem.IsCookable())
                    {
                        Debug.Log("giving item");
                        yield return new WaitForSeconds(.1f);
                        playerController.target = worldObj.position;
                        givingItem = true;
                    }
                }
                else if (objAction == Action.ActionType.Scoop && heldItem.GetDoableAction() == objAction)
                {
                    Debug.Log("giving item");
                    yield return new WaitForSeconds(.1f);
                    playerController.target = worldObj.position;
                    givingItem = true;
                }
                else if (objAction == Action.ActionType.Water && heldItem.GetDoableAction() == objAction)
                {
                    Debug.Log("giving item");
                    yield return new WaitForSeconds(.1f);
                    playerController.target = worldObj.position;
                    givingItem = true;
                }
            }
            else if (objAction == 0)
            {
                Debug.Log("default action");
                yield return new WaitForSeconds(.1f);
                playerController.target = worldObj.position;
                doingAction = true;
            }
            else if (obj.IsInteractable() && doAction == Action.ActionType.Burn)
            {
                yield return new WaitForSeconds(.1f);
                playerController.target = worldObj.position;
                goingToLight = true;
            }
            else if (objAction == doAction)//make it so if were clicking same object, dont spazz around lol
            {
                Debug.Log("correct action");
                yield return new WaitForSeconds(.1f);
                playerController.target = worldObj.position;
                doingAction = true;
            }
            //else if ()
            else//if action mismatch or default action
            {
                //examine maybe or somethin else.....
            }
        }
    }

    public void GiveItem(Collider2D _realObj)//only do this if object accepts item
    {
        Inventory objInv = _realObj.GetComponent<RealWorldObject>().inventory;

        //actually i dont think i need to do anything here reguarding fuel and smelting....

        //objInv.GetItemList().Add(heldItem);//adds full stack but is removed and turned into a single item anyways so maybe change in future
        Debug.Log(" held item amount is " + heldItem.amount);
        Item tempItem = new Item() { amount = heldItem.amount, itemType = heldItem.itemType};//must create new item, if we dont then both variables share same memory location and both values change at same time
        tempItem.amount = 1;
        Debug.Log(" held item amount is " + heldItem.amount);
        objInv.SimpleAddItem(tempItem);
        heldItem.amount--;
        if (heldItem.amount <= 0)
        {
            heldItem = null;
            StopHoldingItem();
        }
        givingItem = false;
    }

    public void UseItem(Item item)
    {
        if (item.isEquippable())
        {
            EquipItem(item);
            return;
        }
        else if (item.isEatable())
        {
            EatItem(item);
            return;
        }
        else if (item.isDeployable())
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
            isHoldingItem = true;
            heldItem = _item;
            pointerImage.sprite = _item.GetSprite();
        }
    }

    public void StopHoldingItem()
    {
        if (isHoldingItem)
        {
            Debug.Log("not holding anymore");
            if (heldItem != null)
            {
                RealItem.SpawnRealItem(transform.position, heldItem, false, true, heldItem.ammo);
            }
            isHoldingItem = false;
            holdingFuel = false;
            holdingValidSmeltItem = false;
            pointerImage.sprite = null;
            givingItem = false;
            heldItem = null;
        }
    }

    private void UseItemDurability()
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
        if (!isItemEquipped)
        {
            isItemEquipped = true;
        }
        else
        {
            Debug.Log("swap item");
            RealItem.SpawnRealItem(transform.position, equippedHandItem, false, true, equippedHandItem.ammo);
        }
        UpdateEquippedItem(item);
    }

    public void UpdateEquippedItem(Item _item)
    {
        aimingSprite.sprite = null;
        doAction = _item.GetDoableAction();
        rightHandSprite.sprite = _item.GetSprite();
        equippedHandItem = _item;
        handSlot.SetItem(_item, _item.uses);
        if (_item.ammo > 0)
        {
            handSlot.UpdateSprite(_item.GetLoadedSprite());
            rightHandSprite.sprite = null;
            aimingSprite.sprite = equippedHandItem.GetLoadedHandSprite();
        }
        else if (doAction == Action.ActionType.Shoot || doAction == Action.ActionType.Throw)
        {
            rightHandSprite.sprite = null;
            aimingSprite.sprite = equippedHandItem.GetAimingSprite();
        }
        if (doAction == Action.ActionType.Shoot || doAction == Action.ActionType.Throw)
        {
            isAiming = true;
        }
        else if (doAction == Action.ActionType.Burn)
        {
            StartCoroutine(DoBurnAction());
        }
    }

    private void Aim()
    {
        if (isAiming)
        {
            Vector3 _look = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));//cant change rotation if we have an animator animating transforms
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

    public void UnequipItem()
    {
        if (isItemEquipped)
        {
            isItemEquipped = false;
            RealItem.SpawnRealItem(transform.position, equippedHandItem, false, true, equippedHandItem.ammo);
            handSlot.RemoveItem();
            doAction = 0;
            rightHandSprite.sprite = null;
            aimingSprite.sprite = null;
            equippedHandItem = null;
            handSlot.ResetHoverText();
        }
    }

    public void EatItem(Item _item)
    {
        hungerManager.AddHunger(_item.GetCalories());
        Debug.Log("ate " + _item);
    }

    public void SetDeployItem(Item _item)
    {
        deployMode = true;
        itemToDeploy = _item;
        pointerImage.sprite = itemToDeploy.GetSprite();//change to object sprite because items will have diff sprites blah blah blah
    }

    public void UnDeployItem()
    {
        isDeploying = false;
        deployMode = false;
        RealItem.SpawnRealItem(transform.position, itemToDeploy, false);
        itemToDeploy = null;
        pointerImage.sprite = null;
    }

    public IEnumerator DeployItem(Item _item)
    {
        if (!currentlyDeploying)
        {
            currentlyDeploying = true;
            yield return new WaitForSeconds(2f);
            if (isDeploying)
            {
                RealWorldObject.SpawnWorldObject(transform.position, new WorldObject { objType = _item.GetDeployableObject() });
                pointerImage.sprite = null;
                deployMode = false;
            }
            currentlyDeploying = false;
        }
    }

    public void BreakItem(Item _item)
    {
        equippedHandItem = null;
        rightHandSprite.sprite = null;
        isItemEquipped = false;
        doAction = 0;
        handSlot.RemoveItem();
        Debug.Log("broke tool");
    }

    public IEnumerator DoAction(Action.ActionType action, RealWorldObject obj, Item _item)
    {
        if (obj.objectAction == 0)
        {
            while (obj.actionsLeft > 0 && doingAction)
            {
                yield return new WaitForSeconds(.5f);
                if (doingAction)
                {
                    obj.GetActionedOn();
                }
            }
        }
        animateWorking = true;
        playerController.target = transform.position;
        while (obj.actionsLeft > 0 && doingAction && doAction == obj.objectAction && _item.uses > 0)
        {
            yield return new WaitForSeconds(.5f);

            int randVal = Random.Range(1, 4);
            if (randVal == 1)
            {
                audio.Play("Chop1");
            }
            else if (randVal == 2)
            {
                audio.Play("Chop2");
            }
            else if (randVal == 3)
            {
                audio.Play("Chop3");
            }

            obj.GetActionedOn();
            UseItemDurability();
        }
        animateWorking = false;
    }

    public IEnumerator DoBurnAction()
    {
        if (doAction == Action.ActionType.Burn)
        {
            UseItemDurability();
            light2D.pointLightOuterRadius -= light2D.pointLightOuterRadius / equippedHandItem.GetMaxItemUses();
        }
        if (doAction == Action.ActionType.Burn)
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(DoBurnAction());
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("WorldObject"))
        {
            RealWorldObject realObj = collision.gameObject.GetComponent<RealWorldObject>();

            if (realObj.obj.IsInteractable())
            {
                if (!realObj.isClosed)//if open
                {
                    if (givingItem)
                    {
                        if (heldItem.isSmeltable() && !realObj.GetComponent<KilnBehavior>().isSmeltingItem)//if smeltable 
                        {
                            GiveItem(collision.collider);
                            playerController.target = transform.position;
                        }
                        else if (heldItem.IsFuel())//if fuel
                        {
                            GiveItem(collision.collider);
                            playerController.target = transform.position;
                        }
                        else if (heldItem.itemType == Item.ItemType.Clay && realObj.GetComponent<Smelter>().isSmelting)//change to sealing item, also make it so we can seal and unseal whenever we want, cuz game design ya know?
                        {
                            GiveItem(collision.collider);
                            playerController.target = transform.position;
                        }
                        else if (heldItem.IsCookable() && realObj.objectAction == Action.ActionType.Cook && !realObj.GetComponent<HotCoalsBehavior>().isCooking)
                        {
                            realObj.Cook(heldItem);
                            GiveItem(collision.collider);
                            playerController.target = transform.position;
                        }
                        else if (realObj.objectAction == Action.ActionType.Scoop && heldItem.GetDoableAction() == realObj.objectAction)
                        {
                            realObj.actionsLeft--;
                            if (realObj.objType == WorldObject.worldObjectType.Pond)
                            {
                                heldItem.amount--;
                                Item _item = new Item { amount = 1, itemType = Item.ItemType.BowlOfWater };
                                RealItem.SpawnRealItem(transform.position, _item, false);
                            }
                            realObj.CheckBroken();
                        }
                        else if (realObj.objectAction == Action.ActionType.Water && heldItem.GetDoableAction() == realObj.objectAction)
                        {
                            heldItem.itemType = Item.ItemType.ClayBowl;
                            realObj.actionsLeft = 0;
                            realObj.CheckBroken();
                            pointerImage.sprite = heldItem.GetSprite();
                        }
                    }
                    else if (!givingItem && goingToLight)//going to light smelter
                    {
                        realObj.StartSmelting();
                        goingToLight = false;
                    }
                }
            }
            else if (realObj.objectAction == doAction && doingAction)
            {
                StartCoroutine(DoAction(doAction, realObj, equippedHandItem));
                playerController.target = transform.position;
            }
            else if (realObj.objectAction == 0 && doingAction)
            {
                StartCoroutine(DoAction(doAction, realObj, equippedHandItem));
                playerController.target = transform.position;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(origin.position, atkRange);
    }
}
