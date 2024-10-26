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
    //public Transform pfProjectile;
    private Vector3 velocity;
    private bool ignoreParasites;
    private bool hitTarget = false;
    public WOSO objToSpawn;

    public void Awake()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    private void Start()
    {
        PlayerMain.Instance.GetComponent<EtherShardManager>().OnReturnToReality += LeaveEther;
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

    public void LeaveEther(object sender, System.Eventargs e)
    {

    }

    /*public static GameObject SpawnProjectile(Item item, Vector3 position, GameObject sender, Vector3 target, bool hasTarget = false)
    {
        var _newProjectile = Instantiate(pfProjectile, position, Quaternion.identity);
    }*/

    public static Transform SpawnProjectile(GameObject sender, GameObject target, Item item, float forceMult)
    {
        var projectile = Instantiate(GameManager.Instance.pfProjectile, sender.transform.position, Quaternion.identity);
        var velocity = target.transform.position - sender.transform.position;
        velocity.Normalize();
        velocity *= forceMult;
        projectile.GetComponent<ProjectileManager>().SetProjectile(item, target.transform.position, sender, velocity);
        projectile.position = new Vector3(sender.transform.position.x, 1, sender.transform.position.z);
        projectile.GetComponent<Rigidbody>().velocity = velocity;
        projectile.GetChild(0).gameObject.AddComponent<BillBoardBehavior>();
        return projectile;
    }

    public static Transform SpawnProjectile(GameObject sender, GameObject target, Item item, float forceMult, WOSO obj)
    {
        var projectile = Instantiate(GameManager.Instance.pfProjectile, sender.transform.position, Quaternion.identity);
        var velocity = target.transform.position - sender.transform.position;
        velocity.Normalize();
        velocity *= forceMult;
        projectile.GetComponent<ProjectileManager>().SetProjectile(item, target.transform.position, sender, velocity);
        projectile.position = new Vector3(sender.transform.position.x, 1, sender.transform.position.z);
        projectile.GetComponent<Rigidbody>().velocity = velocity;
        projectile.GetChild(0).gameObject.AddComponent<BillBoardBehavior>();
        projectile.GetComponent<ProjectileManager>().objToSpawn = obj;
        return projectile;
    }

    public void SetProjectile(Item _item, Vector3 _target, GameObject _sender, Vector3 velocity, bool _hasTarget = false, bool ignoreParasites = false, float lifetime = 1f)//also add ignore tag maybe? 
    {
        this.velocity = velocity;
        sender = _sender;
        if (_hasTarget)
        {
            target = _target;
            //target.y = 0;
            hasTarget = _hasTarget;
        }
        if (_item.itemSO.aimingSprite != null)
        {
            sprRenderer.sprite = _item.itemSO.aimingSprite;
        }
        else
        {
            sprRenderer.sprite = _item.itemSO.itemSprite;
        }
        var newItem = new Item { amount = 1, ammo = _item.ammo, equipType = _item.equipType, itemSO = _item.itemSO, uses = _item.uses };
        this.item = newItem;
        this.ignoreParasites = ignoreParasites;
        StartCoroutine(Timer(lifetime));
        objToSpawn = _item.itemSO.projectileObjectSpawn;
    }

    private void TryToSpawnObj()
    {
        if (objToSpawn != null)
        {
            var pos = transform.position;
            pos.y = 0;
            RealWorldObject.SpawnWorldObject(pos, new WorldObject { woso = objToSpawn });
        }
    }

    private IEnumerator Timer(float time = 1f)
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), sender.GetComponent<Collider>());
        Physics.IgnoreCollision(GetComponent<Collider>(), sender.GetComponentInParent<Collider>());
        yield return new WaitForSeconds(time);//.3f default actually 1 second is better + nerfs spears in an interesting way
        if (item.itemSO.doActionType == Action.ActionType.Throw)
        {
            item.uses--;
            if (item.uses <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                DropItem();
            }
        }
        else
        {
            TryToSpawnObj();
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
        if (collision.collider.CompareTag("Mob") && ignoreParasites && collision.collider.GetComponent<RealMob>().mob.mobSO.isParasite)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            GetComponent<Rigidbody>().velocity = velocity;
            Debug.Log("Lets go boys");
            return;
        }

        if (collision.gameObject == sender || hitTarget)
        {
            return;
        }


        hitTarget = true;
        Debug.Log("target is now true for collider");

        if (collision.collider.CompareTag("WorldObject") && collision.collider.GetComponent<RealWorldObject>().obj.woso.isPlayerMade && ignoreParasites)//if parasite, do damage to playermade buildings
        {
            collision.collider.GetComponent<HealthManager>().TakeDamage(item.itemSO.damage, "parasite", sender);
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
                TryToSpawnObj();
                Destroy(gameObject);
            }
        }
        else//if not shot by parasite actually this code should never run i think
        {
            Debug.Log("HIT COLLIDER");

            if (!collision.collider.CompareTag("WorldObject") && !collision.collider.CompareTag("Item") && collision.collider.GetComponent<HealthManager>() != null)//if item or object dont do dmg lol if ur not parasite
            {
                if (collision.collider.GetComponent<HealthManager>() != null && collision.collider.GetComponent<HealthManager>().isParrying)
                {
                    Physics.IgnoreCollision(GetComponent<Collider>(), sender.GetComponent<Collider>(), false);
                    ignoreParasites = false;
                    velocity *= -1;
                    GetComponent<Rigidbody>().velocity = velocity;
                    hitTarget = false;
                    sender = collision.gameObject;
                    return;
                }
                else
                {
                    collision.collider.GetComponent<HealthManager>().TakeDamage(item.itemSO.damage, sender.tag, sender);
                }
            }
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
        if (collision.transform.parent == null || collision.GetComponent<CollisionReferences>() != null && collision.GetComponent<CollisionReferences>().rootObj == sender)
        {
            return;
        }

        if (collision.gameObject.layer == 16 && ignoreParasites && collision.GetComponent<CollisionReferences>().mob.mob.mobSO.isParasite)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision);
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.GetComponentInParent<Collider>());
            GetComponent<Rigidbody>().velocity = velocity;
            Debug.Log("Lets go boys");
            return;
        }

        if (hitTarget || collision.transform.parent.CompareTag("Tile") || collision.transform.parent.CompareTag("WorldObject") || collision.transform.parent.CompareTag("Item"))//bruh DO NOT COLLIDER WITH TILES or objects or items!!!
        {
            return;
        }


        hitTarget = true;
        Debug.Log("target true for trigger");


        if (collision.gameObject.layer == 16 || collision.gameObject.layer == 11)//16 is mob, 11 is player
        {
            Debug.Log("HIT TRIGGER");
            if (collision.GetComponent<CollisionReferences>().hp != null && collision.GetComponent<CollisionReferences>().hp.isParrying)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), sender.GetComponent<Collider>(), false);
                ignoreParasites = false;
                velocity *= -1;
                GetComponent<Rigidbody>().velocity = velocity;
                hitTarget = false;
                sender = collision.gameObject;
                return;
            }
            else
            {
                collision.GetComponent<CollisionReferences>().hp.TakeDamage(item.itemSO.damage, sender.tag, sender);
            }
            if (item.itemSO.doActionType == Action.ActionType.Throw)
            {
                item.uses--;
                if (item.uses > 0)
                {
                    DropItem();
                    return;
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                TryToSpawnObj();
                Destroy(gameObject);//if arrow, destroy self, maybe in future we drop the arrow with 50% chance?
                return;
            }
        }
        Debug.LogError($"none were true??? {collision.transform.parent.tag}");
    }
}
