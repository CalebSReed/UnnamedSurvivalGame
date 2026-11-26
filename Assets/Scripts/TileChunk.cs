using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
//using static UnityEditor.PlayerSettings;

public class TileChunk : NetworkBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Vector2Int chunkPos;
    public ChunkData chunkData = new ChunkData();
    private bool CheckChunks;
    private WaitForSeconds oneSecondtimer = new WaitForSeconds(1f);

    private void Awake()
    {
        CheckChunks = true;
        GenerateTiles();
    }

    private void OnEnable()//need to send rpc to client to update chunk data
    {
        if (CheckChunks)
        {
            //Debug.Log("checking");
            StartCoroutine(CheckPlayerDistance());
        }
    }

    private void OnDisable()
    {
        chunkData.chunkActive = false;
        //Debug.Log("unactive");
        StopAllCoroutines();
    }

    [Rpc(SendTo.NotServer)]
    private void UpdateChunkDataRPC(int[] biomeTypes, Vector3 chunkPos)
    {
        //Debug.Log($"updating chunk data at {chunkPos}");
        gameObject.SetActive(true);
        transform.position = chunkPos;
        for (int j = 0; j < transform.childCount; j++)
        {
            transform.GetChild(j).GetComponent<Cell>().biomeType = (Cell.BiomeType)biomeTypes[j];
            transform.GetChild(j).GetComponent<Cell>().tileData.biomeType = (Cell.BiomeType)biomeTypes[j];
            WorldGeneration.Instance.SetTileSprite(transform.GetChild(j).GetComponent<SpriteRenderer>(), (Cell.BiomeType)biomeTypes[j]);
        }

        var newPos = new Vector2Int(Mathf.RoundToInt((transform.position.x - WorldGeneration.Instance.tileSeparationDistance * 2) / (WorldGeneration.Instance.chunkSize * WorldGeneration.Instance.tileSeparationDistance)) + WorldGeneration.Instance.worldSize, Mathf.RoundToInt((transform.position.z - WorldGeneration.Instance.tileSeparationDistance * 2) / (WorldGeneration.Instance.chunkSize * WorldGeneration.Instance.tileSeparationDistance)) + WorldGeneration.Instance.worldSize);

        int x = 0;
        int y = 0;
        int i = 0;

        while (y < WorldGeneration.Instance.chunkSize)
        {
            while (x < WorldGeneration.Instance.chunkSize)
            {
                Cell cell = transform.GetChild(i).GetComponent<Cell>();
                //newPos.x = pos.x + x;
                //newPos.y = pos.y + y;

                newPos = WorldGeneration.Instance.ChunkToStartTile(new Vector2Int(Mathf.RoundToInt((transform.position.x - WorldGeneration.Instance.tileSeparationDistance * 2) / (WorldGeneration.Instance.chunkSize * WorldGeneration.Instance.tileSeparationDistance)) + WorldGeneration.Instance.worldSize, Mathf.RoundToInt((transform.position.z - WorldGeneration.Instance.tileSeparationDistance * 2) / (WorldGeneration.Instance.chunkSize * WorldGeneration.Instance.tileSeparationDistance)) + WorldGeneration.Instance.worldSize));
                newPos.x += x;
                newPos.y += y;
                cell.tileLocation = newPos;

                if (!WorldGeneration.Instance.tileDataDict.ContainsKey(newPos))
                {
                    WorldGeneration.Instance.tileDataDict.Add(newPos, cell.tileData);
                    WorldGeneration.Instance.activeTileObjDict.Add(newPos, cell);
                }


                //Debug.Log($"New cell at: {newPos}");
                /*cell.tileData = new TileData();
                cell.tileData.tileLocation = newPos;//remember we need to throw away old tileData since we always reusing the same tile OBJs
                cell.tileLocation = newPos;
                //Debug.Log("adding tiles as client");
                //cell.biomeType = WorldGeneration.Instance.SetBiome(WorldGeneration.Instance.GetHeightPerlinNoise(newPos.x, newPos.y), WorldGeneration.Instance.GetTemperaturePerlinNoise(newPos.x, newPos.y), WorldGeneration.Instance.GetWetnessPerlinNoise(newPos.x, newPos.y));
                //cell.tileData.biomeType = cell.biomeType;
                //WorldGeneration.Instance.SetTileSprite(transform.GetChild(i).GetComponent<SpriteRenderer>(), cell.biomeType);

                TileData tileTemp;
                if (!WorldGeneration.Instance.tileDataDict.TryGetValue(chunkData.chunkPos, out tileTemp))
                {
                    WorldGeneration.Instance.TileDataList.Add(cell.tileData);
                    WorldGeneration.Instance.tileDataDict.Add(cell.tileLocation, cell.tileData);
                }*/

                x++;
                i++;
            }
            x = 0;
            y++;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //Debug.Log("network spawn");
        if (IsServer)
        {
            CheckChunks = true;
            //StartCoroutine(CheckPlayerDistance());
        }

        if (!IsServer)//clients should have same dictionary for their own logic
        {
            //transform.parent = WorldGeneration.Instance.chunkPool.transform;

            chunkPos = new Vector2Int(Mathf.RoundToInt((transform.position.x - WorldGeneration.Instance.tileSeparationDistance * 2) / (WorldGeneration.Instance.chunkSize * WorldGeneration.Instance.tileSeparationDistance)) + WorldGeneration.Instance.worldSize, Mathf.RoundToInt((transform.position.z - WorldGeneration.Instance.tileSeparationDistance * 2) / (WorldGeneration.Instance.chunkSize * WorldGeneration.Instance.tileSeparationDistance)) + WorldGeneration.Instance.worldSize);
            chunkData.chunkPos = chunkPos;
            ChunkData temp;
            if (!WorldGeneration.Instance.existingChunkDictionary.TryGetValue(chunkData.chunkPos, out temp))
            {
                WorldGeneration.Instance.existingChunkDictionary.Add(chunkData.chunkPos, chunkData);
                WorldGeneration.Instance.chunkDictionary.Add(chunkData.chunkPos, chunkData);
            }

            Vector2Int newPos = WorldGeneration.Instance.ChunkToStartTile(chunkPos);


            int x = 0;
            int y = 0;
            int i = 0;

            while (y < WorldGeneration.Instance.chunkSize)
            {
                while (x < WorldGeneration.Instance.chunkSize)
                {
                    Cell cell = transform.GetChild(i).GetComponent<Cell>();
                    //newPos.x = pos.x + x;
                    //newPos.y = pos.y + y;

                    newPos = WorldGeneration.Instance.ChunkToStartTile(chunkPos);
                    newPos.x += x;
                    newPos.y += y;

                    //Debug.Log($"New cell at: {newPos}");
                    cell.tileData = new TileData();
                    cell.tileData.tileLocation = newPos;//remember we need to throw away old tileData since we always reusing the same tile OBJs
                    cell.tileLocation = newPos;
                    //WorldGeneration.Instance.activeTileObjDict.Add(cell.tileLocation, cell);
                    //Debug.Log("adding tiles as client");
                    //cell.biomeType = WorldGeneration.Instance.SetBiome(WorldGeneration.Instance.GetHeightPerlinNoise(newPos.x, newPos.y), WorldGeneration.Instance.GetTemperaturePerlinNoise(newPos.x, newPos.y), WorldGeneration.Instance.GetWetnessPerlinNoise(newPos.x, newPos.y));
                    //cell.tileData.biomeType = cell.biomeType;
                    //WorldGeneration.Instance.SetTileSprite(transform.GetChild(i).GetComponent<SpriteRenderer>(), cell.biomeType);

                    TileData tileTemp;
                    if (!WorldGeneration.Instance.tileDataDict.TryGetValue(chunkData.chunkPos, out tileTemp))//ig check if chunk exists???
                    {
                        //WorldGeneration.Instance.TileDataList.Add(cell.tileData);
                        //WorldGeneration.Instance.tileDataDict.Add(cell.tileLocation, cell.tileData);
                    }

                    x++;
                    i++;
                }
                x = 0;
                y++;
            }
            RequestDataForClientsRPC();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestDataForClientsRPC()
    {
        int[] tileBiomes = new int[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            tileBiomes[i] = (int)transform.GetChild(i).GetComponent<Cell>().biomeType;
        }
        UpdateChunkDataRPC(tileBiomes, transform.position);
    }

    public void GenerateTiles()
    {
        int x = 0;
        int y = 0;

        while (y < WorldGeneration.Instance.chunkSize)
        {
            while (x < WorldGeneration.Instance.chunkSize)
            {
                var newPos = transform.position;
                newPos.x += x * WorldGeneration.Instance.tileSeparationDistance;
                newPos.z += y * WorldGeneration.Instance.tileSeparationDistance;
                var tile = Instantiate(tilePrefab, newPos, Quaternion.identity, transform);
                tile.transform.eulerAngles = new Vector3(90, 0, 0);
                x++;
            }
            x = 0;
            y++;
        }

    }

    public void GenerateChunkData(Vector2Int pos)
    {
        chunkData = new ChunkData();
        chunkData.chunkPos = pos;
        chunkData.chunkActive = true;
        chunkPos = chunkData.chunkPos;

        int x = 0;
        int y = 0;
        int i = 0;
        Vector2Int newPos = Vector2Int.zero;

        //Debug.Log($"new chunk at: {pos}");

        while (y < WorldGeneration.Instance.chunkSize)
        {
            while (x < WorldGeneration.Instance.chunkSize)
            {
                Cell cell = transform.GetChild(i).GetComponent<Cell>();
                //newPos.x = pos.x + x;
                //newPos.y = pos.y + y;

                newPos = WorldGeneration.Instance.ChunkToStartTile(chunkPos);
                newPos.x += x;
                newPos.y += y;

                //Debug.Log($"New cell at: {newPos}");
                cell.tileData = new TileData();
                cell.tileData.tileLocation = newPos;//remember we need to throw away old tileData since we always reusing the same tile OBJs
                cell.tileLocation = newPos;
                cell.biomeType = WorldGeneration.Instance.SetBiome(WorldGeneration.Instance.GetHeightPerlinNoise(newPos.x, newPos.y), WorldGeneration.Instance.GetTemperaturePerlinNoise(newPos.x, newPos.y), WorldGeneration.Instance.GetWetnessPerlinNoise(newPos.x, newPos.y));
                cell.tileData.biomeType = cell.biomeType;
                WorldGeneration.Instance.SetTileSprite(transform.GetChild(i).GetComponent<SpriteRenderer>(), cell.biomeType);
                WorldGeneration.Instance.TileDataList.Add(cell.tileData);
                WorldGeneration.Instance.tileDataDict.Add(cell.tileLocation, cell.tileData);
                WorldGeneration.Instance.activeTileObjDict.Add(cell.tileLocation, cell);

                chunkData.tiles.Add(cell.tileData);

                x++;
                i++;
            }
            x = 0;
            y++;
        }
    }

    public void LoadChunkData(ChunkData data, bool reloading)
    {
        chunkData = data;
        chunkData.chunkActive = true;
        chunkPos = chunkData.chunkPos;

        int x = 0;
        int y = 0;
        int i = 0;
        Vector2Int newPos = Vector2Int.zero;
        var pos = data.chunkPos;
        int[] biomeTypes = new int[transform.childCount];

        while (y < WorldGeneration.Instance.chunkSize)
        {
            while (x < WorldGeneration.Instance.chunkSize)
            {
                Cell cell = transform.GetChild(i).GetComponent<Cell>();



                newPos = WorldGeneration.Instance.ChunkToStartTile(chunkPos);
                newPos.x += x;
                newPos.y += y;


                WorldGeneration.Instance.tileDataDict.TryGetValue(newPos, out cell.tileData);

                if (!WorldGeneration.Instance.activeTileObjDict.ContainsKey(newPos))
                {
                    WorldGeneration.Instance.activeTileObjDict.Add(newPos, cell);
                }

                if (cell.tileData == null)
                {
                    Debug.LogError("CRITICAL ERROR: loaded cell tile data does not exist!!");
                }

                cell.biomeType = cell.tileData.biomeType;
                cell.tileLocation = cell.tileData.tileLocation;
                WorldGeneration.Instance.SetTileSprite(transform.GetChild(i).GetComponent<SpriteRenderer>(), cell.biomeType);
                cell.LoadTile(reloading);

                if (IsServer)
                {
                    biomeTypes[i] = (int)cell.biomeType;
                }

                //Debug.Log($"loading old cell: {cell.tileData.tileLocation} with biome: {cell.tileData.biomeType}");

                x++;
                i++;
            }
            x = 0;
            y++;
        }
        //Debug.Log(chunkData.mobDataList.Count);
        var newList = chunkData.mobDataList.ToList();
        chunkData.mobDataList.Clear();
        foreach (var mob in newList)
        {
            //Debug.Log("loading old mob");
            RealMob.SpawnMob(mob.mobLocation, new Mob() { mobSO = MobObjArray.Instance.SearchMobList(mob.mobType) });
        }
        //Debug.Log($"owned: {IsOwnedByServer}, is server? : {IsServer}");
        if (IsServer)
        {
            if (!IsSpawned)
            {
                GetComponent<NetworkObject>().Spawn();
            }
            UpdateChunkDataRPC(biomeTypes, transform.position);
        }
    }

    private IEnumerator CheckPlayerDistance()
    {
        yield return oneSecondtimer;

        int checkDistance = 300;
        if (EtherShardManager.inEther)
        {
            //checkDistance = 30000;
        }

        if (GameManager.Instance.playerList.Count == 0)
        {
            StartCoroutine(CheckPlayerDistance());
            yield break;
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
            if (!IsServer)
            {
                StartCoroutine(CheckPlayerDistance());
                yield break;
            }
            chunkData.chunkActive = false;

            for (int i = 0; i < transform.childCount; i++)
            {
                Cell cell = transform.GetChild(i).GetComponent<Cell>();
                //Debug.Log($"unloading cell, pos: {cell.tileData.tileLocation} biome was {cell.tileData.biomeType}");
                cell.UnloadTile();
                WorldGeneration.Instance.activeTileObjDict.Remove(cell.tileLocation);
            }
            GetComponent<NetworkObject>().Despawn(false);
            transform.parent.GetComponent<ObjectPool>().DespawnObject(gameObject);
        }
        else
        {
            StartCoroutine(CheckPlayerDistance());
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsServer)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Cell cell = transform.GetChild(i).GetComponent<Cell>();
                WorldGeneration.Instance.activeTileObjDict.Remove(cell.tileLocation);
            }
        }
    }
}
