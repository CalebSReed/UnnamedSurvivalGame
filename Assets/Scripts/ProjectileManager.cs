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
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
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

    public void SetProjectile(Item _item, Vector3 _target, GameObject _sender, Vector3 velocity, bool _hasTarget = false, bool ignoreParasites = false)//also add ignore tag maybe? 
    {
        this.velocity = velocity;
        sender = _sender;
        if (_hasTarget)
        {
            target = _target;
            target.y = 0;
            hasTarget = _hasTarget;
        }
        sprRenderer.sprite = _item.itemSO.aimingSprite;
        this.item = _item;
        this.ignoreParasites = ignoreParasites;
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), sender.GetComponent<Collider>());
        Physics.IgnoreCollision(GetComponent<Collider>(), sender.GetComponentInParent<Collider>());
        yield return new WaitForSeconds(1f);//.3f default actually 1 second is better + nerfs spears in an interesting way
        if (item.itemSO.doActionType == Action.ActionType.Throw)
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

    public void OnCollisionEnter(Collision collision)//non trigger
    {
        if (collision.gameObject == sender)
        {
            return;
        }

        if (collision.collider.CompareTag("Mob") || collision.collider.CompareTag("Player"))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            GetComponent<Rigidbody>().velocity = velocity;
            Debug.LogError("HIT SOMETHIN I SHOULDNT HAVE!");
            return;
        }

        if (collision.collider.CompareTag("WorldObject") && collision.collider.GetComponent<RealWorldObject>().obj.woso.isPlayerMade && sender.GetComponent<RealMob>() != null && sender.GetComponent<RealMob>().mob.mobSO.isParasite)//if parasite, do damage to playermade buildings
        {
            collision.collider.GetComponent<HealthManager>().TakeDamage(item.itemSO.damage, sender.tag, sender);
            if (item.itemSO.doActionType == Action.ActionType.Throw)
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
                Destroy(gameObject);
            }
        }
        else//if world object or sumn
        {
            if (item.itemSO.doActionType == Action.ActionType.Throw)
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
    public void OnTriggerEnter(UnityEngine.Collider collision)//is trigger, so check the parent Gameobject
    {
        if (collision.transform.parent.gameObject == sender)
        {
            return;
        }

        if (collision.transform.parent.CompareTag("Mob") && ignoreParasites && collision.transform.parent.GetComponent<RealMob>().mob.mobSO.isParasite)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision);
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.GetComponentInParent<Collider>());
            GetComponent<Rigidbody>().velocity = velocity;
            Debug.LogError("Lets go boys");
            return;
        }

        if (collision.transform.parent.CompareTag("Mob") || (collision.transform.parent.CompareTag("Player")))
        {
            collision.GetComponentInParent<HealthManager>().TakeDamage(item.itemSO.damage, sender.tag, sender);
            if (item.itemSO.doActionType == Action.ActionType.Throw)
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
