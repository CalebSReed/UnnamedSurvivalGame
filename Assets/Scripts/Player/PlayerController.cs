using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerController : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        //uiMenu.SetActive(false);
    }


    // Update is called once per frame
    /*void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.C))
        {
            OpenCloseCraftingTab();
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            ParasiteFactionManager.StartParasiteRaid();
            Announcer.SetText("SUMMONING WAVE", Color.red);
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (!WeatherManager.Instance.isRaining)
            {
                StartCoroutine(WeatherManager.Instance.StartRaining());
                Announcer.SetText("RAIN STARTING", Color.blue);
            }
            else
            {
                StartCoroutine(WeatherManager.Instance.StopRaining());
                Announcer.SetText("RAIN ENDING", Color.blue);
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!main.godMode)
            {
                Announcer.SetText("GOD MODE ENABLED");
                main.hpManager.RestoreHealth(9999);
                main.healthBar.SetHealth(main.hpManager.currentHealth);
                main.godMode = true;
            }
            else
            {
                Announcer.SetText("GOD MODE DISABLED");
                main.godMode = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            Announcer.SetText("SUMMONING DEPTH WALKERS", Color.red);
            StartCoroutine(nightEvent.SummonDepthWalkers(true));
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (!freeCrafting)
            {
                Announcer.SetText("FREE CRAFTING ENABLED");
                freeCrafting = true;
                audio.Play("KilnLight1", gameObject);
                main.inventory.RefreshInventory();
            }
            else
            {
                Announcer.SetText("FREE CRAFTING DISABLED");
                freeCrafting = false;
                audio.Stop("KilnLight1");
                audio.Play("KilnOut", gameObject);
                main.inventory.RefreshInventory();
            }

        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            int randVal = UnityEngine.Random.Range(1, 3);
            audio.Play($"Music{randVal}", gameObject, Sound.SoundType.Music, Sound.SoundMode.TwoDimensional);
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            Announcer.SetText("New Parasite Base Spawned", Color.magenta);
            ParasiteFactionManager.Instance.SpawnNewParasiteBase();
        }


        if (main.isAiming && main.doAction == Action.ActionType.Shoot && main.equippedHandItem.ammo > 0)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            txt.text = "LMB: Shoot";
        }
        else if (main.isAiming && main.doAction == Action.ActionType.Throw)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            txt.text = "LMB: Throw";
        }
        else if (main.doAction == Action.ActionType.Till && !main.isHoldingItem)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            txt.text = "RMB: Till";
        }

        if (Input.GetKey(KeyCode.Space))//its nice to have getkey, but its very inconsistent. maybe return to getkeydown?? :/
        {
            Collider[] _objList = Physics.OverlapSphere(transform.position, 25);
            _objList = _objList.OrderBy((d) => (d.transform.position - transform.position).sqrMagnitude).ToArray();//bro lambda expressions are black magic
            foreach (Collider _obj in _objList)
            {
                if (!_obj.isTrigger)
                {
                    continue;
                }
                if (_obj.GetComponentInParent<RealWorldObject>() != null)//add additional check if we're already searching??
                {
                    if (_obj.GetComponentInParent<RealWorldObject>().objectAction == main.doAction || _obj.GetComponentInParent<RealWorldObject>().objectAction == Action.ActionType.Default)//dont return if we dont have same action so we can find next available obj to action on
                    {
                        main.OnObjectSelected(_obj.GetComponentInParent<RealWorldObject>().objectAction, _obj.transform.parent, _obj.GetComponentInParent<RealWorldObject>().obj, _obj.transform.parent.gameObject);
                        return;
                    }
                }
                if (_obj.GetComponentInParent<RealItem>() != null && !_obj.GetComponentInParent<RealItem>().isMagnetic)//add additional check if we're already searching??
                {
                    main.OnItemSelected(_obj.GetComponentInParent<RealItem>());
                    return;
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            /*Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] rayHitList = Physics.RaycastAll(ray);//FIND THE DOOR
            foreach (RaycastHit hit in rayHitList)
            {
                if (hit.collider != null && hit.collider.GetComponent<DoorBehavior>() != null && Vector2.Distance(transform.position, hit.transform.position) < 12)
                {
                    hit.collider.GetComponent<DoorBehavior>().ToggleOpen();
                    Debug.Log("OPEN DOOR!");
                    break;
                }
            }

            if (main.deployMode)//and if not close enough to an existing placed object
            {
                var point = main.deploySprite.bounds.center - new Vector3(0, 0, main.deploySprite.bounds.size.y / 4);
                var list = Physics.OverlapSphere(point, .5f);//maybe make it so this only detects triggers
                Debug.Log(point);
                Debug.Log(list.Length);
                bool playerInList = false;
                bool player2ndBoxInList = false;
                foreach (Collider col in list)
                {
                    if (col.gameObject.CompareTag("Player") && col.isTrigger)
                    {
                        playerInList = true;
                    }
                    if (col.gameObject.CompareTag("Player") && !col.isTrigger)
                    {
                        player2ndBoxInList = true;
                    }
                }
                if (list.Length > 2 || list.Length == 2 && !playerInList && !player2ndBoxInList)//everything has 2 hitboxes, trigger and not trigger
                {
                    return;
                }
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    deployPos = main.pointer.transform.position;
                    deployPos.y = 0;
                }
                else if (!Input.GetKey(KeyCode.LeftControl))
                {
                    deployPos = new Vector3(Mathf.Round(main.pointer.transform.position.x / 6.25f) * 6.25f, 0, Mathf.Round(main.pointer.transform.position.z / 6.25f) * 6.25f);
                }
                main.isDeploying = true;
            }
        }

        if (Input.GetMouseButtonDown(1))//RIGHT CLICK
        {
            if (main.deployMode)
            {
                main.UnDeployItem();
                main.deploySprite.transform.localPosition = Vector3.forward;
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
            else if (main.doAction == Action.ActionType.Till && !main.isHoldingItem)
            {
                main.TillLand();
            }
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            CanMoveAgain = true;
        }
    }*/





    private void FixedUpdate()
    {

    }

    private void OnTriggerEnter(Collider collider)
    {
        //moved to item function
    }

    private void OnMouseDown()
    {
        Debug.Log("Yippee!");
    }
}
