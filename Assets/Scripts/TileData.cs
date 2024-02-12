using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData
{
    public Vector2Int tileLocation; //vector2 to use in the dictionary
    public Cell.BiomeType biomeType;
    //public List<string> objTypes = new List<string>();
    //public List<Vector3> objLocations = new List<Vector3>();
    public List<string> itemTypes = new List<string>();
    public List<Vector3> itemLocations = new List<Vector3>();
    public Vector2Int dictKey;
    //public List<WorldObjectData> objSaveDataList = new List<WorldObjectData>();
}
