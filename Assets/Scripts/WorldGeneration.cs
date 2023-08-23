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

    public Cell[,] biomeGridArray;
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
                objectPos.x -= sizeSquared / 2;
                objectPos.y -= sizeSquared / 2;
                objectPos.x += 625;
                objectPos.y += 625;

                int magicalTreeVal = Random.Range(0, magicalTreeSpawnChance);

                objectPos.x += Random.Range(-20, 21);
                objectPos.y += Random.Range(-20, 21);

                Vector3 newPos = objectPos;

                //Debug.Log(noiseValue);

                if (biomeGridArray[x, y].biomeType == Cell.BiomeType.MagicalForest)
                {

                    if (magicalTreeVal == magicalTreeSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.MagicalTree });
                    }
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Desert)
                {
                    RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.Sapling });
                    
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Rocky)
                {
                    RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.Boulder });
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Savannah)
                {
                    RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.Milkweed });
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Forest)
                {
                    RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.Tree });
                    
                    //Debug.Log("dirtmound");
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Grasslands)
                {
                    RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.ClayDeposit });
                    //Debug.Log("tree");
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Snowy)
                {
                    //RealItem.SpawnRealItem(objectPos, new Item { itemSO = ItemObjectArray.Instance.Rock, amount = 1 });
                    
                    //Debug.Log("boulder");
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
                objectPos.x -= sizeSquared / 2;
                objectPos.y -= sizeSquared / 2;
                objectPos.x += 625;
                objectPos.y += 625;


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
                int birchVal = Random.Range(0, birchSpawnChance);
                int tinVal = Random.Range(0, tinSpawnChance);

                //Debug.Log(noiseValue);

                if (biomeGridArray[x, y].biomeType == Cell.BiomeType.MagicalForest)//--------MAGIC--------
                {
                    
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Desert)//--------DESERT--------
                {
                    

                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Rocky)//--------ROCKY--------
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
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Savannah)//--------SAVANNAH--------
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
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Forest)//--------FOREST--------
                {
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (mushroomVal == mushroomSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.BrownShroom });
                    }
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (birchVal == birchSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.BirchTree });
                    }
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Grasslands)//--------GRASSLANDS--------
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
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (pondVal == pondSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(newPos, new WorldObject { woso = WosoArray.Instance.Pond });
                    }
                    newPos = objectPos;
                    newPos.x += Random.Range(-20, 21);
                    newPos.y += Random.Range(-20, 21);
                    if (turkeyVal == turkeySpawnChance - 1)
                    {
                        RealMob.SpawnMob(newPos, new Mob { mobSO = MobObjArray.Instance.Turkey });
                    }
                    //Debug.Log("tree");
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Snowy)//--------SNOWY--------
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
        biomeGridArray = new Cell[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = noiseMap[x, y];
                Cell cell = new Cell();
                if (noiseValue > .75f)
                {
                    cell.biomeType = Cell.BiomeType.MagicalForest;
                }
                else if (noiseValue > .6f)
                {
                    cell.biomeType = Cell.BiomeType.Desert;
                }
                else if (noiseValue > .5f)
                {
                    cell.biomeType = Cell.BiomeType.Rocky;
                }
                else if (noiseValue > .4f)
                {
                    cell.biomeType = Cell.BiomeType.Savannah;
                }
                else if (noiseValue > .3f)
                {
                    cell.biomeType = Cell.BiomeType.Forest;
                }
                else if (noiseValue > .2f)
                {
                    cell.biomeType = Cell.BiomeType.Grasslands;
                }
                else if (noiseValue <= .2f)
                {
                    cell.biomeType = Cell.BiomeType.Snowy;
                }
                else
                {
                    Debug.LogError("SOMETHING HAPPENED");
                }
                biomeGridArray[x, y] = cell; 
            }
        }
    }
}
