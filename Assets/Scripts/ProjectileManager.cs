using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public SpriteRenderer sprRenderer;
    public Item item;
    private Vector3 target;
    private bool hasTarget = false;

    public void Awake()
    {
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

    public void SetProjectile(Item _item, Vector3 _target, bool _hasTarget = false)
    {
        if (_hasTarget)
        {
            target = _target;
            hasTarget = _hasTarget;
        }
        sprRenderer.sprite = _item.itemSO.aimingSprite;
        this.item = _item;
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(.3f);
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
        if (collision.collider.CompareTag("Mob"))
        {
            //Physics2D.IgnoreLayerCollision(16, 16);
            collision.collider.GetComponent<HealthManager>().TakeDamage(item.itemSO.damage);
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
        if (collision.CompareTag("Mob"))
        {
            //Physics2D.IgnoreLayerCollision(16, 16); bro u need to do projectile to mob not mob to mob + this carries out worldwide??? wait maybe not so uh yea fix later
            collision.GetComponent<HealthManager>().TakeDamage(item.itemSO.damage);
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
