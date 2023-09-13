using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    PlayerMain main;
    [SerializeField]
    public float speed = 5f;
    Vector2 movement;
    [SerializeField]
    GameObject uiMenu;
    [SerializeField]
    GameObject uiHUD;
    internal Rigidbody2D rb;
    public Vector3 target;
    public GameObject HoverText;
    public GameObject itemAmountText;
    public TextMeshProUGUI txt;
    public AudioManager audio;
    public Vector3 deployPos;

    public NightEventManager nightEvent;

    //public bool isMovingToObject

    [SerializeField] private Animator craftingUIanimator;

    private bool uiActive = false;
    private bool uiHUDActive = true;

    public event EventHandler onMoved;
 
    public GameObject pauseMenu;

    public bool freeCrafting = false;

    public Transform body;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //uiMenu.SetActive(false);
        HoverText = GameObject.FindGameObjectWithTag("HoverText");
        txt = HoverText.GetComponent<TextMeshProUGUI>();
        pauseMenu.SetActive(false);
        craftingUIanimator.SetBool("Open", false);
        craftingUIanimator.SetBool("Close", true);
        main.playerAnimator.SetBool("isWalking", false);
    }


    // Update is called once per frame
    void Update()
    {

        //Movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.x == -1 && !main.isAttacking)
        {
            body.localScale = new Vector3(-1, 1, 1);
            main.isMirrored = true;
        }
        else if (movement.x == 1 && !main.isAttacking)
        {
            body.localScale = new Vector3(1, 1, 1);
            main.isMirrored = false;
        }

        Vector3 playerPos = transform.position;
        //playerPos.z = playerPos.y;
        transform.position = playerPos;

        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.C))
        {
            OpenCloseCraftingTab();
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            StartCoroutine(nightEvent.SummonDepthWalkers(true));
            Announcer.SetText("SUMMONING DEPTH WALKERS", Color.red);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseMenu.activeSelf)
            {
                audio.Pause("Music1");
                audio.Pause("Music2");
                txt.text = "";
                pauseMenu.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                pauseMenu.SetActive(false);
                Time.timeScale = 1f;
                audio.UnPause("Music1");
                audio.Pause("Music2");
                txt.text = "";
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!main.godMode)
            {
                Announcer.SetText("GOD MODE ENABLED");
                main.godMode = true;
            }
            else
            {
                Announcer.SetText("GOD MODE DISABLED");
                main.godMode = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (!freeCrafting)
            {
                Announcer.SetText("FREE CRAFTING ENABLED");
                freeCrafting = true;
                audio.Play("KilnLight1");
            }
            else
            {
                Announcer.SetText("FREE CRAFTING DISABLED");
                freeCrafting = false;
                audio.Stop("KilnLight1");
                audio.Play("KilnOut");
            }

        }

        if (Input.GetKeyDown(KeyCode.F3))//cheats
        {
            Announcer.SetText("ITEMS SPAWNED");
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x+ 5, main.transform.position.y+2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("BronzeAxe"), amount = 1 });
            //RealItem.SpawnRealItem(new Vector3(3, -2), new Item { itemSO = ItemObjectArray.Instance.RawCopper, amount = 7 });
            //RealItem.SpawnRealItem(new Vector3(4, -2), new Item { itemSO = ItemObjectArray.Instance.Twig, amount = 11 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 2, main.transform.position.y + -4), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Rock"), amount = 15 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 2, main.transform.position.y + -2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Clay"), amount = 20 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + -4, main.transform.position.y + 5), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Twig"), amount = 15 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + -6, main.transform.position.y + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Arrow"), amount = 13 });
            //RealItem.SpawnRealItem(new Vector3(12, 2), new Item { itemSO = ItemObjectArray.Instance.WoodenClub, amount = 1 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 10, main.transform.position.y + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawMeat"), amount = 4 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + -6, main.transform.position.y + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawRabbit"), amount = 6 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + -6, main.transform.position.y + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Fiber"), amount = 20 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + -6, main.transform.position.y + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawDrumstick"), amount = 6 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + -6, main.transform.position.y + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 10 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + -6, main.transform.position.y + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("DeadBunny"), amount = 10 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 12, main.transform.position.y + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Bone"), amount = 2 });
            //RealItem.SpawnRealItem(new Vector3(10, -15), new Item { itemSO = ItemObjectArray.Instance.RawTin, amount = 4 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 10, main.transform.position.y + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("TinIngot"), amount = 4 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 10, main.transform.position.y + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("CopperIngot"), amount = 4 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 10, main.transform.position.y + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 4 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 15, main.transform.position.y + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Log"), amount = 20 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 15, main.transform.position.y + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Charcoal"), amount = 20 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 25, main.transform.position.y + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("BronzeIngot"), amount = 1}, true, false, 0, true);
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 25, main.transform.position.y + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("BagBellows"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 35, main.transform.position.y + -5), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawGold"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + 45, main.transform.position.y + -25), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawMutton"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(main.transform.position.x + -25, main.transform.position.y + -45), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("SheepWool"), amount = 1 });
            RealMob.SpawnMob(new Vector3(main.transform.position.x + 25, main.transform.position.y + 25), new Mob { mobSO = MobObjArray.Instance.SearchMobList("Wolf") });
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            audio.Play($"Music2", true);
        }


        if (main.isAiming && main.doAction == Action.ActionType.Shoot && main.equippedHandItem.ammo > 0)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            txt.text = "Shoot";
        }
        else if (main.isAiming && main.doAction == Action.ActionType.Throw)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            txt.text = "Throw";
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Collider2D[] _objList = Physics2D.OverlapCircleAll(transform.position, 25);
            _objList = _objList.OrderBy((d) => (d.transform.position - transform.position).sqrMagnitude).ToArray();//bro lambda expressions are black magic
            foreach (Collider2D _obj in _objList)
            {
                if (_obj.GetComponent<RealWorldObject>() != null)
                {
                    if (_obj.GetComponent<RealWorldObject>().objectAction == main.doAction || _obj.GetComponent<RealWorldObject>().objectAction == Action.ActionType.Default)//dont return if we dont have same action so we can find next available obj to action on
                    {
                        main.OnObjectSelected(_obj.GetComponent<RealWorldObject>().objectAction, _obj.transform, _obj.GetComponent<RealWorldObject>().obj, _obj.gameObject);
                        return;
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && main.doAction == Action.ActionType.Melee && !main.isAttacking && !main.deployMode)
        {
            StartCoroutine(main.Attack());
        }

        if (Input.GetMouseButton(0))//changing to holding down seems to break a lot of things...... LEFT CLICK
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            else if (main.deployMode)
            {
                main.isDeploying = true;
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    deployPos = main.pointer.transform.position;
                    deployPos.z = 1;
                }
                else if (!Input.GetKey(KeyCode.LeftControl))
                {
                    deployPos = new Vector3(Mathf.Round(main.pointer.transform.position.x / 6.25f) * 6.25f, Mathf.Round(main.pointer.transform.position.y / 6.25f) * 6.25f, 1);
                }
                MoveToMouse();
            }
            else if (main.doAction == Action.ActionType.Melee && !main.deployMode)
            {
                StartCoroutine(main.Attack());
            }
            else if (main.doAction == Action.ActionType.Shoot && !main.deployMode)
            {
                main.Shoot();
            }
            else if (main.doAction == Action.ActionType.Throw && !main.deployMode)
            {
                main.Throw();
            }
            else if (main.doAction != Action.ActionType.Melee && !main.deployMode && main.doAction != Action.ActionType.Shoot && main.doAction != Action.ActionType.Throw)//go to mouse position
            {
                main.isDeploying = false;
                main.pointerImage.transform.localPosition = Vector3.forward;
                MoveToMouse();
            }
        }

        if (target == transform.position && main.isDeploying && !main.currentlyDeploying)
        {
            StartCoroutine(main.DeployItem(main.itemToDeploy));
        }

        if (Input.GetMouseButtonDown(1))//RIGHT CLICK
        {
            if (main.deployMode)
            {
                main.UnDeployItem();
                main.pointerImage.transform.localPosition = Vector3.forward;
            }
            else if (main.isHoldingItem && !main.hoveringOverSlot)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                main.StopHoldingItem();
                txt.text = "";
            }
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (main.doAction == Action.ActionType.Melee && !main.isAttacking || main.doAction == Action.ActionType.Shoot && !main.isAttacking || main.doAction == Action.ActionType.Throw && !main.isAttacking)
            {
                if (!main.isHoldingItem)
                {
                    MoveToMouse();
                }
            }
        }
    }

    public void OpenCloseCraftingTab()
    {
        if (uiActive)
        {
            //uiHUD.SetActive(true);
            //uiHUDActive = true;
            //uiMenu.SetActive(false);
            craftingUIanimator.SetBool("Open", false);
            craftingUIanimator.SetBool("Close", true);
            uiActive = false;
        }
        else
        {
            //uiHUD.SetActive(false);
            //uiHUDActive = false;
            //uiMenu.SetActive(true);
            craftingUIanimator.SetBool("Open", true);
            craftingUIanimator.SetBool("Close", false);
            uiActive = true;
        }
    }

    public void ChangeTarget(Vector3 _target)
    {
        target = _target;
        Vector3 tempPosition = transform.position - target;//if we moving right turn right, moving left turn left, moving straight vertically or not moving at all do nothing
                                                           //Debug.Log(transform.position);
        if (tempPosition.x < 0)
        {
            body.localScale = new Vector3(1, 1, 1);
            main.isMirrored = false;
        }
        else if (tempPosition.x > 0)
        {
            body.localScale = new Vector3(-1, 1, 1);
            main.isMirrored = true;
        }
    }

    private void Moved()
    {
        onMoved?.Invoke(this, EventArgs.Empty);
    }

    private void MoveToMouse()
    {
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

        //if we click object, dont move. this doesnt work tho so idk

        if (rayHit.collider != null)
        {
            if (rayHit.collider.CompareTag("WorldObject"))
            {
                //Debug.Log("hit worldOBJ");
                //target = Vector3.zero;
            }
            else if (rayHit.collider.CompareTag("Attachment"))
            {
                //Debug.Log("hit attachment");
            }
        }     
        else//move here
        {
            //Debug.Log("nuthin");
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = transform.position.z;
            main.doingAction = false;
            //main.animateWorking = false;
            //main.isDeploying = false;
            //main.goingToItem = false;
            if (main.currentlyDeploying)
            {
                main.isDeploying = false;
            }
            main.goingToCollect = false;
            main.givingItem = false;
            main.goingToLight = false;
            main.attachingItem = false;

            Invoke("Moved", .01f);

            Vector3 tempPosition = transform.position - target;//if we moving right turn right, moving left turn left, moving straight vertically or not moving at all do nothing
                                                               //Debug.Log(transform.position);
            if (tempPosition.x < 0)
            {
                body.localScale = new Vector3(1, 1, 1);
                main.isMirrored = false;
            }
            else if (tempPosition.x > 0)
            {
                body.localScale = new Vector3(-1, 1, 1);
                main.isMirrored = true;
            }
        }

        //target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //target.z = transform.position.z;




    }

    public void Chase(Transform _target)
    {
        target = _target.position;
        main.isAttacking = true;
    }

    public void MoveToTarget(Vector3 target)
    {
        if (target != Vector3.zero && Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

    private void CheckIfMoving()
    {
        if (transform.hasChanged && !main.currentlyWorking)
        {
            main.playerAnimator.SetBool("isWalking", true);
            transform.hasChanged = false;
        }
        else
        {
            main.playerAnimator.SetBool("isWalking", false);
        }
    }

    private void FixedUpdate()
    {
        CheckIfMoving();
        HoverText.transform.position = Input.mousePosition;
        HoverText.transform.position = new Vector3(HoverText.transform.position.x + 15, HoverText.transform.position.y - 15, HoverText.transform.position.z);
        itemAmountText.transform.position = Input.mousePosition;
        itemAmountText.transform.position = new Vector3(itemAmountText.transform.position.x + 5, itemAmountText.transform.position.y - 10, itemAmountText.transform.position.z);
        main.pointer.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector3 viewPos = main.pointer.transform.position;
        //viewPos.x = Mathf.Clamp(main.pointer.transform.position.x, Screen.width * -1 + 200, Screen.width - 200);
        //viewPos.y = Mathf.Clamp(main.pointer.transform.position.y, Screen.height * -1 + 200, Screen.height - 200);
        //main.pointer.transform.position = viewPos;
        if (main.deployMode && !Input.GetKey(KeyCode.LeftControl) && !main.itemToDeploy.itemSO.isWall)// if holding left control will NOT snap to a grid
        {
            Vector3 currentPos = main.pointer.transform.position;
            main.pointerImage.transform.localPosition = Vector3.forward;
            main.pointerImage.transform.position = new Vector3(Mathf.Round(currentPos.x / 6.25f) * 6.25f, Mathf.Round(currentPos.y / 6.25f) * 6.25f, 1);//these dont actually place where they SHOULD!!!
        }
        else if (main.deployMode && main.itemToDeploy.itemSO.isWall)//if is a wall, always snap >:(
        {
            Vector3 currentPos = main.pointer.transform.position;
            main.pointerImage.transform.localPosition = Vector3.forward;
            main.pointerImage.transform.position = new Vector3(Mathf.Round(currentPos.x / 6.25f) * 6.25f, Mathf.Round(currentPos.y / 6.25f) * 6.25f, 1);//fix this shid bruh
        }
        else if (main.deployMode && !main.itemToDeploy.itemSO.isWall && Input.GetKey(KeyCode.LeftControl))
        {
            main.pointerImage.transform.localPosition = Vector3.forward;
            main.pointer.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (!main.isAttacking)
        {
            rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
        }

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            target = Vector3.zero;
            onMoved?.Invoke(this, EventArgs.Empty);
            main.doingAction = false;
            main.animateWorking = false;
            main.isDeploying = false;
            main.goingToItem = false;
            main.goingToCollect = false;
            main.givingItem = false;
            main.goingToLight = false;
            main.attachingItem = false;
        }

        MoveToTarget(target);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        RealItem realItem = collider.GetComponent<RealItem>();
        if (realItem != null && !realItem.isHot)
        {
            main.inventory.AddItem(realItem.GetItem(), transform.position);
            realItem.DestroySelf();
            //realItem.DestroySelf();//figure out how to call destroy method when collected and not touched     
        }
    }
}
