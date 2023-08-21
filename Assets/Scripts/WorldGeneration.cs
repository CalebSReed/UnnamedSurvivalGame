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

                objectPos.x += Random.Range(-20, 20);
                objectPos.y += Random.Range(-20, 20);
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
                    RealItem.SpawnRealItem(objectPos, new Item { itemSO = ItemObjectArray.Instance.Rock, amount = 1 });
                    
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

                objectPos.x += Random.Range(-20, 21);
                objectPos.y += Random.Range(-20, 21);

                int bunnyVal = Random.Range(0, bunnySpawnChance);
                int carrotVal = Random.Range(0, carrotSpawnChance);
                int wolfVal = Random.Range(0, wolfSpawnChance);
                int turkeyVal = Random.Range(0, turkeySpawnChance);
                int parsnipVal = Random.Range(0, parsnipSpawnChance);
                int mushroomVal = Random.Range(0, mushroomSpawnChance);
                int pondVal = Random.Range(0, pondSpawnChance);
                int copperVal = Random.Range(0, copperSpawnChance);

                //Debug.Log(noiseValue);

                if (biomeGridArray[x, y].biomeType == Cell.BiomeType.MagicalForest)
                {
                    
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Desert)
                {
                    

                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Rocky)
                {
                    if (copperVal == copperSpawnChance - 1)//wolf val for now im lazy
                    {
                        RealItem.SpawnRealItem(objectPos, new Item { itemSO = ItemObjectArray.Instance.RawCopper, amount = 1 });
                    }
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Savannah)
                {
                    
                    if (carrotVal == carrotSpawnChance-1)
                    {
                        RealWorldObject.SpawnWorldObject(carrotPos, new WorldObject { woso = WosoArray.Instance.WildCarrot });
                    }
                    if (bunnyVal == bunnySpawnChance-1)
                    {
                        RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.BunnyHole });
                        RealMob.SpawnMob(objectPos, new Mob { mobSO = MobObjArray.Instance.Bunny });
                    }
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Forest)
                {
                    if (mushroomVal == carrotSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.BrownShroom });
                    }
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Grasslands)
                {
                    if (parsnipVal == parsnipSpawnChance-1)
                    {
                    RealWorldObject.SpawnWorldObject(objectPos, new WorldObject { woso = WosoArray.Instance.WildParsnip });
                    }

                    if (pondVal == pondSpawnChance - 1)
                    {
                        RealWorldObject.SpawnWorldObject(pondPos, new WorldObject { woso = WosoArray.Instance.Pond });
                    }

                    if (turkeyVal == turkeySpawnChance - 1)
                    {
                        RealMob.SpawnMob(objectPos, new Mob { mobSO = MobObjArray.Instance.Turkey });
                    }
                    //Debug.Log("tree");
                }
                else if (biomeGridArray[x, y].biomeType == Cell.BiomeType.Snowy)
                {
                    if (wolfVal == wolfSpawnChance - 1)
                    {
                        RealMob.SpawnMob(objectPos, new Mob { mobSO = MobObjArray.Instance.Wolf });
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
