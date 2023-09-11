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
    
    public int bunnySpawnChance;
    public int carrotSpawnChance;
    public int parsnipSpawnChance;
    public int wolfSpawnChance;
    public int magicalTreeSpawnChance;
    public int mushroomSpawnChance;
    public int turkeySpawnChance;
    public int pondSpawnChance;
    public int copperSpawnChance;
    public int rockSpawnChance;
    public int tinSpawnChance;
    public int birchSpawnChance;
    public int wheatSpawnChance;
    public int cypressSpawnChance;
    public int sheepSpawnChance;
    public int goldBoulderSpawnChance;
    public int boulderSpawnChance;

    //public GameObject[,] biomeGridArray;
    public List<Sprite> TileList;
    public List<GameObject> TileObjList;
    public List<RealMob> mobList;
    public GameObject groundTileObject;
    public float randomOffset;

    public Dictionary<Vector2, GameObject> tileDictionary = new Dictionary<Vector2, GameObject>();//i hate that we're using vector2's but creating a huge array destroys memory and idk how to not do that 
    private GameObject temp = null;

    public GameManager gameManager;

    public void GenerateWorld()
    {
        randomOffset = Random.Range(-offset, offset);
        StartCoroutine(CheckPlayerPosition());
    }

    private float GetPerlinNoise(int x, int y)
    {
        
        float noiseValue = Mathf.PerlinNoise(x * scale + randomOffset, y * scale + randomOffset);
        return noiseValue;
    }

    public IEnumerator CheckPlayerPosition()//hey BUCKO my games been running fine since THIS! Something in the worldgen script is collecting HELLA GARBAGE! (I THINK) Clean up this shit bruh
    {
        yield return new WaitForSeconds(1f);
        if (!gameManager.isLoading)
        {
            int x = player.cellPosition[0] + worldSize;
            int y = player.cellPosition[1] + worldSize;

            int xi = -3;
            int yi = -3;

            while (yi < 3)
            {
                if (xi > 3)
                {
                    xi = -3;
                    yi++;
                }
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

                /*if (biomeGridArray.GetValue(tempValX, tempValY) == null)//if is length BUT cell is null/deactivated then reactivate BAM! old check: biomeGridArray.GetLength(0) < player.cellPosition[0] || biomeGridArray.GetLength(1) < player.cellPosition[1]
                {
                    //Debug.LogError($"GENERATING NEW TILE AT {tempValX}, {tempValY}");
                    GenerateTile(tempValX, tempValY);
                }
                else if (!biomeGridArray[tempValX, tempValY].gameObject.activeSelf)
                {
                    //Debug.LogError("ACTIVATE");
                    biomeGridArray[tempValX, tempValY].gameObject.SetActive(true);
                }
                xi++;*/
            }
            //Debug.Log($"CHECKING FOR PLAYER!!! THEY ARE AT {player.cellPosition[0]}, {player.cellPosition[1]}... x length is {biomeGridArray.GetLength(0)} and y length is {biomeGridArray.GetLength(1)}");

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

    private void GenerateTileObject(string obj, int value, int chance, string objType, int x, int y, TileData cell, Vector3 objectPos)
    {
        if (value == chance - 1)
        {
            Vector3 newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);

            if (obj == "item")
            {
                var tempObj = RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.SearchItemList(objType)});
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

    private void GenerateTileObjects(GameObject _tile, int x, int y)
    {
        TileData cell = _tile.GetComponent<Cell>().tileData;

        Vector3 objectPos = _tile.transform.position;

        int magicalTreeVal = Random.Range(0, magicalTreeSpawnChance);//on world load, new chunks will have random objects and item amounts
        int birchVal = Random.Range(0, birchSpawnChance);
        int wheatVal = Random.Range(0, wheatSpawnChance);
        int goldBoulderVal = Random.Range(0, goldBoulderSpawnChance);
        int boulderVal = Random.Range(0, boulderSpawnChance);

        //Debug.Log(noiseValue);

        //template: GenerateTileObject("", Val, SpawnChance, "", x, y, cell, objectPos);

        if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)
        {
            GenerateTileObject("object", magicalTreeVal, magicalTreeSpawnChance, "MagicalTree", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Desert)
        {

        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Rocky)
        {
            GenerateTileObject("object", boulderVal, boulderSpawnChance, "Boulder", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)
        {
            GenerateTileObject("object", wheatVal, wheatSpawnChance, "Wheat", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)
        {
            GenerateTileObject("object", 0, 1, "ClayDeposit", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)
        {
            GenerateTileObject("object", 0, 1, "Tree", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)
        {
            GenerateTileObject("object", birchVal, birchSpawnChance, "BirchTree", x, y, cell, objectPos);

            GenerateTileObject("object", 0, 1, "Milkweed", x, y, cell, objectPos);

            GenerateTileObject("object", 0, 1, "Sapling", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Snowy)
        {
            GenerateTileObject("object", boulderVal, boulderSpawnChance, "Boulder", x, y, cell, objectPos);

            GenerateTileObject("object", goldBoulderVal, goldBoulderSpawnChance, "GoldBoulder", x, y, cell, objectPos);
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

        int bunnyVal = Random.Range(0, bunnySpawnChance);
        int carrotVal = Random.Range(0, carrotSpawnChance);
        int wolfVal = Random.Range(0, wolfSpawnChance);
        int turkeyVal = Random.Range(0, turkeySpawnChance);
        int parsnipVal = Random.Range(0, parsnipSpawnChance);
        int mushroomVal = Random.Range(0, mushroomSpawnChance);
        int pondVal = Random.Range(0, pondSpawnChance);
        int copperVal = Random.Range(0, copperSpawnChance);
        int rockVal = Random.Range(0, rockSpawnChance);
        int cypressVal = Random.Range(0, cypressSpawnChance);
        int sheepVal = Random.Range(0, sheepSpawnChance);

        int tinVal = Random.Range(0, tinSpawnChance);

        //Debug.Log(noiseValue);

        if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)//--------MAGIC--------
        {
            GenerateTileObject("object", mushroomVal, mushroomSpawnChance, "PurpleFungTree", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Desert)//--------DESERT--------
        {

        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Rocky)//--------ROCKY--------
        {
            GenerateTileObject("item", copperVal, copperSpawnChance, "RawCopper", x, y, cell, objectPos);

            GenerateTileObject("item", tinVal, tinSpawnChance, "RawTin", x, y, cell, objectPos);

            GenerateTileObject("item", rockVal, rockSpawnChance, "Rock", x, y, cell, objectPos);

            GenerateTileObject("mob", sheepVal, sheepSpawnChance, "Sheep", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)//--------PRAIRIE--------
        {
            GenerateTileObject("object", carrotVal, carrotSpawnChance, "WildCarrot", x, y, cell, objectPos);

            GenerateTileObject("object", bunnyVal, bunnySpawnChance, "BunnyHole", x, y, cell, objectPos);

            GenerateTileObject("mob", bunnyVal, bunnySpawnChance, "Bunny", x, y, cell, objectPos);

            GenerateTileObject("mob", turkeyVal, turkeySpawnChance, "Turkey", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)//--------SWAMP--------
        {
            GenerateTileObject("object", pondVal, pondSpawnChance, "Pond", x, y, cell, objectPos);

            GenerateTileObject("object", cypressVal, cypressSpawnChance, "CypressTree", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)//--------FOREST--------
        {
            GenerateTileObject("object", mushroomVal, mushroomSpawnChance, "BrownShroom", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)//--------GRASSLANDS--------
        {
            GenerateTileObject("object", parsnipVal, parsnipSpawnChance, "WildParsnip", x, y, cell, objectPos);

            GenerateTileObject("item", rockVal, rockSpawnChance, "Rock", x, y, cell, objectPos);
        }
        else if (tileDictionary[new Vector2(x,y)].GetComponent<Cell>().biomeType == Cell.BiomeType.Snowy)//--------SNOWY--------
        {
            GenerateTileObject("mob", wolfVal, wolfSpawnChance, "Wolf", x, y, cell, objectPos);
        }
    }
}
