using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData
{
    public Vector2Int chunkPos;
    public List<TileData> tiles = new List<TileData>();
    public bool chunkActive;
    public List<MobSaveData> mobDataList = new List<MobSaveData>();
}
