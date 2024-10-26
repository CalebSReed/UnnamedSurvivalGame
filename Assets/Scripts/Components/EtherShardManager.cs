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
    [SerializeField] GameObject arenaFloor;
    [SerializeField] GameObject arenaInstance;
    public event EventHandler OnReturnToReality;

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
            //obj.GetComponent<HealthManager>().RestoreHealth(99999);
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
        var ether = GameManager.Instance.player.GetComponent<EtherShardManager>();
        var arena = Instantiate(ether.arenaFloor, GameManager.Instance.player.transform.position, Quaternion.identity);
        ether.arenaInstance = arena;
        //arena.transform.rotation = Quaternion.LookRotation(Vector3.down);
        var adrenaline = GameManager.Instance.player.GetComponent<AdrenalineManager>();
        if (adrenaline.inSlowMode || adrenaline.inAdrenalineMode)
        {
            adrenaline.EndAdrenalinePrematurely();
        }
    }

    public static void ReturnToReality()
    {
        //WorldGeneration.Instance.checkSize = 8;
        GameManager.Instance.player.transform.position -= new Vector3(0, 250, 0);
        RenderSettings.fogDensity = 0.025f;
        inEther = false;
        Destroy(GameManager.Instance.player.GetComponent<EtherShardManager>().arenaInstance);
        OnReturnToReality?.Invoke(this, EventArgs.Empty);
    }

    public void ResetUI()
    {
        fullChargeOutline.SetActive(false);
    }
}
