using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Cell : NetworkBehaviour
{
    public BiomeType biomeType;

    public TileData tileData = new TileData();

    private Transform player;

    public bool isCellLoaded = false;

    [SerializeField] private bool isParasitic;

    public NetworkVariable<Vector2Int> tileLocation = new NetworkVariable<Vector2Int>();

    public enum BiomeType
    {
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

    public override void OnNetworkSpawn()
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
    }

    [Rpc(SendTo.NotServer)]
    public void SetBiomeRPC(int biomeType, int x, int y)
    {
        tileData.biomeType = (BiomeType)biomeType;
        this.biomeType = (BiomeType)biomeType;
        tileData.tileLocation = new Vector2Int(x, y);
        WorldGeneration.Instance.SetClientTileData(gameObject, this, x, y);
        GetComponent<SpriteRenderer>().sprite = WorldGeneration.Instance.LoadSprite((BiomeType)biomeType);
    }

    private void OnEnable()
    {
        StartCoroutine(CheckPlayerDistance());
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        isCellLoaded = true;

        if (!isParasitic && biomeType == BiomeType.Parasitic)
        {
            Debug.Log("Bruh");
            BecomeParasitic();
        }
    }

    private IEnumerator CheckPlayerDistance()
    {
        /*if (!GameManager.Instance.isServer)
        {
            yield break;
        }*/
        //Debug.Log("Checking distance from players!");
        yield return new WaitForSeconds(1f);

        int checkDistance = 300;
        if (EtherShardManager.inEther)
        {
            //checkDistance = 30000;
        }

        bool closeToAnyPlayer = false;

        for (int i = 0; i < GameManager.Instance.playerList.Count; i++)
        {
            if (GameManager.Instance.playerList[i] != null && Vector3.Distance(transform.position, GameManager.Instance.playerList[i].transform.position) < checkDistance)
            {
                closeToAnyPlayer = true;
            }
        }

        if (!closeToAnyPlayer)
        {
            UnloadCell();
        }
        else
        {
            StartCoroutine(CheckPlayerDistance());
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

    private void UnloadCell()
    {
        gameObject.SetActive(false);
    }

}
