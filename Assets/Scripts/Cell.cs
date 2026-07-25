using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Cell : MonoBehaviour
{
    public BiomeType biomeType;

    public TileData tileData = new TileData();

    private Transform player;

    public bool isCellLoaded = false;

    [SerializeField] private bool isParasitic;

    public Vector2Int tileLocation = Vector2Int.zero;

    public List<RealWorldObject> objectsList = new List<RealWorldObject>();

    public List<RealItem> itemList = new List<RealItem>();

    public enum BiomeType
    {
        Null,
        Forest,
        Savannah,
        Desert,
        Snowy,
        Rocky,
        Grasslands,
        MagicalForest,
        Swamp,
        Deciduous,
        Parasitic
    }

    /*public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            AskForBiomeDataRPC();
        }
    }

    [Rpc(SendTo.Server)]
    public void AskForBiomeDataRPC()
    {
        SetBiomeRPC((int)biomeType, tileData.tileLocation.x, tileData.tileLocation.y);
    }*/

    [Rpc(SendTo.NotServer)]
    public void SetBiomeRPC(int biomeType, int x, int y)
    {
        tileData.biomeType = (BiomeType)biomeType;
        this.biomeType = (BiomeType)biomeType;
        tileData.tileLocation = new Vector2Int(x, y);
        WorldGeneration.Instance.SetClientTileData(transform.parent.gameObject, this, x, y);
        GetComponent<SpriteRenderer>().sprite = WorldGeneration.Instance.LoadSprite((BiomeType)biomeType);
    }

    private void OnEnable()
    {
        //StartCoroutine(CheckPlayerDistance());
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        isCellLoaded = true;

        if (!isParasitic && biomeType == BiomeType.Parasitic)
        {
            Debug.Log("Bruh");
            BecomeParasitic();
        }
    }

    public void BecomeParasitic()
    {
        if (isParasitic)
        {
            return;
        }

        isParasitic = true;
        biomeType = BiomeType.Parasitic;
        tileData.biomeType = BiomeType.Parasitic;
        WorldGeneration.Instance.SetTileSprite(GetComponent<SpriteRenderer>(), biomeType);

        if (GameManager.Instance.isServer)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (!transform.GetChild(i).CompareTag("Tile"))
                {
                    if (transform.GetChild(i).GetComponent<RealWorldObject>() != null)
                    {
                        transform.GetChild(i).GetComponent<RealWorldObject>().Break(true);
                    }
                    else if (transform.GetChild(i).GetComponent<RealItem>() != null)
                    {
                        transform.GetChild(i).GetComponent<RealItem>().DestroySelf();
                    }
                }
            }
        }
        BecomeParasiticRPC();
        Debug.Log("Parasite biome!!!");

    }

    [Rpc(SendTo.NotServer)]
    private void BecomeParasiticRPC()
    {
        BecomeParasitic();
    }

    public void LoadTile()
    {
        //Debug.Log($"loading tileData: {tileData}");
        foreach(var saveObj in tileData.objDataList)
        {
            var obj = RealWorldObject.SpawnWorldObject(saveObj.pos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList(saveObj.objType) }, true);
            obj.LoadData(saveObj);
        }

        foreach (var saveItem in tileData.itemDataList)
        {
            var item = RealItem.SpawnRealItem(saveItem.pos, saveItem, true);
        }
    }

    public void UnloadTile()
    {
        foreach(var obj in objectsList)
        {
            if (obj != null)
            {
                obj.SaveData();
                obj.GetComponent<NetworkObject>().Despawn();
            }
        }
        objectsList.Clear();

        foreach(var item in itemList)
        {
            if (item != null)
            {
                item.Save();
                item.GetComponent<NetworkObject>().Despawn();
            }
        }

        itemList.Clear();
        tileData = null;
    }
}
