using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class TileChunk : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    public ChunkData chunkData = new ChunkData();

    private void Awake()
    {
        GenerateTiles();
    }

    private void OnEnable()
    {
        StartCoroutine(CheckPlayerDistance());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
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

                newPos = new Vector2Int(Mathf.RoundToInt(transform.GetChild(i).position.x / WorldGeneration.Instance.tileSeparationDistance) + WorldGeneration.Instance.worldSize, Mathf.RoundToInt(transform.GetChild(i).position.z / WorldGeneration.Instance.tileSeparationDistance + WorldGeneration.Instance.worldSize));

                //Debug.Log($"New cell at: {newPos}");
                cell.tileData = new TileData();
                cell.tileData.tileLocation = newPos;//remember we need to throw away old tileData since we always reusing the same tile OBJs
                cell.tileLocation = newPos;
                cell.biomeType = WorldGeneration.Instance.SetBiome(WorldGeneration.Instance.GetHeightPerlinNoise(newPos.x, newPos.y), WorldGeneration.Instance.GetTemperaturePerlinNoise(newPos.x, newPos.y), WorldGeneration.Instance.GetWetnessPerlinNoise(newPos.x, newPos.y));
                cell.tileData.biomeType = cell.biomeType;
                WorldGeneration.Instance.SetTileSprite(transform.GetChild(i).GetComponent<SpriteRenderer>(), cell.biomeType);
                WorldGeneration.Instance.TileDataList.Add(cell.tileData);
                WorldGeneration.Instance.tileDataDict.Add(cell.tileLocation, cell.tileData);

                x++;
                i++;
            }
            x = 0;
            y++;
        }
    }

    public void LoadChunkData(ChunkData data)
    {
        chunkData = data;
        chunkData.chunkActive = true;

        int x = 0;
        int y = 0;
        int i = 0;
        Vector2Int newPos = Vector2Int.zero;
        var pos = data.chunkPos;

        while (y < WorldGeneration.Instance.chunkSize)
        {
            while (x < WorldGeneration.Instance.chunkSize)
            {
                Cell cell = transform.GetChild(i).GetComponent<Cell>();

                newPos = new Vector2Int(Mathf.RoundToInt(transform.GetChild(i).position.x / WorldGeneration.Instance.tileSeparationDistance) + WorldGeneration.Instance.worldSize, Mathf.RoundToInt(transform.GetChild(i).position.z / WorldGeneration.Instance.tileSeparationDistance + WorldGeneration.Instance.worldSize));

                WorldGeneration.Instance.tileDataDict.TryGetValue(newPos, out cell.tileData);

                if (cell.tileData == null)
                {
                    Debug.LogError("CRITICAL ERROR: loaded cell tile data does not exist!!");
                }

                cell.biomeType = cell.tileData.biomeType;
                cell.tileLocation = cell.tileData.tileLocation;
                WorldGeneration.Instance.SetTileSprite(transform.GetChild(i).GetComponent<SpriteRenderer>(), cell.biomeType);
                //Debug.Log($"loading old cell: {cell.tileData.tileLocation} with biome: {cell.tileData.biomeType}");

                x++;
                i++;
            }
            x = 0;
            y++;
        }
    }

    private IEnumerator CheckPlayerDistance()
    {
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
            chunkData.chunkActive = false;

            for (int i = 0; i < transform.childCount; i++)
            {
                Cell cell = transform.GetChild(i).GetComponent<Cell>();
                //Debug.Log($"unloading cell, pos: {cell.tileData.tileLocation} biome was {cell.tileData.biomeType}");
                cell.Unload();
            }

            transform.parent.GetComponent<ObjectPool>().DespawnObject(gameObject);
        }
        else
        {
            StartCoroutine(CheckPlayerDistance());
        }
    }
}
