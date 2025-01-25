using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Unity.Netcode;
public class WorldGeneration : NetworkBehaviour
{
    [SerializeField] public PlayerMain player;
    [SerializeField] public Transform mobContainer;
    public WorldSaveData worldSeed = new WorldSaveData();
    public List<WorldObjectData> naturalObjectSaveList = new List<WorldObjectData>();
    public Dictionary<Vector2Int, List<WorldObjectData>> naturalObjectSaveDict = new Dictionary<Vector2Int, List<WorldObjectData>>();
    public bool forceBiome;
    public Cell.BiomeType forcedBiome;
    //CancellationTokenSource tokenSource;

    public static WorldGeneration Instance { get; private set; }

    public int worldSize;
    public float scale;
    public int offset;
    public int checkSize;
    
    [Header("Gyre Meadows")]
    public float parsnipSpawnChance;
    public float birchSpawnChance;
    public float prairieDogSpawnChance;
    public float horseSpawnChance;
    [Header("Forest")]
    public float mushroomSpawnChance;
    public float torkShroomSpawnChance;
    public float elderberrySpawnChance;
    public float deadLogSpawnChance;
    public float deadStumpSpawnChance;
    public float deerSpawnChance;
    public float bearSpawnChance;
    public float mossyRockSpawnChance;
    [Header("Crystal Crags")]
    public float cragRockSpawnChance;
    public float smallCrystalChance;
    public float largeCrystalChance;
    public float crystalGolemChance;
    public float tinSpawnChance;
    public float limeSpawnChance;
    public float rockSpawnChance;
    public float copperSpawnChance;
    public float sheepSpawnChance;
    [Header("Bleak Bogs")]
    public float cypressSpawnChance;
    public float mudMonsterChance;
    public float pondSpawnChance;
    [Header("Prairie")]
    public float bunnySpawnChance;
    public float carrotSpawnChance;
    public float turkeySpawnChance;
    public float wheatSpawnChance;
    [Header("Frozen Tundras")]
    public float icePondSpawnChance;
    public float wolfSpawnChance;
    public float boulderSpawnChance;
    public float goldBoulderSpawnChance;
    public float snowBankSpawnChance;

    [Header("Sulfur Springs")]
    public float smallVentChance;
    public float tallVentChance;
    public float sulfurPoolChance;
    public float sulfurSoulChance;
    public float sulfurCystChance;
    public float sulfurBoulderChance;
    public float sulfurTreeChance;
    public float lavaChance;
    [Header("Misc")]
    public float snakeSpawnChance;
    public float cactusSpawnChance;
    public float sandSpawnChance;
    public float magicalTreeSpawnChance;
    public float flowerChance;

    //public GameObject[,] biomeGridArray;
    public List<Sprite> TileList;
    public List<TileData> TileDataList = new List<TileData>();//list of raw tile datas
    public List<GameObject> TileObjList;//list of existing tiles
    public List<RealMob> mobList;
    public GameObject groundTileObject;
    public float randomOffsetX { get; set; }
    public float randomOffsetY { get; set; }

    public float temperatureOffsetX { get; set; }
    public float temperatureOffsetY { get; set; }

    public float wetnessOffsetX { get; set; }
    public float wetnessOffsetY { get; set; }

    public Dictionary<Vector2Int, GameObject> existingTileDictionary = new Dictionary<Vector2Int, GameObject>();//Dictionary of already existing tiles
    private GameObject temp = null;

    public GameManager gameManager;
    public Dictionary<Vector2, TileData> tileDataDict = new Dictionary<Vector2, TileData>();//Dictionary of all tiles, existing or not.

    private Vector2Int tileCheck;

    private WaitForSeconds checkCooldown = new WaitForSeconds(1);

    private void Start()
    {
        DayNightCycle.Instance.OnDawn += DoDawnTasks;
        //gameManager.OnLocalPlayerSpawned += OnPlayerSpawned;
        //tokenSource = new CancellationTokenSource();

        if (GameManager.Instance.isServer)
        {
            return;
        }
        StartCoroutine(CheckTilesAroundPlayer());
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        player = gameManager.localPlayer.GetComponent<PlayerMain>();
    }

    public void GenerateWorld()
    {
        randomOffsetX = Random.Range(-offset, offset);
        randomOffsetY = Random.Range(-offset, offset);

        worldSeed.HeightOffSetX = randomOffsetX;
        worldSeed.HeightOffSetY = randomOffsetY;

        temperatureOffsetX = Random.Range(-offset, offset);
        temperatureOffsetY = Random.Range(-offset, offset);

        worldSeed.TemperatureOffSetX = temperatureOffsetX;
        worldSeed.TemperatureOffSetY = temperatureOffsetY;

        wetnessOffsetX = Random.Range(-offset, offset);
        wetnessOffsetY = Random.Range(-offset, offset);

        worldSeed.WetnessOffSetX = wetnessOffsetX;
        worldSeed.WetnessOffSetY = wetnessOffsetY;

        StartCoroutine(CheckTilesAroundPlayer());
    }

    private float GetHeightPerlinNoise(int x, int y)
    {
        float noiseValue = Mathf.PerlinNoise(x * scale + randomOffsetX, y * scale + randomOffsetY);
        return noiseValue;
    }

    private float GetTemperaturePerlinNoise(int x, int y)
    {
        float noiseValue = Mathf.PerlinNoise(x * scale + temperatureOffsetX, y * scale + temperatureOffsetY);
        return noiseValue;
    }

    private float GetWetnessPerlinNoise(int x, int y)
    {
        float noiseValue = Mathf.PerlinNoise(x * scale + wetnessOffsetX, y * scale + wetnessOffsetY);
        return noiseValue;
    }

    private IEnumerator CheckTilesAroundPlayer(bool isWorldLoading = false)
    {
        //int _tileRange = 8;//3 is default EDIT: NOW 5 because you can rotate the camera ig
        //Debug.Log("running script");
        if (player == null)
        {
            yield return null;
            StartCoroutine(CheckTilesAroundPlayer());
            yield break;
        }
        //Debug.Log("player not null");
        if (!player.IsOwnedByServer)//run on only client check to disable tiles
        {
            int coolDown = 0;

            int x = player.cellPosition[0] + worldSize;
            int y = player.cellPosition[1] + worldSize;

            int xi = -checkSize;
            int yi = -checkSize;//this shape generates a weird ass rectangle but TBF most monitors are rectangles so idk lol...

            while (yi < checkSize)//switch to dividing into chunks, we can check 9 chunks around player instead of 25 / 20 tiles
            {
                int tempValX = x;
                int tempValY = y;
                tempValX += xi;
                tempValY += yi;

                tileCheck.x = tempValX;
                tileCheck.y = tempValY;

                GameObject temp = null;
                existingTileDictionary.TryGetValue(tileCheck, out temp);
                //Debug.Log($"{temp}, and tilecheck: {tileCheck}");
                if (existingTileDictionary.TryGetValue(tileCheck, out temp))
                {
                    if (!temp.activeSelf)
                    {
                        //Debug.Log("Reenabling existing tile!");
                        temp.SetActive(true);
                    }
                }

                xi++;

                if (xi > checkSize)
                {
                    xi = -checkSize;
                    yi++;
                }
                coolDown++;

                if (coolDown > 3)
                {
                    yield return null;
                    coolDown = 0;
                }
            }
            StartCoroutine(CheckTilesAroundPlayer());
            yield break;
        }

        for (int i = 0; i < gameManager.playerList.Count; i++)
        {
            if (!gameManager.isLoading || isWorldLoading)
            {
                int coolDown = 0;
                if (gameManager.playerList[i] == null)
                {
                    StartCoroutine(CheckTilesAroundPlayer());
                    yield break;
                }
                int x = gameManager.playerList[i].cellPosition[0] + worldSize;
                int y = gameManager.playerList[i].cellPosition[1] + worldSize;

                int xi = -checkSize;
                int yi = -checkSize;//this shape generates a weird ass rectangle but TBF most monitors are rectangles so idk lol...

                while (yi < checkSize)//switch to dividing into chunks, we can check 9 chunks around player instead of 25 / 20 tiles
                {
                    int tempValX = x;
                    int tempValY = y;
                    tempValX += xi;
                    tempValY += yi;

                    tileCheck.x = tempValX;
                    tileCheck.y = tempValY;

                    if (existingTileDictionary.TryGetValue(tileCheck, out temp))
                    {
                        if (!temp.activeSelf)
                        {
                            //Debug.Log("Reenabling existing tile!");
                            temp.SetActive(true);
                        }
                    }
                    else if (TileExistsOnDisk(tileCheck))//perhaps tiles that are very old delete themselves and remove themselves from existing dictionary? And we reload them from disk if revisited. Keeps long play sessions from dropping frames over time.
                    {
                        //Debug.Log("Generating from disk!");
                        if (IsServer)
                        {
                            GenerateTileFromDisk(tileCheck);
                        }
                    }
                    else//null
                    {
                        //Debug.Log("Generating new tile never seen before!");
                        if (IsServer)
                        {
                            GenerateTile(tempValX, tempValY);
                        }
                    }

                    xi++;

                    if (xi > checkSize)
                    {
                        xi = -checkSize;
                        yi++;
                    }
                    coolDown++;

                    if (coolDown > 3 * gameManager.playerList.Count)//Prepare for heavy lag cuz of my shitty code
                    {
                        yield return null;
                        coolDown = 0;
                    }
                }
            }
        }
        StartCoroutine(CheckTilesAroundPlayer());
    }

    private bool TileExistsOnDisk(Vector2Int key)
    {
        if (tileDataDict.ContainsKey(key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetTileDataDictionary(List<TileData> tileDataList)
    {
        tileDataDict.Clear();
        foreach (TileData tile in tileDataList)
        {
            tileDataDict.Add(tile.tileLocation, tile);
            TileDataList.Add(tile);
        }
    }

    private void GenerateTileFromDisk(Vector2Int key)
    {
        TileData _tileData;
        tileDataDict.TryGetValue(key, out _tileData);

        var _tile = Instantiate(groundTileObject);
        _tile.GetComponent<SpriteRenderer>().sprite = LoadSprite(_tileData.biomeType);
        _tile.transform.position = (Vector2)_tileData.tileLocation;
        _tile.transform.position = new Vector3(_tile.transform.position.x - worldSize, 0, _tile.transform.position.y - worldSize);
        _tile.transform.position *= 25;
        _tile.transform.rotation = Quaternion.LookRotation(Vector3.down);
        _tile.GetComponent<Cell>().tileData.tileLocation = _tileData.tileLocation;
        _tile.GetComponent<Cell>().tileData.biomeType = _tileData.biomeType;
        _tile.GetComponent<Cell>().tileData.dictKey = _tileData.dictKey;
        _tile.GetComponent<Cell>().biomeType = _tileData.biomeType;//forgot to set the ACTUAL cell biometype this whole time lol!
        existingTileDictionary.Add(_tileData.tileLocation, _tile);
        //TileDataList.Add(_tile.GetComponent<Cell>().tileData);   dont do this, we need to add all to list at beginning bcuz ungenned tiles from disk get lost during save!
        TileObjList.Add(_tile);

        int i = 0;
        /*foreach (string obj in _tileData.objTypes)//we should save the object placement random seed but eh im lazy
        {
            var wObj = RealWorldObject.SpawnWorldObject(_tileData.objLocations[i], new WorldObject { woso = WosoArray.Instance.SearchWOSOList(_tileData.objTypes[i]) });
            wObj.transform.parent = _tile.transform;
            wObj.transform.localScale = new Vector3(1, 1, 1);
            _tile.GetComponent<Cell>().tileData.objTypes.Add(wObj.obj.woso.objType);
            _tile.GetComponent<Cell>().tileData.objLocations.Add(wObj.transform.position);
            i++;
        }*/
        i = 0;
        foreach (string item in _tileData.itemTypes)
        {
            var realItem = RealItem.SpawnRealItem(_tileData.itemLocations[i], new Item { itemSO = ItemObjectArray.Instance.SearchItemList(_tileData.itemTypes[i]), amount = 1, equipType = ItemObjectArray.Instance.SearchItemList(_tileData.itemTypes[i]).equipType });
            realItem.transform.parent = _tile.transform;
            _tile.GetComponent<Cell>().tileData.itemTypes.Add(realItem.item.itemSO.itemType);
            _tile.GetComponent<Cell>().tileData.itemLocations.Add(realItem.transform.position);
            i++;
        }

        if (naturalObjectSaveDict.ContainsKey(key))
        {
            foreach (var objSave in naturalObjectSaveDict[key])
            {
                var obj = RealWorldObject.SpawnWorldObject(objSave.pos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList(objSave.objType) }, true);
                obj.LoadData(objSave);
            }
        }
        else
        {
            //Debug.Log("none here!");
        }
    }

    public Sprite LoadSprite(Cell.BiomeType biome)
    {
        switch (biome)
        {
            default: return null;
            case Cell.BiomeType.Savannah: return TileList[0];
            case Cell.BiomeType.Grasslands: return TileList[1];
            case Cell.BiomeType.Rocky: return TileList[2];
            case Cell.BiomeType.Snowy: return TileList[3];
            case Cell.BiomeType.Desert: return TileList[4];
            case Cell.BiomeType.Forest: return TileList[5];
            case Cell.BiomeType.MagicalForest: return TileList[6];
            case Cell.BiomeType.Swamp: return TileList[7];
            case Cell.BiomeType.Deciduous: return TileList[8];
            case Cell.BiomeType.Parasitic: return TileList[9];
        }
    }


    public void GenerateTile(int x, int y)
    {
        //float noiseValue = noiseMap[player.cellPosition[0]+worldSize, player.cellPosition[1]+worldSize];
        float heightValue = GetHeightPerlinNoise(x, y);
        float tempValue = GetTemperaturePerlinNoise(x, y);
        float wetValue = GetWetnessPerlinNoise(x, y);
        GameObject groundTile = Instantiate(groundTileObject);
        groundTile.GetComponent<SpriteRenderer>().sprite = null;

        groundTile.transform.position = new Vector3((x-worldSize) * 25,0, (y-worldSize) * 25);//change Z axis instead of Y cuz of 3D
        groundTile.transform.rotation = Quaternion.LookRotation(Vector3.down);

        Cell cell = groundTile.GetComponent<Cell>();

        cell.biomeType = SetBiome(heightValue, tempValue, wetValue);

        SetTileSprite(groundTile.GetComponent<SpriteRenderer>(), cell.biomeType);
        //biomeGridArray[x,y] = groundTile;
        groundTile.SetActive(true);
        existingTileDictionary.Add(new Vector2Int(x, y), groundTile);
        TileDataList.Add(cell.tileData);
        TileObjList.Add(groundTile);
        //cell.tileData = new TileData();
        cell.tileData.biomeType = cell.biomeType;
        cell.tileData.tileLocation = new Vector2Int(x, y);
        cell.tileLocation.Value = new Vector2Int(x, y);

        if (GameManager.Instance.isServer)
        {
            groundTile.GetComponent<NetworkObject>().Spawn();
        }

        GenerateTileObjects(groundTile, x, y);
    }

    public void SetClientTileData(GameObject groundTile, Cell cell, int x, int y)
    {
        existingTileDictionary.Add(new Vector2Int(x, y), groundTile);
        TileDataList.Add(cell.tileData);
        TileObjList.Add(groundTile);
    }

    private Cell.BiomeType SetBiome(float height, float temp, float wet)
    {
        if (forceBiome)
        {
            return forcedBiome;
        }

        //magical if else list so that even I can't predict world gen

        if (temp > .8f)
        {
            return Cell.BiomeType.Desert;
        }
        else if (temp < .2f)
        {
            return Cell.BiomeType.Snowy;
        }
        else if (wet > .8f)
        {
            return Cell.BiomeType.Swamp;
        }
        else if (wet < .1f)
        {
            return Cell.BiomeType.Desert;
        }
        else if (height > .9f)
        {
            return Cell.BiomeType.Snowy;
        }
        else if (height > .8f)
        {
            return Cell.BiomeType.Rocky;
        }
        else if (height < .1f)
        {
            return Cell.BiomeType.MagicalForest;
        }
        else if (height < .2f)
        {
            return Cell.BiomeType.Swamp;
        }
        else if (temp >= .5f && temp < .8f && wet > .5f && height > .4f)
        {
            return Cell.BiomeType.Forest;
        }
        else if (temp > .35f && temp < .5f && wet < .5f && height > .25f && height < .5f)
        {
            return Cell.BiomeType.Grasslands;
        }
        else if (temp > .2f && temp < .5f && wet > .5f)
        {
            return Cell.BiomeType.Deciduous;
        }
        else if (height > .25f && height < .75f && temp > .5f)
        {
            return Cell.BiomeType.Savannah;
        }
        else if (height > .25f && wet > .5f && temp > .25f)
        {
            return Cell.BiomeType.Swamp;
        }
        else
        {
            Debug.Log($"No conditions met for this biome tile, values: height {height}, wet {wet}, temperature {temp}");//In future make a special biome that only spawns in else statement?
            return Cell.BiomeType.Rocky;
        }

    }

    public void SetTileSprite(SpriteRenderer spr, Cell.BiomeType biomeType)
    {
        if (biomeType == Cell.BiomeType.Forest)
        {
            spr.sprite = TileList[5];
        }
        else if (biomeType == Cell.BiomeType.Grasslands)
        {
            spr.sprite = TileList[1];
        }
        else if (biomeType == Cell.BiomeType.Savannah)
        {
            spr.sprite = TileList[0];
        }
        else if (biomeType == Cell.BiomeType.Rocky)
        {
            spr.sprite = TileList[2];
        }
        else if (biomeType == Cell.BiomeType.Swamp)
        {
            spr.sprite = TileList[7];
        }
        else if (biomeType == Cell.BiomeType.Desert)
        {
            spr.sprite = TileList[4];
        }
        else if (biomeType == Cell.BiomeType.Snowy)
        {
            spr.sprite = TileList[3];
        }
        else if (biomeType == Cell.BiomeType.MagicalForest)
        {
            spr.sprite = TileList[6];
        }
        else if (biomeType == Cell.BiomeType.Deciduous)
        {
            spr.sprite = TileList[8];
        }
        else if (biomeType == Cell.BiomeType.Parasitic)
        {
            spr.sprite = TileList[9];
        }
    }

    private Vector3 CalculateObjectPos(Vector3 objectPos)
    {

        float sizeMultiplied = worldSize * 25;//25 units separate em
        objectPos.x -= sizeMultiplied / 2;
        objectPos.z -= sizeMultiplied / 2;
        //objectPos.x -= sizeSquared / 4;
        //objectPos.y -= sizeSquared / 4;
        return objectPos;
    }

    public void GenerateTileObject(string obj, float chance, string objType, int x, int y, TileData cell, Vector3 objectPos)
    {
        float spawnVal = Random.Range(0f, 100f);//on world load, new chunks will have random objects and item amounts AS WELL AS POSITIONS!
        //if spawnval is more than chance, do not generate. so 100 chance will always spawn. 1 will generate 1% of time. we can go as low as 0.0000001


        if (spawnVal < chance)
        {
            //Debug.Log("spawned!");
            Vector3 newPos = objectPos;
            newPos.x += Random.Range(-10, 11);
            newPos.z += Random.Range(-10, 11);
            newPos.y = 0;

            if (!gameManager.localPlayer.GetComponent<PlayerMain>().IsServer)
            {
                return;
            }

            if (obj == "item")
            {
                var tempObj = RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.SearchItemList(objType), amount = 1});
                //tempObj.transform.parent = existingTileDictionary[new Vector2Int(x, y)].transform;
                tempObj.transform.localScale = new Vector3(1, 1, 1);
                cell.itemTypes.Add(tempObj.item.itemSO.itemType);
                cell.itemLocations.Add(tempObj.transform.position);
            }
            else if (obj == "object")
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList(objType) }, false, false);
                naturalObjectSaveList.Add(tempObj.saveData);
            }
            else if (obj == "mob")
            {
                //return;
                var tempObj = RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList(objType) });
                if (tempObj != null)
                {
                    tempObj.transform.localScale = new Vector3(1, 1, 1);
                }
                //tempObj.transform.parent = existingTileDictionary[new Vector2(x, y)].transform;
                //cell.tileData.objTypes.Add(tempObj.obj.woso.objType);
                //cell.tileData.objLocations.Add(tempObj.transform.position);
            }
        }
    }

    private void GenerateTileObjects(GameObject _tile, int x, int y, float chanceMultiplier = 1f)
    {
        TileData cell = _tile.GetComponent<Cell>().tileData;//HEY! Use this instead of the DICTIONARY to regen stuff!!! I think... Im pretty sure tho

        Vector3 objectPos = _tile.transform.position;

        //Debug.Log(noiseValue);

        //template: GenerateTileObject("", Val, SpawnChance, "", x, y, cell, objectPos);

        if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)
        {
            GenerateTileObject("object", magicalTreeSpawnChance / chanceMultiplier, "MagicalTree", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Desert)
        {
            //GenerateTileObject("object", sandSpawnChance / chanceMultiplier, "Sand Deposit", x, y, cell, objectPos);

            GenerateTileObject("object", sulfurPoolChance / chanceMultiplier, "SulfurPool", x, y, cell, objectPos);

            GenerateTileObject("object", smallVentChance / chanceMultiplier, "Short Sulfur Vent", x, y, cell, objectPos);

            GenerateTileObject("object", tallVentChance / chanceMultiplier, "Tall Sulfur Vent", x, y, cell, objectPos);

            GenerateTileObject("object", sulfurBoulderChance / chanceMultiplier, "Sulfur Boulder", x, y, cell, objectPos);

            GenerateTileObject("object", sulfurTreeChance / chanceMultiplier, "Sulfur-Ridden Tree", x, y, cell, objectPos);

            GenerateTileObject("object", lavaChance / chanceMultiplier, "LavaDeposit", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Rocky)
        {
            GenerateTileObject("object", cragRockSpawnChance / chanceMultiplier, "Arch Formation", x, y, cell, objectPos);

            GenerateTileObject("object", cragRockSpawnChance / chanceMultiplier, "Spike Formation", x, y, cell, objectPos);

            GenerateTileObject("object", cragRockSpawnChance / chanceMultiplier, "Crag Formation", x, y, cell, objectPos);

            GenerateTileObject("object", cragRockSpawnChance / chanceMultiplier, "Crystal Geode", x, y, cell, objectPos);

            GenerateTileObject("object", boulderSpawnChance / chanceMultiplier, "Copper Deposit", x, y, cell, objectPos);

            GenerateTileObject("object", boulderSpawnChance / chanceMultiplier, "Cassiterite Deposit", x, y, cell, objectPos);

            GenerateTileObject("object", smallCrystalChance / chanceMultiplier, "Small Crystal Formation", x, y, cell, objectPos);

            GenerateTileObject("object", largeCrystalChance / chanceMultiplier, "Crystal Pillars", x, y, cell, objectPos);

            GenerateTileObject("object", flowerChance / chanceMultiplier, "crystalflower", x, y, cell, objectPos);

            GenerateTileObject("object", flowerChance / chanceMultiplier, "crystalsapling2", x, y, cell, objectPos);

            GenerateTileObject("object", sandSpawnChance / chanceMultiplier, "sanddeposit", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)
        {
            GenerateTileObject("object", wheatSpawnChance / chanceMultiplier, "Wheat", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)
        {
            GenerateTileObject("object", 100 / chanceMultiplier, "ClayDeposit", x, y, cell, objectPos);

            GenerateTileObject("object", flowerChance / chanceMultiplier, "opalflower", x, y, cell, objectPos);

            GenerateTileObject("mob", mudMonsterChance / chanceMultiplier, "mudTrekker", x, y, cell, objectPos);

            GenerateTileObject("object", flowerChance / chanceMultiplier, "fireweed", x, y, cell, objectPos);

            GenerateTileObject("object", 25 / chanceMultiplier, "Cerulean Fern", x, y, cell, objectPos);

            GenerateTileObject("object", 5 / chanceMultiplier, "buddinglush", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)
        {
            GenerateTileObject("object", 2 / chanceMultiplier, "ClayDeposit", x, y, cell, objectPos);

            GenerateTileObject("object", 100 / chanceMultiplier, "Tree", x, y, cell, objectPos);

            GenerateTileObject("object", deadLogSpawnChance / chanceMultiplier, "Dead Log", x, y, cell, objectPos);

            GenerateTileObject("object", deadStumpSpawnChance / chanceMultiplier, "Dead Stump", x, y, cell, objectPos);

            GenerateTileObject("object", torkShroomSpawnChance / chanceMultiplier, "Tork Shroom", x, y, cell, objectPos);

            GenerateTileObject("object", mossyRockSpawnChance / chanceMultiplier, "Mossy Rock", x, y, cell, objectPos);

            GenerateTileObject("object", elderberrySpawnChance / chanceMultiplier, "Elderberry Bush", x, y, cell, objectPos);

            GenerateTileObject("object", 2 / chanceMultiplier, "Cerulean Fern", x, y, cell, objectPos);

            GenerateTileObject("object", 2 / chanceMultiplier, "Sapling", x, y, cell, objectPos);

            GenerateTileObject("object", 25 / chanceMultiplier, "spookytree", x, y, cell, objectPos);

            GenerateTileObject("object", 5 / chanceMultiplier, "Pond", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)
        {
            GenerateTileObject("object", birchSpawnChance / chanceMultiplier, "Gyre Tree", x, y, cell, objectPos);

            GenerateTileObject("object", 100 / chanceMultiplier, "Cerulean Fern", x, y, cell, objectPos);

            GenerateTileObject("object", 100 / chanceMultiplier, "Sapling", x, y, cell, objectPos);

            GenerateTileObject("object", flowerChance / chanceMultiplier, "gyreflower", x, y, cell, objectPos);

            GenerateTileObject("object", parsnipSpawnChance / chanceMultiplier, "Wild Lumble", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Snowy)
        {
            GenerateTileObject("object", boulderSpawnChance / chanceMultiplier, "Boulder", x, y, cell, objectPos);

            GenerateTileObject("object", goldBoulderSpawnChance / chanceMultiplier, "GoldBoulder", x, y, cell, objectPos);

            GenerateTileObject("object", snowBankSpawnChance / chanceMultiplier, "Snowbank", x, y, cell, objectPos);

            GenerateTileObject("object", icePondSpawnChance / chanceMultiplier, "Ice Pond", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Deciduous)
        {
            GenerateTileObject("object", 5 / chanceMultiplier, "ClayDeposit", x, y, cell, objectPos);

            GenerateTileObject("object", 5 / chanceMultiplier, "deciduoustree", x, y, cell, objectPos);

            GenerateTileObject("object", birchSpawnChance / chanceMultiplier, "birchtree", x, y, cell, objectPos);

            GenerateTileObject("object", boulderSpawnChance / chanceMultiplier, "boulder2", x, y, cell, objectPos);

            GenerateTileObject("object", 3 / chanceMultiplier, "funnyfungus", x, y, cell, objectPos);

            GenerateTileObject("object", 25 / chanceMultiplier, "Sapling", x, y, cell, objectPos);

            GenerateTileObject("object", 8 / chanceMultiplier, "Cerulean Fern", x, y, cell, objectPos);

            GenerateTileObject("object", 10 / chanceMultiplier, "glowinglog", x, y, cell, objectPos);

            GenerateTileObject("object", 5 / chanceMultiplier, "Pond", x, y, cell, objectPos);
        }





        Vector3 carrotPos = objectPos;
        carrotPos.x += Random.Range(-20, 21);
        carrotPos.y += Random.Range(-20, 21);

        Vector3 pondPos = objectPos;
        pondPos.x += Random.Range(-20, 21);
        pondPos.y += Random.Range(-20, 21);

        Vector3 birchPos = objectPos;
        birchPos.x += Random.Range(-20, 21);
        birchPos.y += Random.Range(-20, 21);

        //Debug.Log(noiseValue);

        if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)//--------MAGIC--------
        {
            GenerateTileObject("object", mushroomSpawnChance / chanceMultiplier, "PurpleFungTree", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Desert)//--------SULFURIC SPRINGS--------
        {
            //GenerateTileObject("mob", snakeSpawnChance / chanceMultiplier, "Snake", x, y, cell, objectPos);
            //GenerateTileObject("object", cactusSpawnChance / chanceMultiplier, "Cactus", x, y, cell, objectPos);

            GenerateTileObject("mob", sulfurSoulChance / chanceMultiplier, "sulfuredSoul", x, y, cell, objectPos);

            GenerateTileObject("mob", sulfurCystChance / chanceMultiplier, "SulfurCyst", x, y, cell, objectPos);

            GenerateTileObject("item", rockSpawnChance / chanceMultiplier, "Rock", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Rocky)//--------CRYSTAL CRAGS--------
        {
            GenerateTileObject("item", copperSpawnChance / chanceMultiplier, "RawCopper", x, y, cell, objectPos);

            GenerateTileObject("item", tinSpawnChance / chanceMultiplier, "RawTin", x, y, cell, objectPos);

            GenerateTileObject("item", rockSpawnChance / chanceMultiplier, "Rock", x, y, cell, objectPos);

            GenerateTileObject("item", limeSpawnChance / chanceMultiplier, "limestone", x, y, cell, objectPos);

            GenerateTileObject("mob", sheepSpawnChance / chanceMultiplier, "Sheep", x, y, cell, objectPos);

            GenerateTileObject("mob", crystalGolemChance / chanceMultiplier, "crystalgolem", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)//--------PRAIRIE--------
        {
            GenerateTileObject("object", carrotSpawnChance / chanceMultiplier, "WildCarrot", x, y, cell, objectPos);

            GenerateTileObject("object", bunnySpawnChance / chanceMultiplier, "BunnyHole", x, y, cell, objectPos);

            GenerateTileObject("mob", turkeySpawnChance / chanceMultiplier, "Turkey", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)//--------BLEAK BOGS--------
        {
            GenerateTileObject("object", pondSpawnChance / chanceMultiplier, "Pond", x, y, cell, objectPos);

            GenerateTileObject("object", cypressSpawnChance / chanceMultiplier, "CypressTree", x, y, cell, objectPos);

            GenerateTileObject("mob", 3 / chanceMultiplier, "rolleychloe", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)//--------FOREST--------
        {
            GenerateTileObject("object", mushroomSpawnChance / chanceMultiplier, "BrownShroom", x, y, cell, objectPos);

            GenerateTileObject("mob", bearSpawnChance / chanceMultiplier, "Grizzly Bear", x, y, cell, objectPos);

            GenerateTileObject("item", rockSpawnChance / chanceMultiplier, "Rock", x, y, cell, objectPos);

        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)//--------MEADOWS--------
        {
            GenerateTileObject("item", rockSpawnChance / chanceMultiplier, "Rock", x, y, cell, objectPos);

            //GenerateTileObject("mob", horseSpawnChance / chanceMultiplier, "Horse", x, y, cell, objectPos);

            GenerateTileObject("mob", prairieDogSpawnChance / chanceMultiplier, "Squirmle", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Snowy)//--------SNOWY--------
        {
            GenerateTileObject("mob", wolfSpawnChance / chanceMultiplier, "Wolf", x, y, cell, objectPos);
        }
        else if (_tile.GetComponent<Cell>().biomeType == Cell.BiomeType.Deciduous)//--------DECIDUOUS--------
        {

        }
    }

    private void DoDawnTasks(object sender, System.EventArgs e)
    {
        RegenerateNewTileObjects();
    }

    private void RegenerateNewTileObjects()//glitchy, checks for wrong biome, i believe this happens after saving and loading?? or maybe after game update..
    {
        if (DayNightCycle.Instance.currentSeason == DayNightCycle.Season.Winter)
        {
            return;
        }
        Debug.Log("Regenning world");
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Tile"))
        {
            if (_obj.transform.parent != null)
            {
                continue;
            }

            Vector2 tileLocation = _obj.GetComponent<Cell>().tileData.tileLocation;
            GenerateTileObjects(_obj, (int)tileLocation.x, (int)tileLocation.y, 50);//check every 
        }
    }

    public void GenerateNewNaturalObjDict(List<WorldObjectData> List)
    {
        naturalObjectSaveDict.Clear();
        naturalObjectSaveList = List;

        foreach (var obj in naturalObjectSaveList)
        {
            if (naturalObjectSaveDict.ContainsKey(obj.dictKey))//if tile exists,
            {
                naturalObjectSaveDict[obj.dictKey].Add(obj);//add to list of object saves this tile has
            }
            else//if tile doesnt exist, add new dict key with empty list, and add the first obj save to the list
            {
                var newList = new List<WorldObjectData>();
                newList.Add(obj);
                naturalObjectSaveDict.Add(obj.dictKey, newList);
            }
        }
        Debug.Log("Done doing dict!");
    }

    public void LoadWorld()
    {
        randomOffsetX = worldSeed.HeightOffSetX;
        randomOffsetY = worldSeed.HeightOffSetY;

        temperatureOffsetX = worldSeed.TemperatureOffSetX;
        temperatureOffsetY = worldSeed.TemperatureOffSetY;

        wetnessOffsetX = worldSeed.WetnessOffSetX;
        wetnessOffsetY = worldSeed.WetnessOffSetY;

        StartCoroutine(CheckTilesAroundPlayer(true));
    }

    private void OnDisable()
    {
        //tokenSource.Cancel();
    }
}
