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
        sprRenderer = GetComponent<SpriteRenderer>();
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
    }

    public void SetMob(Mob _mob)
    {
        this.mob = _mob;
        //mobType = _mob.mobSO.mobType;
        inventory = new Inventory(64);
        lootTable = _mob.mobSO.lootTable;
        lootAmounts = _mob.mobSO.lootAmounts;
        lootChances = _mob.mobSO.lootChances;
        inventory.AddLootItems(lootTable, lootAmounts, lootChances);
        sprRenderer.sprite = mob.mobSO.mobSprite;
        gameObject.AddComponent<HealthManager>();
        hpManager = GetComponent<HealthManager>();
        hpManager.SetHealth(_mob.mobSO.maxHealth);
        hpManager.OnDamageTaken += CheckHealth;
        SetMobComponent();
    }

    public void SetMobComponent()
    {
        if (mob.mobSO == MobObjArray.Instance.Wolf)
        {
            var AI = gameObject.AddComponent<WolfAI>();
            AI.visionDistance = 25;
        }
        else if (mob.mobSO == MobObjArray.Instance.Bunny)
        {
            var AI = gameObject.AddComponent<BunnyAI>();
        }
        else if (mob.mobSO == MobObjArray.Instance.Turkey)
        {
            var AI = gameObject.AddComponent<BunnyAI>();
        }
        else if (mob.mobSO == MobObjArray.Instance.Sheep)
        {
            var AI = gameObject.AddComponent<WanderBehavior>();
        }
        else if (mob.mobSO == MobObjArray.Instance.DepthWalker)
        {
            var AI = gameObject.AddComponent<WolfAI>();
            AI.visionDistance = 500;
            gameObject.AddComponent<IsVampire>();
        }
    }

    private void CheckHealth(object sender, System.EventArgs e)
    {
        StartCoroutine(Flicker());
        if (hpManager.currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator Flicker()
    {
        sprRenderer.color = new Color(255, 0, 0);
        yield return new WaitForSeconds(.1f);
        sprRenderer.color = new Color(255, 255, 255);
    }

    public void Die()
    {
        inventory.DropAllItems(transform.position);
        Destroy(gameObject);
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
}
