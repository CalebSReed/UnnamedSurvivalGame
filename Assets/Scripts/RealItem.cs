using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RealItem : MonoBehaviour
{
    private TextMeshPro textMeshPro;
    private PlayerMain player;
    private TextMeshProUGUI txt;
    private GameObject mouse;

    public static RealItem SpawnRealItem(Vector3 position, Item item, bool visible = true, bool used = false, int _ammo = 0, bool _isHot = false, bool pickupCooldown = false, bool isMagnetic = false) //spawns item into the game world.
    {
        Transform transform = Instantiate(ItemObjectArray.Instance.pfItem, position, Quaternion.identity); //sets transform variable to instance that was just created

        RealItem realItem = transform.GetComponent<RealItem>(); //Gets component of this class for the item just spawned so it can use SetItem() function to set the item type to whatever the spawnrealitem function received when called.
        SpriteRenderer spr = realItem.GetComponent<SpriteRenderer>();
        TextMeshPro txt = transform.Find("Text").GetComponent<TextMeshPro>();
        if (visible)
        {
            spr.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            spr.color = new Color(1f, 1f, 1f, 0f);
            txt.color = Color.clear;
        }
        if (!used && item.itemSO.maxUses != 0)
        {
            item.uses = item.itemSO.maxUses;
        }

        if (pickupCooldown)
        {
            realItem.pickUpCooldown = pickupCooldown;
        }

        if (isMagnetic)
        {
            realItem.isMagnetic = true;
        }

        item.ammo = _ammo;
        item.equipType = item.itemSO.equipType;
        realItem.SetItem(item, _isHot);
        return realItem;
    }

    public Item item;
    private SpriteRenderer spriteRenderer;
    public bool isHot = false;
    public bool pickUpCooldown = false;
    public bool isMagnetic = false;
    private float multiplier = 0f;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();
        mouse = GameObject.FindGameObjectWithTag("Mouse");
        txt = mouse.GetComponentInChildren<TextMeshProUGUI>();
        StartCoroutine(CoolDown());
    }

    private IEnumerator PickupCoolDown()
    {
        GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(.5f);
        GetComponent<Collider2D>().enabled = true;

        if (isMagnetic)
        {
            StartCoroutine(Magnetize());
        }
    }

    private IEnumerator Magnetize()
    {
        multiplier += .1f;
        if (player.gameObject != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime * multiplier * 2);
        }
        else
        {
            yield break;
        }
        yield return null;
        StartCoroutine(Magnetize());
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(10f);
        if (isHot)
        {
            Debug.LogError("COLD NOW");
            isHot = false;
        }
    }

    public void SetItem(Item item, bool _isHot)
    {
        if (_isHot)
        {
            isHot = _isHot;
        }
        if (item == null)//this might break some things???? im not sure honestly
        {
            Destroy(gameObject);
        }
        if (pickUpCooldown)
        {
            StartCoroutine(PickupCoolDown());
        }
        spriteRenderer.sprite = item.itemSO.itemSprite;
        if (item.ammo > 0)
        {
            spriteRenderer.sprite = item.itemSO.loadedSprite;
        }
        this.item = item;
        RefreshAmount(item);
        //gameObject.GetComponent<MonoBehaviour>().enabled = false; idk why this shit no work AND lag game
    }

    public Item GetItem()
    {
        return item;
    }

    public void RefreshAmount(Item item)
    {
        if (item.amount > 1)
        {
            textMeshPro.SetText(item.amount.ToString());
        }
        else if (item.uses > 0)
        {
            int newUses = Mathf.RoundToInt((float)item.uses / item.itemSO.maxUses * 100);
            textMeshPro.text = $"{newUses}%";
        }
        else
        {
            textMeshPro.SetText("");
        }
    }

    public void OnMouseDown() //FOR THESE MOUSE EVENTS ENTITIES WITH COLLIDERS AS VISION ARE SET TO IGNORE RAYCAST LAYER SO THEY ARENT CLICKABLE BY MOUSE, CHANGE IF WE WANT TO CHANGE THAT??
    {
        Debug.Log("i was clicked lol");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!isMagnetic)
        {
            player.GetComponent<PlayerMain>().OnItemSelected(this);
        }
    }

    public void DestroySelf()
    {
        if (GetComponentInParent<Cell>() != null)
        {
            int i = 0;
            Cell cell = GetComponentInParent<Cell>();
            foreach (string tileItem in cell.tileData.itemTypes)
            {
                if (tileItem == item.itemSO.itemType)
                {
                    cell.tileData.itemTypes.RemoveAt(i);
                    cell.tileData.itemLocations.RemoveAt(i);
                    break;
                }
                i++;
            }
        }
        Destroy(gameObject);
    }

    private void OnMouseEnter()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        txt.text = $"LMB: Pick up {item.itemSO.itemName}";
    }

    private void OnMouseExit()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        txt.text = "";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger && !isHot)//if triggering player's trigger collider, and is not hot
        {
            if (collision.CompareTag("Player") && isMagnetic)
            {
                player.inventory.AddItem(item, player.transform.position);
                DestroySelf();
                //realItem.DestroySelf();//figure out how to call destroy method when collected and not touched  
            }
        }
    }
}
