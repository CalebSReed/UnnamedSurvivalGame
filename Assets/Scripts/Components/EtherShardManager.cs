using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EtherShardManager : MonoBehaviour
{
    public bool shardReady;
    public float shardChargeProgress;
    public float maxShardCharge;
    static public bool inEther;
    [SerializeField] Slider shardSlider;
    [SerializeField] GameObject fullChargeOutline;

    private void Start()
    {
        inEther = false;
        shardReady = true;
        shardChargeProgress = maxShardCharge;
        shardSlider.maxValue = maxShardCharge;
        shardSlider.value = shardChargeProgress;
    }

    public void AddCharge(float val)
    {
        shardChargeProgress += val;
        shardSlider.value = shardChargeProgress;
        if (shardChargeProgress >= maxShardCharge)
        {
            FullyCharged();
        }
    }

    public void FullyCharged()
    {
        shardReady = true;
        fullChargeOutline.SetActive(true);
    }

    public void BreakShard()
    {
        fullChargeOutline.SetActive(false);
        shardReady = false;
        shardChargeProgress = 0;
        shardSlider.value = 0;
    }

    public static void SendToEther(GameObject obj, bool isEnemy = false, bool ignoreHeal = false)
    {
        if (isEnemy)
        {
            obj.GetComponent<RealMob>().etherTarget = true;
        }
        obj.transform.position += new Vector3(0, 250, 0);
        if (!ignoreHeal)
        {
            obj.GetComponent<HealthManager>().RestoreHealth(99999);
            if (obj.GetComponent<HungerManager>() != null)
            {
                obj.GetComponent<HungerManager>().AddHunger(99999);
            }
        }
    }

    public static void EnterEtherMode()
    {
        //WorldGeneration.Instance.checkSize = 25;
        RenderSettings.fogDensity = 0.0035f;
        inEther = true;
        GameManager.Instance.world.player.audio.Play("EnterEther", Vector3.zero, null, true, false, true);
    }

    public static void ReturnToReality()
    {
        //WorldGeneration.Instance.checkSize = 8;
        GameManager.Instance.player.transform.position -= new Vector3(0, 250, 0);
        RenderSettings.fogDensity = 0.025f;
        inEther = false;
    }

    public void ResetUI()
    {
        fullChargeOutline.SetActive(false);
    }
}
