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

    public GameObject[,] biomeGridArray;
    public List<Sprite> TileList;
    public GameObject groundTileObject;
    float randomOffset;

    void Start()
    {
        randomOffset = Random.Range(-offset, offset);
        biomeGridArray = new GameObject[worldSize * 2, worldSize * 2];
        StartCoroutine(CheckPlayerPosition());
    }

    private float GetPerlinNoise(int x, int y)
    {
        
        float noiseValue = Mathf.PerlinNoise(x * scale + randomOffset, y * scale + randomOffset);
        return noiseValue;
    }

    private IEnumerator CheckPlayerPosition()//hey BUCKO my games been running fine since THIS! Something in the worldgen script is collecting HELLA GARBAGE! (I THINK) Clean up this shit bruh
    {
        yield return new WaitForSeconds(1f);
        int x = player.cellPosition[0]+worldSize;
        int y = player.cellPosition[1]+worldSize;

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
            if (biomeGridArray.GetValue(tempValX, tempValY) == null)//if is length BUT cell is null/deactivated then reactivate BAM! old check: biomeGridArray.GetLength(0) < player.cellPosition[0] || biomeGridArray.GetLength(1) < player.cellPosition[1]
            {
                //Debug.LogError($"GENERATING NEW TILE AT {tempValX}, {tempValY}");
                GenerateTile(tempValX, tempValY);
            }
            else if (!biomeGridArray[tempValX, tempValY].gameObject.activeSelf)
            {
                //Debug.LogError("ACTIVATE");
                biomeGridArray[tempValX, tempValY].gameObject.SetActive(true);
            }
            xi++;
        }
        //Debug.Log($"CHECKING FOR PLAYER!!! THEY ARE AT {player.cellPosition[0]}, {player.cellPosition[1]}... x length is {biomeGridArray.GetLength(0)} and y length is {biomeGridArray.GetLength(1)}");

        StartCoroutine(CheckPlayerPosition());
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
        biomeGridArray[x,y] = groundTile;
        groundTile.SetActive(true);
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

    private void GenerateTileObjects(GameObject _tile, int x, int y)
    {
        //Vector3 objectPos = new Vector3(x * 25, y * 25);
        //objectPos = CalculateObjectPos(objectPos);

        Vector3 objectPos = _tile.transform.position;

        objectPos.x += Random.Range(-20, 21);
        objectPos.y += Random.Range(-20, 21);

        Vector3 newPos = objectPos;

        int magicalTreeVal = Random.Range(0, magicalTreeSpawnChance);
        int birchVal = Random.Range(0, birchSpawnChance);
        int wheatVal = Random.Range(0, wheatSpawnChance);
        int goldBoulderVal = Random.Range(0, goldBoulderSpawnChance);
        int boulderVal = Random.Range(0, boulderSpawnChance);

        //Debug.Log(noiseValue);

        if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)
        {

            if (magicalTreeVal == magicalTreeSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("MagicalTree") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Desert)
        {

        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Rocky)
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (boulderVal == boulderSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Boulder") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (wheatVal == wheatSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Wheat") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)
        {
            var tempObj = RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("ClayDeposit") });
            tempObj.transform.parent = biomeGridArray[x, y].transform;
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)
        {
            var tempObj = RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Tree") });
            tempObj.transform.parent = biomeGridArray[x, y].transform;

            //Debug.Log("dirtmound");
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)
        {

            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);

            if (birchVal == birchSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("BirchTree") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            var tempObj2 = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Milkweed") });
            tempObj2.transform.parent = biomeGridArray[x, y].transform;

            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            var tempObj3 = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Sapling") });
            tempObj3.transform.parent = biomeGridArray[x, y].transform;
            //Debug.Log("tree");
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Snowy)
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);

            if (boulderVal == boulderSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Boulder") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);

            if (goldBoulderVal == goldBoulderSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("GoldBoulder") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
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

        if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)//--------MAGIC--------
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (mushroomVal == mushroomSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("PurpleFungTree") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Desert)//--------DESERT--------
        {


        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Rocky)//--------ROCKY--------
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (copperVal == copperSpawnChance - 1)//wolf val for now im lazy
            {
                var tempObj = RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawCopper"), amount = 1 });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (tinVal == tinSpawnChance - 1)//wolf val for now im lazy
            {
                var tempObj = RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawTin"), amount = 1 });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (rockVal == rockSpawnChance - 1)
            {
                var tempObj = RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Rock"), amount = 1 });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (sheepVal == sheepSpawnChance - 1)
            {
                var tempObj = RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.Sheep });
                //tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)//--------PRAIRIE--------
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (carrotVal == carrotSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("WildCarrot") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (bunnyVal == bunnySpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("BunnyHole") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
                var tempObj2 = RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.Bunny });
                //tempObj2.transform.parent = biomeGridArray[x, y].transform;
            }
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (turkeyVal == turkeySpawnChance - 1)
            {
                var tempObj = RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.Turkey });
                //tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)//--------SWAMP--------
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (pondVal == pondSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("Pond") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (cypressVal == cypressSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("CypressTree") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)//--------FOREST--------
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (mushroomVal == mushroomSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("BrownShroom") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }

        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)//--------GRASSLANDS--------
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (parsnipVal == parsnipSpawnChance - 1)
            {
                var tempObj = RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList("WildParsnip") });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (rockVal == rockSpawnChance - 1)
            {
                var tempObj = RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Rock"), amount = 1 });
                tempObj.transform.parent = biomeGridArray[x, y].transform;
            }


            //Debug.Log("tree");
        }
        else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Snowy)//--------SNOWY--------
        {
            newPos = objectPos;
            newPos.x += Random.Range(-20, 21);
            newPos.y += Random.Range(-20, 21);
            if (wolfVal == wolfSpawnChance - 1)
            {
                var tempObj = RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.Wolf });
                //tempObj.transform.parent = biomeGridArray[x, y].transform;
            }

            //Debug.Log("boulder");
        }
    }
}
