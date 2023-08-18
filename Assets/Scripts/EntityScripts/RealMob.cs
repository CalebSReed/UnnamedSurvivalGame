using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class RealMob : MonoBehaviour//short for mobile... moves around
{
    public Mob.MobType mobType { get; private set; }
    public Inventory inventory;
    private List<Item> lootTable;
    HealthManager hpManager;
    private TextMeshProUGUI txt;

    public static RealMob SpawnMob(Vector3 position, Mob _mob)
    {
        Transform transform = Instantiate(Mob_Assets.Instance.pfMobSpawner, position, Quaternion.identity);
        RealMob realMob = transform.GetComponent<RealMob>();
        realMob.SetMob(_mob);
        return realMob;
    }

    public Mob mob;
    public SpriteRenderer sprRenderer;

    private void Awake()
    {
        sprRenderer = GetComponent<SpriteRenderer>();
        txt = GameObject.FindGameObjectWithTag("HoverText").GetComponent<TextMeshProUGUI>();
    }

    public void SetMob(Mob _mob)
    {
        this.mob = _mob;
        mobType = _mob.mobType;
        inventory = new Inventory();
        lootTable = _mob.GetLootTable();
        inventory.AddLootItems(lootTable);
        sprRenderer.sprite = mob.GetSprite();
        gameObject.AddComponent<HealthManager>();
        hpManager = GetComponent<HealthManager>();
        hpManager.SetHealth(_mob.GetMaxHealth());
        hpManager.OnDamageTaken += CheckHealth;
        SetMobComponent();
    }

    public Component SetMobComponent()
    {
        switch (mobType)
        {
            default: return null;
            case Mob.MobType.Bunny: return gameObject.AddComponent<BunnyAI>();
            case Mob.MobType.Wolf: return gameObject.AddComponent<WolfAI>();
            case Mob.MobType.Turkey: return gameObject.AddComponent<BunnyAI>();
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

    private void Die()
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
        txt.text = mobType.ToString();
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
