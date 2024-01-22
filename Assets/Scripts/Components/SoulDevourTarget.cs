using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoulDevourTarget : MonoBehaviour
{
    HealthManager hp;
    [SerializeField] private GameObject soulOrbPF;
    private void Start()
    {
        hp = GetComponent<HealthManager>();
        hp.OnDamageTaken += CheckNecklace;
    }

    public void CheckNecklace(object sender, DamageArgs e)
    {
        if (e.senderObject.GetComponent<PlayerMain>() != null)
        {
            PlayerMain pMain = e.senderObject.GetComponent<PlayerMain>();
            if (pMain.chestSlot.currentItem != null && pMain.chestSlot.currentItem.itemSO == ItemObjectArray.Instance.SearchItemList("soulnecklace") && hp.currentHealth <= 0)
            {
                ReleaseHealthOrbs(e);
            }
        }
    }

    private void ReleaseHealthOrbs(DamageArgs e)
    {
        Vector3 pos = transform.position;
        pos.y = 1.5f;
        SoulOrbBehavior orb = Instantiate(soulOrbPF, pos, Quaternion.identity).GetComponent<SoulOrbBehavior>();

        if (hp.maxHealth <= 200)
        {
            orb.healthVal = 5;
        }
        else if (hp.maxHealth <= 500)
        {
            orb.healthVal = 25;
            orb.transform.localScale *= 2;
        }
        else if (hp.maxHealth < 1000)
        {
            orb.healthVal = 50;
            orb.transform.localScale *= 4;
        }
        else
        {
            orb.healthVal = 100;
            orb.transform.localScale *= 8;
        }

        e.senderObject.GetComponent<PlayerMain>().chestSlot.currentItem.uses--;
        e.senderObject.GetComponent<PlayerMain>().chestSlot.UpdateDurability();
    }
}
