using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Unity.Netcode;

[RequireComponent(typeof(MobMovementBase))]
public class RealMob : NetworkBehaviour
{
    public Mob.MobType mobType { get; private set; }
    public Inventory inventory;
    private List<ItemSO> lootTable;
    private List<int> lootAmounts;
    private List<int> lootChances;
    public HealthManager hpManager;
    private TextMeshProUGUI txt;
    private WorldGeneration world;
    public MobSaveData mobSaveData = new MobSaveData();
    public RealWorldObject home;
    public bool etherTarget;
    private DayNightCycle dayCycle;
    private bool goingHome = false;
    public Animator mobAnim;
    public Animator shadowAnim;
    public MobAnimEvent animEvent;
    //public Bounds SpriteBounds { get; set; }
    public Transform sprite;
    public AudioManager audio;
    public Hoverable hoverBehavior;

    public EventHandler homeEvent;

    public Interactable interactable;
    public bool hasSpecialInteraction;

    public PlayerMain player;
    public MobMovementBase mobMovement;

    public PlayerInteractUnityEvent receiveEvent = new PlayerInteractUnityEvent();
    public PlayerInteractUnityEvent interactEvent = new PlayerInteractUnityEvent();

    public SpriteRenderer heldItem;
    private Coroutine callRoutine;
    public Rigidbody rb;
    [SerializeField] SphereCollider hurtBox;
    public bool willStun = true;

    public static RealMob SpawnMob(Vector3 position, Mob _mob)
    {
        var pfToUse = MobPfFinder.Instance.FindMobPf(_mob.mobSO.mobName);
        if (pfToUse == null)
        {
            Debug.LogError("Mob prefab not found!");
            return null;
        }
        Transform transform = Instantiate(pfToUse, position, Quaternion.identity);
        RealMob realMob = transform.GetComponent<RealMob>();
        realMob.SetMob(_mob);
        return realMob;
    }

    public Mob mob;
    public SpriteRenderer sprRenderer;
    public SpriteRenderer shadowCaster;
    public Component objComponent;

    private void Awake()
    {
        hoverBehavior = GetComponent<Hoverable>();
        audio = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        world = GameManager.Instance.world;
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
        dayCycle = GameObject.FindGameObjectWithTag("DayCycle").GetComponent<DayNightCycle>();
        //StartCoroutine(CheckPlayerDistance());

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();
    }

    public void SetMob(Mob _mob)
    {
        this.mob = _mob;
        hoverBehavior.Name = mob.mobSO.mobName;
        world.mobList.Add(this);
        mobSaveData.mobType = mob.mobSO.mobType;
        mobSaveData.mobLocation = transform.position;
        mobMovement = gameObject.GetComponent<MobMovementBase>();

        if (mob.mobSO.mobType == "mercenary")
        {
            inventory = new Inventory(12);
        }
        else
        {
            inventory = new Inventory(64);//if a mob's loot chances can drop more than the inventory limit, some loot will be lost, please fix this.
        }
        lootTable = _mob.mobSO.lootTable;
        lootAmounts = _mob.mobSO.lootAmounts;
        lootChances = _mob.mobSO.lootChances;
        inventory.AddLootItems(lootTable, lootAmounts, lootChances);

        //sprRenderer.sprite = mob.mobSO.mobSprite;
        //shadowCaster.sprite = mob.mobSO.mobSprite;
        //SpriteBounds = sprRenderer.bounds;
        hpManager.SetHealth(_mob.mobSO.maxHealth);
        hpManager.OnDamageTaken += CheckHealth;
        hpManager.OnDamageTaken += OnDamageTaken;
        hpManager.OnHealed += OnHealed;

        /*if (mob.mobSO.mobType == "Squirmle")
        {
            var newObj = Instantiate(MobSkeletonList.Instance.FindSkeleton(mob.mobSO.mobType), transform.GetChild(0));
            sprRenderer.sprite = null;
            mobAnim = newObj.GetComponent<Animator>();
            //GetComponent<ClientNetworkAnimator>().Animator = mobAnim;
        }*/

        if (GameManager.Instance.isServer)
        {
            GetComponent<NetworkObject>().Spawn();
        }

        if (GameManager.Instance.isServer)
        {
            transform.parent = world.mobContainer;
        }

        hurtBox.radius = mob.mobSO.hurtBoxRadius;
        hurtBox.center = new Vector3(0, mob.mobSO.hurtBoxYOffset, 0);

        //SetMobAnimations();
        //SetSpecialMobAI();

        if (GameManager.Instance.isServer)
        {
            //SetBaseMobAI();
            //SetMobRPC(mob.mobSO.mobType);
        }

        shadowAnim = shadowCaster.gameObject.AddComponent<Animator>();

        shadowAnim.runtimeAnimatorController = mob.mobSO.anim;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer && mob == null)
        {
            //take object name, and trim (clone) off the end of it since object name is always same as mobSO.mobName
            string newMobType = gameObject.name;
            int amountToRemove = newMobType.Length - 7;
            newMobType = newMobType.Remove(amountToRemove, 7);

            Mob newMob = new Mob { mobSO = MobObjArray.Instance.SearchMobListByName(newMobType) };
            SetMob(newMob);
            GetComponent<MobAggroAI>().SetFields();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (etherTarget)
        {
            GameManager.Instance.localPlayer.GetComponent<EtherShardManager>().ReturnToReality();
        }
    }

    [Rpc(SendTo.Server)]
    private void AskForMobDataRPC()
    {
        SetMobRPC(mob.mobSO.mobType);
    }

    [Rpc(SendTo.NotServer)]
    private void SetMobRPC(string mobType)
    {
        Mob newMob = new Mob { mobSO = MobObjArray.Instance.SearchMobList(mobType) };
        SetMob(newMob);
    }

    private void OnDamageTaken(object sender, DamageArgs args)
    {
        if (args.dmgType == DamageType.Heavy && !mob.mobSO.heavyWeight)
        {
            mobAnim.Play("Stunned");
        }
        //realMob.willStun = !realMob.willStun;
    }

    private void SetBaseMobAI()
    {
        MobFleeAI _fleeAI;
        

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

            //transform.GetChild(0).GetComponent<SphereCollider>().radius = 4;
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
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("Skirmisher") || mob.mobSO.mobType == "reinforcement")
        {
            gameObject.AddComponent<SkirmisherAttackAI>();
        }
        else if (mob.mobSO == MobObjArray.Instance.SearchMobList("mudTrekker"))
        {
            gameObject.AddComponent<MudtrekkerAttackAI>();
        }
        else if (mob.mobSO.mobType == "crystalgolem")
        {
            gameObject.AddComponent<CrystalGolemAttackAI>();
        }
        else if (mob.mobSO.mobType == "SulfurCyst")
        {
            gameObject.AddComponent<SulfurCystAttackAI>();
        }
        else if (mob.mobSO.mobType == "sulfuredSoul")
        {
            gameObject.AddComponent<SulfurSoulAttackAI>();
        }
        else if (mob.mobSO.mobType == "lyncher")
        {
            gameObject.AddComponent<LyncherAttackAI>();
        }
        else if (mob.mobSO.mobType == "ravager")
        {
            gameObject.AddComponent<RavagerAttackAI>();
        }
        else if (mob.mobSO.mobType == "destroyer")
        {
            gameObject.AddComponent<DestroyerAttackAI>();
        }
        else if (mob.mobSO.mobType == "mercenary")
        {
            gameObject.AddComponent<MercenaryAttackAI>();
        }
        else if (mob.mobSO.mobType == "rolleychloe")
        {
            gameObject.AddComponent<Ridable>();
        }
        else if (mob.mobSO.mobType == "parasiticheartlvl1")
        {
            GetComponent<Rigidbody>().mass = 1000;
            GetComponent<Rigidbody>().drag = 1000;
            GetComponent<Rigidbody>().angularDrag = 1000;
            gameObject.AddComponent<ParasiticHeartAttackAI>();
            //transform.GetChild(0).GetComponent<SphereCollider>().radius = 4;
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
            e.senderObject.GetComponent<EtherShardManager>().AddCharge(mob.mobSO.shardCharge);//should be player bcuz of tag! Should also only be if player dealt final blow!
        }
        else if (hpManager.currentHealth <= 0 && e.damageSenderTag == "fire")
        {
            Die(false);
        }
        else if (hpManager.currentHealth <= 0 && e.damageSenderTag != "Player")
        {
            Die(true, false);
        }
        else
        {
            UpdateHealthForOtherClientsRPC(hpManager.currentHealth);
        }
    }

    private void OnHealed(object sender, EventArgs e)
    {
        UpdateHealthForOtherClientsRPC(hpManager.currentHealth);
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateHealthForOtherClientsRPC(float newHp)
    {
        hpManager.SetCurrentHealth(Mathf.RoundToInt(newHp));
    }

    private IEnumerator Flicker()
    {
        sprRenderer.color = new Color(255, 0, 0);
        yield return new WaitForSeconds(.1f);
        sprRenderer.color = new Color(255, 255, 255);
    }

    public bool HitEnemies(float radius, int mult, bool grabItems = false, bool parriable = true)
    {
        Debug.Log("Start hitting!");
        willStun = true;
        if (mobMovement.target == null)
        {
            return false;
        }
        Vector3 _newPos = transform.position;
        _newPos.y += 5;
        Collider[] _hitEnemies = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider _enemy in _hitEnemies)
        {
            if (!_enemy.isTrigger || _enemy.attachedRigidbody == null)
            {
                continue;
            }

            var enemyObj = _enemy.attachedRigidbody.gameObject;

            if (enemyObj.GetComponent<PlayerMain>() != null)
            {
                if (enemyObj.GetComponent<PlayerMain>().godMode)
                {
                    GetComponent<HealthManager>().TakeDamage(999999, "Player", _enemy.transform.root.gameObject);
                    return true;
                }
            }
            else if (mobMovement.target.GetComponent<RealItem>() != null)
            {
                inventory.AddItem(mobMovement.target.GetComponent<RealItem>().item, transform.position, false);
                mobMovement.target.GetComponent<RealItem>().DestroySelf();
                return true;
            }

            if (enemyObj.GetComponent<HealthManager>() != null && enemyObj.GetComponent<HealthManager>().isParrying && parriable)
            {
                mobAnim.Play("Parried");
                if (!GameManager.Instance.isServer)
                {
                    Debug.Log("Go! Get parried!");
                    ForceAnimationRPC("Parried");
                    AskToKnockBackRPC(enemyObj.GetComponent<PlayerMain>().swingingState.dir.normalized);
                }
                GetKnockedBack(enemyObj.GetComponent<PlayerMain>().swingingState.dir.normalized);
                //hpManager.TakeDamage(player.equippedHandItem.itemSO.damage, player.tag, player.gameObject, DamageType.Light);
                return false;
            }
            if (enemyObj == mobMovement.target)
            {
                enemyObj.GetComponent<HealthManager>().TakeDamage(GetComponent<RealMob>().mob.mobSO.damage * mult, GetComponent<RealMob>().mob.mobSO.mobType, gameObject);
                return true;
            }
            Debug.Log($"{enemyObj}");
        }
        Debug.Log("End hitting!");
        return false;
    }

    private Vector3 knockbackDir;
    private bool beingKnockedBack;
    private float knockBackMult;

    [Rpc(SendTo.Server)]
    private void ForceAnimationRPC(string anim)
    {
        mobAnim.Play(anim);
    }

    public void GetKnockedBack(Vector3 dir)
    {
        if (!GameManager.Instance.isServer)
        {
            Debug.Log("knock!");
            AskToKnockBackRPC(dir);
        }
        knockbackDir = dir;
        knockbackDir.y = 0;
        knockBackMult = 25;
        beingKnockedBack = true;
    }

    [Rpc(SendTo.Server)]
    public void AskToKnockBackRPC(Vector3 dir)
    {
        Debug.Log($"running knockback with dir: {dir}");
        GetKnockedBack(dir);
    }

    private void Update()
    {
        if (beingKnockedBack)
        {
            transform.position += knockbackDir * knockBackMult * Time.deltaTime;
            knockBackMult -= .5f;
            if (knockBackMult <= 0)
            {
                beingKnockedBack = false;
            }
        }
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
        if (etherTarget)
        {
            //GameManager.Instance.localPlayer.GetComponent<EtherShardManager>().ReturnToReality();//change so we keep track of which player enters ether
        }
        if (_dropItems)
        {
            inventory.DropAllItems(transform.position, false, magnetized);            
        }

        mobSaveData.mobType = "Null";
        mobSaveData.mobLocation = Vector3.zero;
        int i = 0;
        foreach (RealMob _mob in world.mobList)
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

        if (GetComponent<NetworkObject>().IsSpawned)
        {
            DespawnNetworkObjectRPC();
        }
    }

    [Rpc(SendTo.Server)]
    private void DespawnNetworkObjectRPC()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (home != null && collision.GetComponentInParent<RealWorldObject>() == home && GetComponent<MobMovementBase>().goHome)
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

    public void OnInteract()
    {
        interactEvent?.Invoke();
    }

    public void ReceiveItem()
    {
        receiveEvent?.Invoke();
    }

    private IEnumerator Speak()
    {
        var _rand = UnityEngine.Random.Range(15f, 25f);
        yield return new WaitForSeconds(_rand);
        _rand = UnityEngine.Random.Range(1,4);
        if (mob.mobSO.talks)
        {
            audio.Play($"{mob.mobSO.mobType}Talk{_rand}", transform.position, gameObject, false, true);
            callRoutine = StartCoroutine(Speak());
        }
    }

    private void OnEnable()
    {
        StartCoroutine(CheckPlayerDistance());
        if (home != null)
        {
            DayNightCycle.Instance.OnNight += GoHome;
        }
        callRoutine = StartCoroutine(Speak());
    }

    private void OnDisable()
    {
        if (home != null)
        {
            DayNightCycle.Instance.OnNight -= GoHome;
        }
        if (callRoutine != null)
        {
            StopCoroutine(callRoutine);
        }
    }

    private IEnumerator CheckPlayerDistance()
    {
        if (!GameManager.Instance.isServer)
        {
            yield break;
        }
        yield return new WaitForSeconds(1);
        bool closeToAnyPlayer = false;
        if (mob.mobSO.isParasite)//parasites are important, keep them loaded from farther values
        {
            foreach (var player in GameManager.Instance.playerList)
            {
                if (Vector3.Distance(player.transform.position, transform.position) < 1200)
                {                    
                    closeToAnyPlayer = true;
                }
            }
        }
        else
        {
            foreach (var player in GameManager.Instance.playerList)
            {
                if (Vector3.Distance(world.player.transform.position, transform.position) < 200)
                {
                    closeToAnyPlayer = true;                  
                }
            }
        }
        if (!closeToAnyPlayer)
        {
            gameObject.SetActive(false);
            yield break;
        }
        StartCoroutine(CheckPlayerDistance());
    }

    public void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(hurtBox.transform.position, mob.mobSO.hurtBoxRadius);
    }

    public void SaveData()
    {
        mobSaveData.mobType = mob.mobSO.mobType;
        mobSaveData.mobLocation = transform.position;
        mobSaveData.currentHealth = hpManager.currentHealth;
        mobSaveData.isEtherTarget = etherTarget;
    }
}
