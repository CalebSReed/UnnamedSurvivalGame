using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public SpriteRenderer sprRenderer;
    public Item item;
    private Vector3 target;
    private bool hasTarget = false;
    private GameObject sender;
    private GameObject pfProjectile;
    private Vector2 velocity;
    private bool ignoreParasites;

    public void Awake()
    {
        pfProjectile = GameObject.FindGameObjectWithTag("Player");
        sprRenderer = GetComponent<SpriteRenderer>();
        //Physics2D.IgnoreLayerCollision(9, 12);
    }

    private void Update()
    {
        if (hasTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, 10 * Time.deltaTime);//switch to mouse pos so our arrows dont get flipped on the frame we flip char b4 we flip the bow
            if (target == transform.position)
            {
                Debug.Log("we've stopped");
                item.uses--;
                DropItem();
            }
        }     
    }

    /*public static GameObject SpawnProjectile(Item item, Vector3 position, GameObject sender, Vector3 target, bool hasTarget = false)
    {
        var _newProjectile = Instantiate(pfProjectile, position, Quaternion.identity);
    }*/

    public void SetProjectile(Item _item, Vector3 _target, GameObject _sender, Vector2 velocity, bool _hasTarget = false, bool ignoreParasites = false)//also add ignore tag maybe? 
    {
        this.velocity = velocity;
        sender = _sender;
        if (_hasTarget)
        {
            target = _target;
            hasTarget = _hasTarget;
        }
        sprRenderer.sprite = _item.itemSO.aimingSprite;
        this.item = _item;
        this.ignoreParasites = ignoreParasites;
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), sender.GetComponent<Collider2D>());
        yield return new WaitForSeconds(1f);//.3f default
        if (item.itemSO.actionType == Action.ActionType.Throw)
        {
            item.uses--;
            DropItem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void DropItem()
    {
        RealItem.SpawnRealItem(transform.position, item, true, true);
        Destroy(gameObject);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == sender)
        {
            return;
        }

        if (collision.collider.CompareTag("Mob") && ignoreParasites && collision.collider.GetComponent<RealMob>().mob.mobSO.isParasite)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
            GetComponent<Rigidbody2D>().velocity = velocity;
            return;
        }

        if (collision.collider.CompareTag("Mob") || (collision.collider.CompareTag("Player")))
        {
            collision.collider.GetComponent<HealthManager>().TakeDamage(item.itemSO.damage, sender.tag, sender);
            if (item.itemSO.actionType == Action.ActionType.Throw)
            {
                item.uses--;
                if (item.uses > 0)
                {
                    DropItem();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);//if arrow, destroy self, maybe in future we drop the arrow with 50% chance?
            }
        }
        else//if world object or sumn
        {
            if (item.itemSO.actionType == Action.ActionType.Throw)
            {
                item.uses--;
                if (item.uses > 0)
                {
                    DropItem();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);//if arrow, destroy self, maybe in future we drop the arrow with 50% chance?
            }
        }
    }
    public void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if (collision.gameObject == sender)
        {
            return;
        }

        if (collision.CompareTag("Mob") && ignoreParasites && collision.GetComponent<RealMob>().mob.mobSO.isParasite)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision);
            GetComponent<Rigidbody2D>().velocity = velocity;
            return;
        }

        if (collision.CompareTag("Mob") || (collision.CompareTag("Player")))
        {
            collision.GetComponent<HealthManager>().TakeDamage(item.itemSO.damage, sender.tag, sender);
            if (item.itemSO.actionType == Action.ActionType.Throw)
            {
                item.uses--;
                if (item.uses > 0)
                {
                    DropItem();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);//if arrow, destroy self, maybe in future we drop the arrow with 50% chance?
            }
        }
    }
}
