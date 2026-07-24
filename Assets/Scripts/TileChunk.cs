using System.Collections;
using System.Collections.Generic;
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

        while (y <= WorldGeneration.Instance.chunkSize)
        {
            while (x <= WorldGeneration.Instance.chunkSize)
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

        while (y < WorldGeneration.Instance.chunkSize)
        {
            while (x < WorldGeneration.Instance.chunkSize)
            {
                newPos.x = pos.x + x;
                newPos.y = pos.y + y;
                transform.GetChild(i).GetComponent<Cell>().tileData.tileLocation = newPos;//remember we need to throw away old tileData since we always reusing the same tile OBJs
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
                newPos.x = pos.x + x;
                newPos.y = pos.y + y;
                transform.GetChild(i).GetComponent<Cell>().tileData.tileLocation = newPos;//remember we need to throw away old tileData since we always reusing the same tile OBJs
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
            transform.parent.GetComponent<ObjectPool>().DespawnObject(gameObject);
        }
        else
        {
            StartCoroutine(CheckPlayerDistance());
        }
    }
}
