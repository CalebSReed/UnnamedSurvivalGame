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
    public TextMeshProUGUI txt;
    //public bool isMovingToObject

    private bool uiActive = false;
    private bool uiHUDActive = true;

    public GameObject pauseMenu;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        uiMenu.SetActive(false);
        HoverText = GameObject.FindGameObjectWithTag("HoverText");
        txt = HoverText.GetComponent<TextMeshProUGUI>();
        pauseMenu.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {

        //Movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.x == -1 && !main.isAttacking)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            main.isMirrored = true;
        }
        else if (movement.x == 1 && !main.isAttacking)
        {
            transform.localScale = new Vector3(1, 1, 1);
            main.isMirrored = false;
        }

        Vector3 playerPos = transform.position;
        //playerPos.z = playerPos.y;
        transform.position = playerPos;

        if (Input.GetKeyDown(KeyCode.F))
        {
            main.currentHealth = 0;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (uiActive)
            {
                uiHUD.SetActive(true);
                uiHUDActive = true;
                uiMenu.SetActive(false);
                uiActive = false;
            }
            else
            {
                uiHUD.SetActive(false);
                uiHUDActive = false;
                uiMenu.SetActive(true);
                uiActive = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseMenu.activeSelf)
            {
                txt.text = "";
                pauseMenu.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                pauseMenu.SetActive(false);
                Time.timeScale = 1f;
                txt.text = "";
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))//cheats
        {
            RealItem.SpawnRealItem(new Vector3(2, 2), new Item { itemSO = ItemObjectArray.Instance.StoneAxe, amount = 1 });
            RealItem.SpawnRealItem(new Vector3(-3, 2), new Item { itemSO = ItemObjectArray.Instance.Twig, amount = 1 });
            RealItem.SpawnRealItem(new Vector3(3, -2), new Item { itemSO = ItemObjectArray.Instance.RawCopper, amount = 7 });
            RealItem.SpawnRealItem(new Vector3(4, -2), new Item { itemSO = ItemObjectArray.Instance.Twig, amount = 11 });
            RealItem.SpawnRealItem(new Vector3(2, -4), new Item { itemSO = ItemObjectArray.Instance.Rock, amount = 15 });
            RealItem.SpawnRealItem(new Vector3(2, -2), new Item { itemSO = ItemObjectArray.Instance.Clay, amount = 20 });
            RealItem.SpawnRealItem(new Vector3(-4, 5), new Item { itemSO = ItemObjectArray.Instance.Twig, amount = 15 });
            RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemSO = ItemObjectArray.Instance.Arrow, amount = 13 });
            RealItem.SpawnRealItem(new Vector3(12, 2), new Item { itemSO = ItemObjectArray.Instance.WoodenClub, amount = 1 });
            RealItem.SpawnRealItem(new Vector3(10, -15), new Item { itemSO = ItemObjectArray.Instance.RawMeat, amount = 4 });
            RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemSO = ItemObjectArray.Instance.RawRabbit, amount = 6 });
            RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemSO = ItemObjectArray.Instance.Fiber, amount = 20 });
            RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemSO = ItemObjectArray.Instance.RawDrumstick, amount = 6 });
            RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemSO = ItemObjectArray.Instance.ClayBowl, amount = 10 });
            RealItem.SpawnRealItem(new Vector3(-6, 2), new Item { itemSO = ItemObjectArray.Instance.DeadBunny, amount = 10 });
            RealItem.SpawnRealItem(new Vector3(12, 2), new Item { itemSO = ItemObjectArray.Instance.Bone, amount = 2 });
            RealItem.SpawnRealItem(new Vector3(10, -15), new Item { itemSO = ItemObjectArray.Instance.RawTin, amount = 4 });
            RealItem.SpawnRealItem(new Vector3(10, -15), new Item { itemSO = ItemObjectArray.Instance.TinIngot, amount = 4 });
            RealItem.SpawnRealItem(new Vector3(10, -15), new Item { itemSO = ItemObjectArray.Instance.CopperIngot, amount = 4 });
            RealItem.SpawnRealItem(new Vector3(10, -15), new Item { itemSO = ItemObjectArray.Instance.ClayPlate, amount = 4 });
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

        if (Input.GetMouseButtonDown(0))//changing to holding down seems to break a lot of things...... LEFT CLICK
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            else if (main.deployMode)
            {
                main.isDeploying = true;
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
                MoveToMouse();
            }
            else if (main.doAction == Action.ActionType.Throw)
            {
                StartCoroutine(main.ThrowWeapon());
            }
        }

        if (target == transform.position && main.isDeploying)
        {
            StartCoroutine(main.DeployItem(main.itemToDeploy));
        }
        if (Input.GetMouseButtonDown(1))//RIGHT CLICK
        {
            if (main.deployMode)
            {
                main.UnDeployItem();
            }
            else if (main.isHoldingItem && !main.hoveringOverSlot)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                main.StopHoldingItem();
            }
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (main.doAction == Action.ActionType.Melee && !main.isAttacking || main.doAction == Action.ActionType.Shoot && !main.isAttacking || main.doAction == Action.ActionType.Throw && !main.isAttacking)
            {
                MoveToMouse();
            }
        }
    }

    private void MoveToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.z = transform.position.z;
        main.doingAction = false;
        main.animateWorking = false;

        Vector3 tempPosition = transform.position - target;//if we moving right turn right, moving left turn left, moving straight vertically or not moving at all do nothing
        //Debug.Log(transform.position);
        if (tempPosition.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            main.isMirrored = false;
        }
        else if (tempPosition.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            main.isMirrored = true;
        }

        if (Physics.Raycast(ray, out hit, 100))//if we click object, dont move. this doesnt work tho so idk
        {
            Debug.Log("bro PLEASE");
            if (hit.collider.CompareTag("WorldObject"))
            {
                Debug.Log("hit");
                target = Vector3.zero;
            }
            else//move here
            {
                Debug.Log("nuthin");
                target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                target.z = transform.position.z;
            }
        }
    }

    public void Chase(Transform _target)
    {
        target = _target.position;
        main.isAttacking = true;
    }

    public void MoveToTarget(Vector3 target)
    {
        if (target != Vector3.zero)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        HoverText.transform.position = Input.mousePosition;
        HoverText.transform.position = new Vector3(HoverText.transform.position.x + 15, HoverText.transform.position.y - 15, HoverText.transform.position.z);
        main.pointer.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (main.currentHealth > 0)
        {
          
            
        }

        if (!main.isAttacking)
        {
            rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
        }

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            target = Vector3.zero;
            main.doingAction = false;
            main.animateWorking = false;
            main.isDeploying = false;
        }

        MoveToTarget(target);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        RealItem realItem = collider.GetComponent<RealItem>();
        if (realItem != null)
        {
            main.inventory.AddItem(realItem.GetItem(), collider);
            //realItem.DestroySelf();//figure out how to call destroy method when collected and not touched     
        }
    }
}
