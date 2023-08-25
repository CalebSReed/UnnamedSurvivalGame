using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    public int size;
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

    public GameObject[,] biomeGridArray;
    public List<Sprite> TileList;
    public GameObject groundTileObject;
    float[,] noiseMap;

    void Start()
    {        
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        GeneratePerlinNoise();
        GenerateTiles();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                //Debug.Log(biomeGridArray[x, y].biomeType);
                float sizeSquared = size * size;

                Vector3 objectPos = new Vector3(x * 25, y * 25);
                objectPos = CalculateObjectPos(objectPos);

                int magicalTreeVal = Random.Range(0, magicalTreeSpawnChance);

                objectPos.x += Random.Range(-20, 21);
                objectPos.y += Random.Range(-20, 21);

                Vector3 newPos = objectPos;

                int birchVal = Random.Range(0, birchSpawnChance);

                //Debug.Log(noiseValue);

                if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)
                {

                    if (magicalTreeVal == magicalTreeSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.MagicalTree });
                    }
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Desert)
                {
                                       
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Rocky)
                {
                    RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.Boulder });
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)
                {
                    
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)
                {
                    RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.ClayDeposit });
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)
                {
                    RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.Tree });
                    
                    //Debug.Log("dirtmound");
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)
                {

                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);

                    if (birchVal == birchSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.BirchTree });
                    }
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.Milkweed });

                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.Sapling });
                    //Debug.Log("tree");
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Snowy)
                {
                    
                }
            }
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                //Debug.Log(biomeGridArray[x, y].biomeType);

                float sizeSquared = size * size;
                Vector3 objectPos = new Vector3(x * 25, y * 25);
                objectPos = CalculateObjectPos(objectPos);


                Vector3 carrotPos = objectPos;
                carrotPos.x += Random.Range(-20, 21);
                carrotPos.y += Random.Range(-20, 21);

                Vector3 pondPos = objectPos;
                pondPos.x += Random.Range(-20, 21);
                pondPos.y += Random.Range(-20, 21);

                Vector3 birchPos = objectPos;
                birchPos.x += Random.Range(-20, 21);
                birchPos.y += Random.Range(-20, 21);

                //objectPos.x += Random.Range(-20, 21);
                //objectPos.y += Random.Range(-20, 21);

                Vector3 newPos = objectPos;

                int bunnyVal = Random.Range(0, bunnySpawnChance);
                int carrotVal = Random.Range(0, carrotSpawnChance);
                int wolfVal = Random.Range(0, wolfSpawnChance);
                int turkeyVal = Random.Range(0, turkeySpawnChance);
                int parsnipVal = Random.Range(0, parsnipSpawnChance);
                int mushroomVal = Random.Range(0, mushroomSpawnChance);
                int pondVal = Random.Range(0, pondSpawnChance);
                int copperVal = Random.Range(0, copperSpawnChance);
                int rockVal = Random.Range(0, rockSpawnChance);

                int tinVal = Random.Range(0, tinSpawnChance);

                //Debug.Log(noiseValue);

                if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.MagicalForest)//--------MAGIC--------
                {
                    
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
                        RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.RawCopper, amount = 1 });
                    }
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (tinVal == tinSpawnChance - 1)//wolf val for now im lazy
                    {
                        RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.RawTin, amount = 1 });
                    }
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (rockVal == rockSpawnChance - 1)
                    {
                        RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.Rock, amount = 1 });
                    }
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Savannah)//--------PRAIRIE--------
                {
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (carrotVal == carrotSpawnChance-1)
                    {
                        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.WildCarrot });
                    }
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (bunnyVal == bunnySpawnChance-1)
                    {
                        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.BunnyHole });
                        RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.Bunny });
                    }
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (turkeyVal == turkeySpawnChance - 1)
                    {
                        RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.Turkey });
                    }
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Swamp)//--------SWAMP--------
                {
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (pondVal == pondSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.Pond });
                    }
                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Forest)//--------FOREST--------
                {
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (mushroomVal == mushroomSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.BrownShroom });
                    }

                }
                else if (biomeGridArray[x, y].GetComponent<Cell>().biomeType == Cell.BiomeType.Grasslands)//--------GRASSLANDS--------
                {
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (parsnipVal == parsnipSpawnChance-1)
                    {
                    RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.WildParsnip });
                    }
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (rockVal == rockSpawnChance - 1)
                    {
                        RealItem.SpawnRealItem(newPos, new Item { itemSO = ItemObjectArray.Instance.Rock, amount = 1 });
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
                        RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.Wolf });
                    }

                    //Debug.Log("boulder");
                }
            }
        }
    }

    public void GeneratePerlinNoise()
    {
        noiseMap = new float[size, size];
        float randomOffset = Random.Range(-offset, offset);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * scale + randomOffset, y * scale + randomOffset);
                //Debug.Log(noiseValue);
                noiseMap[x, y] = noiseValue;
            }
        }
    }

    public void GenerateTiles()
    {
        biomeGridArray = new GameObject[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = noiseMap[x, y];
                GameObject groundTile = Instantiate(groundTileObject);
                groundTile.GetComponent<SpriteRenderer>().sprite = null;

                Vector3 objectPos = new Vector3(x * 25, y * 25);
                objectPos = CalculateObjectPos(objectPos);


                groundTile.transform.position = objectPos;

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
                    groundTile.GetComponent<SpriteRenderer>().sprite = TileList[7];
                }
                else if (noiseValue > .4f)
                {
                    cell.biomeType = Cell.BiomeType.Forest;
                    groundTile.GetComponent<SpriteRenderer>().sprite = TileList[5];
                }
                else if (noiseValue > .3f)
                {
                    cell.biomeType = Cell.BiomeType.Swamp;
                    groundTile.GetComponent<SpriteRenderer>().sprite = TileList[1];
                }
                else if (noiseValue > .2f)
                {
                    cell.biomeType = Cell.BiomeType.Snowy;
                    groundTile.GetComponent<SpriteRenderer>().sprite = TileList[3];
                }
                if (noiseValue <= .2f)
                {
                    cell.biomeType = Cell.BiomeType.MagicalForest;
                    groundTile.GetComponent<SpriteRenderer>().sprite = TileList[6];
                }
                else
                {
                    Debug.LogError("SOMETHING HAPPENED");
                }
                biomeGridArray[x, y] = groundTile; 
            }
        }
    }

    private Vector3 CalculateObjectPos(Vector3 objectPos)
    {

        float sizeMultiplied = size * 25;//25 units separate em
        objectPos.x -= sizeMultiplied / 2;
        objectPos.y -= sizeMultiplied / 2;
        //objectPos.x -= sizeSquared / 4;
        //objectPos.y -= sizeSquared / 4;
        return objectPos;
    }
}
