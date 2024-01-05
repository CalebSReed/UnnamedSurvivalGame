using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class RealMob : MonoBehaviour//short for mobile... moves around
{
    public Mob.MobType mobType { get; private set; }
    public Inventory inventory;
    private List<ItemSO> lootTable;
    private List<int> lootAmounts;
    private List<int> lootChances;
    HealthManager hpManager;
    private TextMeshProUGUI txt;
    private WorldGeneration world;
    public MobSaveData mobSaveData = new MobSaveData();
    public RealWorldObject home;
    private DayNightCycle dayCycle;
    private bool goingHome = false;
    public Animator mobAnim;
    //public Bounds SpriteBounds { get; set; }
    public Transform sprite;
    public AudioManager audio;
    private Hoverable hoverBehavior;

    public EventHandler homeEvent;


    public static RealMob SpawnMob(Vector3 position, Mob _mob)
    {
        Transform transform = Instantiate(MobObjArray.Instance.pfMob, position, Quaternion.identity);
        RealMob realMob = transform.GetComponent<RealMob>();
        realMob.SetMob(_mob);
        return realMob;
    }

    public Mob mob;
    public SpriteRenderer sprRenderer;
    public Component objComponent;

    private void Awake()
    {
        hoverBehavior = GetComponent<Hoverable>();
        audio = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        world = GameObject.FindGameObjectWithTag("World").GetComponent<WorldGeneration>();
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
        dayCycle = GameObject.FindGameObjectWithTag("DayCycle").GetComponent<DayNightCycle>();
        //StartCoroutine(CheckPlayerDistance());
    }

    public void SetMob(Mob _mob)
    {
        this.mob = _mob;
        hoverBehavior.Name = mob.mobSO.mobName;
        world.mobList.Add(this);
        mobSaveData.mobTypes.Add(mob.mobSO.mobType);
        mobSaveData.mobLocations.Add(transform.position);

        inventory = new Inventory(64);
        lootTable = _mob.mobSO.lootTable;
        lootAmounts = _mob.mobSO.lootAmounts;
        lootChances = _mob.mobSO.lootChances;
        inventory.AddLootItems(lootTable, lootAmounts, lootChances);

        sprRenderer.sprite = mob.mobSO.mobSprite;
        //SpriteBounds = sprRenderer.bounds;
        hpManager = gameObject.AddComponent<HealthManager>();
        hpManager.SetHealth(_mob.mobSO.maxHealth);
        hpManager.OnDamageTaken += CheckHealth;

        if (mob.mobSO.mobType == "Squirmle")
        {
            var newObj = Instantiate(MobSkeletonList.Instance.FindSkeleton(mob.mobSO.mobType), transform.GetChild(0));
            sprRenderer.sprite = null;
            mobAnim = newObj.GetComponent<Animator>();
        }

        transform.parent = world.mobContainer;

        SetBaseMobAI();
        SetMobAnimations();
        SetSpecialMobAI();
    }

    private void SetBaseMobAI()
    {
        MobFleeAI _fleeAI;
        gameObject.AddComponent<MobMovementBase>();

        if (mob.mobSO.predators.Count > 0)
        {
            _fleeAI = gameObject.AddComponent<MobFleeAI>();
        }

        
        switch (mob.mobSO.aggroType)
        {
            case MobAggroType.AggroType.Passive: //u can flee if ur aggressive and have predators
                //_fleeAI.
                break;
            case MobAggroType.AggroType.Neutral://all enemies should fight back when attacked (if not passive)
                gameObject.AddComponent<MobNeutralAI>();
                if (!mob.mobSO.isSpecialAttacker)
                {
                    gameObject.AddComponent<MobBasicMeleeAI>();
                }
                break;
            case MobAggroType.AggroType.PassiveNeutral:
                gameObject.AddComponent<MobPassiveNeutralAI>();
                if (mob.mobSO.predators.Count == 0)
                {
                    _fleeAI = gameObject.AddComponent<MobFleeAI>();
                }
                break;
            case MobAggroType.AggroType.Aggressive:
                if (!mob.mobSO.isSpecialAttacker)
                {
                    gameObject.AddComponent<MobBasicMeleeAI>();
                }
                gameObject.AddComponent<MobNeutralAI>();
                gameObject.AddComponent<MobAggroAI>();
                break;
        }

        if (mob.mobSO.isScouter)
        {
            gameObject.AddComponent<ParasiteScouterAI>();
        }
    }

    public void SetSpecialMobAI()
    {
        if (mob.mobSO.isVampire)
        {
            gameObject.AddComponent<IsVampire>();
        }

        if (mob.mobSO == MobObjArray.Instance.SearchMobList("Wolf"))
        {
            //var AI = gameObject.AddComponent<WolfAI>();
            //AI.visionDistance = 25;
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Bunny"))
        {
            var AI = gameObject.AddComponent<BunnyAI>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Turkey"))
        {
            //var AI = gameObject.AddComponent<BunnyAI>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Sheep"))
        {
            //var AI = gameObject.AddComponent<WanderBehavior>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("DepthWalker"))
        {
            gameObject.AddComponent<DepthWalkerAttackAI>();

            transform.GetChild(0).GetComponent<SphereCollider>().radius = 4;
            //var AI = gameObject.AddComponent<WolfAI>();
            //AI.visionDistance = 500;
            //gameObject.AddComponent<IsVampire>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Snake"))
        {
            //var AI = gameObject.AddComponent<WolfAI>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Deer"))
        {
            //var AI = gameObject.AddComponent<WanderBehavior>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Grizzly Bear"))
        {
            //var AI = gameObject.AddComponent<WolfAI>();
            //AI.speed = 20;
           // AI.atkDmg = 75;
            //AI.visionDistance = 10; need to fix so aggroes in small range but retains aggro for long range
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Horse"))
        {
            //var AI = gameObject.AddComponent<WanderBehavior>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Prairie Dog"))
        {
            //var AI = gameObject.AddComponent<BunnyAI>();
        }
        else if (MobObjArray.Instance.SearchMobList("Scouter").mobType == mob.mobSO.mobType)
        {
            var AI = gameObject.AddComponent<ScouterAttackAI>();
            GetComponent<Rigidbody>().mass = .25f;
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Soldier"))
        {
            gameObject.AddComponent<SoldierAttackAI>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Skirmisher"))
        {
            gameObject.AddComponent<SkirmisherAttackAI>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Mud Trekker"))
        {
            gameObject.AddComponent<MudtrekkerAttackAI>();
        }
        else if (mob.mobSO.mobType == "Crystal Golem")
        {
            gameObject.AddComponent<CrystalGolemAttackAI>();
        }
        else if (mob.mobSO.mobType == "SulfurCyst")
        {
            gameObject.AddComponent<SulfurCystAttackAI>();
        }
        else if (mob.mobSO.mobType == "Sulfured Soul")
        {
            gameObject.AddComponent<SulfurSoulAttackAI>();
        }
        else if (mob.mobSO.mobType == "lyncher")
        {
            gameObject.AddComponent<LyncherAttackAI>();
        }
    }

    private void SetMobAnimations()
    {
        if (mob.mobSO.anim != null)
        {
            mobAnim.runtimeAnimatorController = mob.mobSO.anim;
        }
    }

    private void CheckHealth(object sender, DamageArgs e)
    {
        StartCoroutine(Flicker());
        int _rand = UnityEngine.Random.Range(1, 4);
        audio.Play($"MobDamaged{_rand}", transform.position, gameObject);
        if (GetComponent<ScoutAI>() != null)
        {
            GetComponent<ScoutAI>().OnHit();
        }
        if (hpManager.currentHealth <= 0 && e.damageSenderTag == "Player")
        {
            Die();
        }
        else if (hpManager.currentHealth <= 0 && e.damageSenderTag == "fire")
        {
            Die(false);
        }
        else if (hpManager.currentHealth <= 0 && e.damageSenderTag != "Player")
        {
            Die(true, false);
        }
    }

    private IEnumerator Flicker()
    {
        sprRenderer.color = new Color(255, 0, 0);
        yield return new WaitForSeconds(.1f);
        sprRenderer.color = new Color(255, 255, 255);
    }

    public void TransitionMob()
    {
        mob.mobSO = mob.mobSO.mobTransition;
    }

    public void SetHome(RealWorldObject _obj)
    {
        home = _obj;
    }

    public void Die(bool _dropItems = true, bool magnetized = true)//if slain by non player, dont magnetize lol
    {
        if (_dropItems)
        {
            inventory.DropAllItems(transform.position, false, magnetized);
        }

        int i = 0;
        foreach(string _mobType in mobSaveData.mobTypes)
        {
            if (_mobType == mob.mobSO.mobType)
            {
                mobSaveData.mobTypes.RemoveAt(i);
                mobSaveData.mobLocations.RemoveAt(i);
                break;
            }
            i++;
        }
        foreach(RealMob _mob in world.mobList)
        {
            if (_mob.mob.mobSO.mobType == world.mobList[i].mob.mobSO.mobType)
            {
                world.mobList.RemoveAt(i);
                break;
            }
            i++;
        }

        DayNightCycle.Instance.OnDawn -= GoHome;
        DayNightCycle.Instance.OnDay -= GoHome;
        DayNightCycle.Instance.OnDusk -= GoHome;
        DayNightCycle.Instance.OnNight -= GoHome;

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.GetComponentInParent<RealWorldObject>() == home && GetComponent<MobMovementBase>().goHome)
        {
            if (mob.mobSO.mobType == "Bunny")
            {
                collision.GetComponentInParent<BunnyHole>().bunnyCount++;//change this later to accomodate all homes
            }
            Die(false);//despawn and drop nothing
        }
        if (collision.GetComponent<RealWorldObject>() == null)
        {
            return;
        }
    }

    public void GoHome(object sender, EventArgs e)
    {
        GetComponent<MobHomeAI>().GoHome();
    }

    public void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        txt.text = mob.mobSO.mobType.ToString();
    }

    public void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        txt.text = "";
    }

    private void OnEnable()
    {
        StartCoroutine(CheckPlayerDistance());
        if (home != null)
        {
            DayNightCycle.Instance.OnNight += GoHome;
        }

    }

    private void OnDisable()
    {
        if (home != null)
        {
            DayNightCycle.Instance.OnNight -= GoHome;
        }
    }

    private IEnumerator CheckPlayerDistance()
    {
        yield return new WaitForSeconds(1);
        if (mob.mobSO.isParasite)//parasites are important, keep them loaded from farther values
        {
            if (Vector3.Distance(world.player.transform.position, transform.position) > 1200)
            {
                gameObject.SetActive(false);
                yield break;
            }
        }
        else
        {
            if (Vector3.Distance(world.player.transform.position, transform.position) > 200)
            {
                gameObject.SetActive(false);
                yield break;
            }
        }
        StartCoroutine(CheckPlayerDistance());
    }
}
