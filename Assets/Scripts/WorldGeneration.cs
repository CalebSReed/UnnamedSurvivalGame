using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class WorldGeneration : MonoBehaviour
{
    [SerializeField] private PlayerMain player;

    public int worldSize;
    public float scale;
    public int offset;
    
    public float bunnySpawnChance;
    public float carrotSpawnChance;
    public float parsnipSpawnChance;
    public float wolfSpawnChance;
    public float snakeSpawnChance;
    public float magicalTreeSpawnChance;
    public float mushroomSpawnChance;
    public float turkeySpawnChance;
    public float pondSpawnChance;
    public float copperSpawnChance;
    public float rockSpawnChance;
    public float tinSpawnChance;
    public float birchSpawnChance;
    public float wheatSpawnChance;
    public float cypressSpawnChance;
    public float sheepSpawnChance;
    public float goldBoulderSpawnChance;
    public float boulderSpawnChance;
    public float cactusSpawnChance;
    public float deadLogSpawnChance;
    public float deadStumpSpawnChance;
    public float torkShroomSpawnChance;
    public float deerSpawnChance;
    public float bearSpawnChance;
    public float mossyRockSpawnChance;
    public float elderberrySpawnChance;
    public float horseSpawnChance;
    public float prairieDogSpawnChance;
    public float snowBankSpawnChance;
    public float icePondSpawnChance;

    //public GameObject[,] biomeGridArray;
    public List<Sprite> TileList;
    public List<GameObject> TileObjList;
    public List<RealMob> mobList;
    public GameObject groundTileObject;
    public float randomOffsetX;
    public float randomOffsetY;

    public Dictionary<Vector2, GameObject> tileDictionary = new Dictionary<Vector2, GameObject>();//i hate that we're using vector2's but creating a huge array destroys memory and idk how to not do that 
    private GameObject temp = null;

    public GameManager gameManager;

    private void Start()
    {
        DayNightCycle.Instance.OnDawn += DoDawnTasks;
    }

    public void GenerateWorld()
    {
        randomOffsetX = Random.Range(-offset, offset);
        randomOffsetY = Random.Range(-offset, offset);
        StartCoroutine(CheckPlayerPosition());
    }

    private float GetPerlinNoise(int x, int y)
    {
        
        float noiseValue = Mathf.PerlinNoise(x * scale + randomOffsetX, y * scale + randomOffsetY);
        return noiseValue;
    }

    public IEnumerator CheckPlayerPosition()
    {
        int _tileRange = 3;//3 is default
        yield return new WaitForSeconds(1f);
        if (!gameManager.isLoading)
        {
            int x = player.cellPosition[0] + worldSize;
            int y = player.cellPosition[1] + worldSize;

            int xi = -_tileRange;
            int yi = -_tileRange + 1;//this shape generates a weird ass rectangle but TBF most monitors are rectangles so idk lol...

            while (yi < _tileRange)
            {
                int tempValX = x;
                int tempValY = y;
                tempValX += xi;
                tempValY += yi;

                if (tileDictionary.TryGetValue(new Vector2(tempValX, tempValY), out temp))
                {
                    if (!tileDictionary[new Vector2(tempValX, tempValY)].gameObject.activeSelf)
                    {
                        tileDictionary[new Vector2(tempValX, tempValY)].gameObject.SetActive(true);
                    }
                }
                else//null
                {
                    GenerateTile(tempValX, tempValY);
                }

                xi++;
                if (xi > _tileRange)
                {
                    xi = -_tileRange;
                    yi++;
                }
            }
        }
        StartCoroutine(CheckPlayerPosition());
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
        }
    }


    private void GenerateTile(int x, int y)
    {
        //float noiseValue = noiseMap[player.cellPosition[0]+worldSize, player.cellPosition[1]+worldSize];
        float noiseValue = GetPerlinNoise(x, y);
        GameObject groundTile = Instantiate(groundTileObject);
        groundTile.GetComponent<SpriteRenderer>().sprite = null;

        Vector3 objectPos = new Vector3(x * 25, y * 25);
        objectPos = CalculateObjectPos(objectPos);//wait we dont need this anymore


        groundTile.transform.position = new Vector3((x-worldSize) * 25, (y-worldSize) * 25, 0);

        Cell cell = groundTile.GetComponent<Cell>();

        if (noiseValue > .8f)
        {
            cell.biomeType = Cell.BiomeType.Desert;
            groundTile.GetComponent<SpriteRenderer>().sprite = TileList[4];
        }
        else if (noiseValue > .7f)
        {
            cell.biomeType = Cell.BiomeType.Rocky;
            groundTile.GetComponent<SpriteRenderer>().sprite = TileList[2];
        }
        else if (noiseValue > .6f)
        {
            cell.biomeType = Cell.BiomeType.Savannah;
            groundTile.GetComponent<SpriteRenderer>().sprite = TileList[0];
        }
        else if (noiseValue > .5f)
        {
            cell.biomeType = Cell.BiomeType.Grasslands;
            groundTile.GetComponent<SpriteRenderer>().sprite = TileList[1];
        }
        else if (noiseValue > .4f)
        {
            cell.biomeType = Cell.BiomeType.Forest;
            groundTile.GetComponent<SpriteRenderer>().sprite = TileList[5];
        }
        else if (noiseValue > .3f)
        {
            cell.biomeType = Cell.BiomeType.Swamp;
            groundTile.GetComponent<SpriteRenderer>().sprite = TileList[7];
        }
        else if (noiseValue > .2f)
        {
            cell.biomeType = Cell.BiomeType.Snowy;
            groundTile.GetComponent<SpriteRenderer>().sprite = TileList[3];
        }
        else if (noiseValue <= .2f)
        {
            cell.biomeType = Cell.BiomeType.MagicalForest;
            groundTile.GetComponent<SpriteRenderer>().sprite = TileList[6];
        }
        else
        {

            Debug.LogError($"SOMETHING HAPPENED. NOISEVALUE IS {noiseValue}");
        }
        //biomeGridArray[x,y] = groundTile;
        tileDictionary.Add(new Vector2(x, y), groundTile);
        TileObjList.Add(groundTile);
        groundTile.SetActive(true);
        //cell.tileData = new TileData();
        cell.tileData.biomeType = cell.biomeType;
        cell.tileData.tileLocation = new Vector2(x, y);
        GenerateTileObjects(groundTile, x, y);
    }

    private Vector3 CalculateObjectPos(Vector3 objectPos)
    {

        float sizeMultiplied = worldSize * 25;//25 units separate em
        objectPos.x -= sizeMultiplied / 2;
        objectPos.y -= sizeMultiplied / 2;
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
            Debug.Log("spawned!");
            Vector3 newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);

            if (obj == "item")
            {
                var tempObj = RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.SearchItemList(objType), amount = 1});
                tempObj.transform.parent = tileDictionary[new Vector2(x, y)].transform;
                cell.itemTypes.Add(tempObj.item.itemSO.itemType);
                cell.itemLocations.Add(tempObj.transform.position);
            }
            else if (obj == "object")
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList(objType) });
                tempObj.transform.parent = tileDictionary[new Vector2(x, y)].transform;
                cell.objTypes.Add(tempObj.obj.woso.objType);
                cell.objLocations.Add(tempObj.transform.position);
            }
            else if (obj == "mob")
            {
                var tempObj = RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList(objType) });
                //tempObj.transform.parent = tileDictionary[new Vector2(x, y)].transform;
                //cell.tileData.objTypes.Add(tempObj.obj.woso.objType);
                //cell.tileData.objLocations.Add(tempObj.transform.position);
            }
        }
    }

    private void GenerateTileObjects(GameObject _tile, int x, int y, float chanceMultiplier = 1f)
    {
        TileData cell = _tile.GetComponent<Cell>().tileData;

        Vector3 objectPos = _tile.transform.position;

        //Debug.Log(noiseValue);

        //template: GenerateTileObject("", Val, SpawnChance, "", x, y, cell, objectPos);

        if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)
        {
            GenerateTileObject("object", magicalTreeSpawnChance / chanceMultiplier, "MagicalTree", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Desert)
        {

        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Rocky)
        {
            GenerateTileObject("object", boulderSpawnChance / chanceMultiplier, "Boulder", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)
        {
            GenerateTileObject("object", wheatSpawnChance / chanceMultiplier, "Wheat", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)
        {
            GenerateTileObject("object", 100 / chanceMultiplier, "ClayDeposit", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)
        {
            GenerateTileObject("object", 100 / chanceMultiplier, "Tree", x, y, cell, objectPos);

            GenerateTileObject("object", deadLogSpawnChance / chanceMultiplier, "Dead Log", x, y, cell, objectPos);

            GenerateTileObject("object", deadStumpSpawnChance / chanceMultiplier, "Dead Stump", x, y, cell, objectPos);

            GenerateTileObject("object", torkShroomSpawnChance / chanceMultiplier, "Tork Shroom", x, y, cell, objectPos);

            GenerateTileObject("object", mossyRockSpawnChance / chanceMultiplier, "Mossy Rock", x, y, cell, objectPos);

            GenerateTileObject("object", elderberrySpawnChance / chanceMultiplier, "Elderberry Bush", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)
        {
            GenerateTileObject("object", birchSpawnChance / chanceMultiplier, "BirchTree", x, y, cell, objectPos);

            GenerateTileObject("object", 100 / chanceMultiplier, "Milkweed", x, y, cell, objectPos);

            GenerateTileObject("object", 100 / chanceMultiplier, "Sapling", x, y, cell, objectPos);

            GenerateTileObject("object", parsnipSpawnChance / chanceMultiplier, "WildParsnip", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Snowy)
        {
            GenerateTileObject("object", boulderSpawnChance / chanceMultiplier, "Boulder", x, y, cell, objectPos);

            GenerateTileObject("object", goldBoulderSpawnChance / chanceMultiplier, "GoldBoulder", x, y, cell, objectPos);

            GenerateTileObject("object", snowBankSpawnChance / chanceMultiplier, "Snowbank", x, y, cell, objectPos);

            GenerateTileObject("object", icePondSpawnChance / chanceMultiplier, "Ice Pond", x, y, cell, objectPos);
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

        if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)//--------MAGIC--------
        {
            GenerateTileObject("object", mushroomSpawnChance / chanceMultiplier, "PurpleFungTree", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Desert)//--------DESERT--------
        {
            GenerateTileObject("mob", snakeSpawnChance / chanceMultiplier, "Snake", x, y, cell, objectPos);
            GenerateTileObject("object", cactusSpawnChance / chanceMultiplier, "Cactus", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Rocky)//--------ROCKY--------
        {
            GenerateTileObject("item", copperSpawnChance / chanceMultiplier, "RawCopper", x, y, cell, objectPos);

            GenerateTileObject("item", tinSpawnChance / chanceMultiplier, "RawTin", x, y, cell, objectPos);

            GenerateTileObject("item", rockSpawnChance / chanceMultiplier, "Rock", x, y, cell, objectPos);

            GenerateTileObject("mob", sheepSpawnChance / chanceMultiplier, "Sheep", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)//--------PRAIRIE--------
        {
            GenerateTileObject("object", carrotSpawnChance / chanceMultiplier, "WildCarrot", x, y, cell, objectPos);

            GenerateTileObject("object", bunnySpawnChance / chanceMultiplier, "BunnyHole", x, y, cell, objectPos);

            GenerateTileObject("mob", turkeySpawnChance / chanceMultiplier, "Turkey", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)//--------SWAMP--------
        {
            GenerateTileObject("object", pondSpawnChance / chanceMultiplier, "Pond", x, y, cell, objectPos);

            GenerateTileObject("object", cypressSpawnChance / chanceMultiplier, "CypressTree", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)//--------FOREST--------
        {
            GenerateTileObject("object", mushroomSpawnChance / chanceMultiplier, "BrownShroom", x, y, cell, objectPos);

            GenerateTileObject("mob", deerSpawnChance / chanceMultiplier, "Deer", x, y, cell, objectPos);

            GenerateTileObject("mob", bearSpawnChance / chanceMultiplier, "Grizzly Bear", x, y, cell, objectPos);

        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)//--------GRASSLANDS--------
        {
            GenerateTileObject("item", rockSpawnChance / chanceMultiplier, "Rock", x, y, cell, objectPos);

            GenerateTileObject("mob", horseSpawnChance / chanceMultiplier, "Horse", x, y, cell, objectPos);

            GenerateTileObject("mob", prairieDogSpawnChance / chanceMultiplier, "Prairie Dog", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Snowy)//--------SNOWY--------
        {
            GenerateTileObject("mob", wolfSpawnChance / chanceMultiplier, "Wolf", x, y, cell, objectPos);
        }
    }

    private void DoDawnTasks(object sender, System.EventArgs e)
    {
        RegenerateNewTileObjects();
    }

    private void RegenerateNewTileObjects()
    {
        Debug.Log("Regenning world");
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Tile"))
        {
            Vector2 tileLocation = _obj.GetComponent<Cell>().tileData.tileLocation;
            GenerateTileObjects(_obj, (int)tileLocation.x, (int)tileLocation.y, 50);//check every 
        }
    }
}
