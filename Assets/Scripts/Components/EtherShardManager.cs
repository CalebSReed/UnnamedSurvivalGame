using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Netcode;

public class EtherShardManager : MonoBehaviour
{
    public bool shardReady;
    public float shardChargeProgress;
    public float maxShardCharge;
    static public bool inEther;
    [SerializeField] Slider shardSlider;
    [SerializeField] GameObject fullChargeOutline;
    [SerializeField] public GameObject arenaFloor;
    [SerializeField] public GameObject arenaInstance;
    public event EventHandler OnReturnToReality;

    private void Start()
    {
        inEther = false;
        shardReady = true;
        shardChargeProgress = maxShardCharge;
        shardSlider = SceneReferences.Instance.etherSlider;
        shardSlider.maxValue = maxShardCharge;
        shardSlider.value = shardChargeProgress;
        fullChargeOutline = SceneReferences.Instance.fullEtherChargeOutline;
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
            if (obj.GetComponent<RealMob>() != null)
            {
                obj.GetComponent<RealMob>().etherTarget = true;
            }
            else if (obj.GetComponent<PlayerMain>() != null)
            {
                obj.GetComponent<PlayerMain>().etherTarget = true;
                obj.GetComponent<PlayerMain>().SetPositionRPC(new Vector3(obj.transform.position.x, obj.transform.position.y + 250, obj.transform.position.z));//Ask client to move.
            }
        }
        if (GameManager.Instance.isServer)
        {
            obj.transform.position += new Vector3(0, 250, 0);
        }
        else
        {
            Vector3 newPos = obj.transform.position + new Vector3(0, 250, 0);
            if (obj.GetComponent<RealMob>() != null)
            {
                ClientHelper.Instance.RequestToMoveObjectRPC(newPos, obj.GetComponent<NetworkObject>().NetworkObjectId);
            }
            else
            {
                obj.transform.position = newPos;
            }
        }

        if (!ignoreHeal)
        {
            //obj.GetComponent<HealthManager>().RestoreHealth(99999);
            if (obj.GetComponent<HungerManager>() != null)
            {
                obj.GetComponent<HungerManager>().AddHunger(99999);
            }
        }
    }

    public void EnterEtherMode()
    {
        //WorldGeneration.Instance.checkSize = 25;
        RenderSettings.fogDensity = 0.0035f;
        inEther = true;
        if (GameManager.Instance.isServer)
        {
            var arena = Instantiate(arenaFloor, GetComponent<PlayerMain>().transform.position, Quaternion.identity);
            arenaInstance = arena;
        }
        else
        {
            ClientHelper.Instance.SpawnEtherRPC(GetComponent<PlayerMain>().playerId.Value, transform.position.y);
        }
        var adrenaline = GetComponent<AdrenalineManager>();
        if (adrenaline.inSlowMode || adrenaline.inAdrenalineMode)
        {
            adrenaline.EndAdrenalinePrematurely();
        }
    }

    public void ReturnToReality()
    {
        //WorldGeneration.Instance.checkSize = 8;
        GetComponent<PlayerMain>().transform.position -= new Vector3(0, 250, 0);
        RenderSettings.fogDensity = 0.025f;
        inEther = false;
        if (GameManager.Instance.isServer)
        {
            arenaInstance.GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            ClientHelper.Instance.DespawnEtherRPC(GetComponent<PlayerMain>().playerId.Value);
        }
        OnReturnToReality?.Invoke(PlayerMain.Instance.GetComponent<EtherShardManager>(), EventArgs.Empty);
    }

    public void ResetUI()
    {
        fullChargeOutline.SetActive(false);
    }
}
