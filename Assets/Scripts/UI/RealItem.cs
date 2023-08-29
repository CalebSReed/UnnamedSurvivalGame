using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RealItem : MonoBehaviour
{
    private TextMeshPro textMeshPro;

    public static RealItem SpawnRealItem(Vector3 position, Item item, bool visible = true, bool used = false, int _ammo = 0, bool _isHot = false) //spawns item into the game world.
    {
        Transform transform = Instantiate(ItemObjectArray.Instance.pfItem, position, Quaternion.identity); //sets transform variable to instance that was just created

        RealItem realItem = transform.GetComponent<RealItem>(); //Gets component of this class for the item just spawned so it can use SetItem() function to set the item type to whatever the spawnrealitem function received when called.
        SpriteRenderer spr = realItem.GetComponent<SpriteRenderer>();
        if (visible)
        {
            spr.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            spr.color = new Color(1f, 1f, 1f, 0f);
        }
        if (!used && item.itemSO.maxUses != 0)
        {
            item.uses = item.itemSO.maxUses;
        }
        item.ammo = _ammo;
        realItem.SetItem(item, _isHot);
        return realItem;
    }

    public Item item;
    private SpriteRenderer spriteRenderer;
    public bool isHot = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();
        StartCoroutine(CoolDown());
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
        else
        {
            textMeshPro.SetText("");
        }
    }

    public void OnMouseDown() //FOR THESE MOUSE EVENTS ENTITIES WITH COLLIDERS AS VISION ARE SET TO IGNORE RAYCAST LAYER SO THEY ARENT CLICKABLE BY MOUSE, CHANGE IF WE WANT TO CHANGE THAT??
    {
        Debug.Log("i was clicked lol");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerMain>().OnItemSelected(this);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
