using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public GameObject minigame;
    public GameObject chestUI;
    public WorldGeneration world;
    public DayNightCycle dayCycle;
    public MusicManager musicPlayer;
    public GameObject pauseMenu;

    public List<string> objTypeArray;//change name to list not array bro
    public List<Vector2> objTransformArray;
    public List<float> objUsesArray;
    public List<GameObject> tileList;

    public bool isLoading;

    private string worldSaveFileName;
    private string worldSeedFileName;
    private string worldMobsFileName;

    //public event EventHandler onLoad;

    private bool eraseWarning = false;
    private bool resetWarning = false;

    private int worldGenSeed;

    void Start()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/SaveFiles"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/SaveFiles");
        }
        worldSaveFileName = Application.persistentDataPath + "/SaveFiles/worldSave.json";
        worldSeedFileName = Application.persistentDataPath + "/SaveFiles/worldSeed.json";
        worldMobsFileName = Application.persistentDataPath + "/SaveFiles/mobSave.json";

        minigame = GameObject.FindGameObjectWithTag("Bellow");
        minigame.SetActive(false);
        chestUI.SetActive(false);
        //UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        Debug.Log("SEED SET!");
        worldGenSeed = (int)DateTime.Now.Ticks;
        world.GenerateWorld();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (!resetWarning && player != null)
            {
                Announcer.SetText("WARNING: HIT F9 AGAIN TO RESTART THE GAME", Color.red);
                resetWarning = true;
                Invoke(nameof(ResetWorldWarning), 3f);
            }
            else
            {
                Time.timeScale = 1;
                SceneManager.LoadScene(0);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (!pauseMenu.activeSelf)
        {
            musicPlayer.audio.Pause("Music1");
            musicPlayer.audio.Pause("Music2");
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            musicPlayer.audio.UnPause("Music1");
            musicPlayer.audio.UnPause("Music2");
        }
    }

    public void AttemptToClearSaveData()
    {
        if (!eraseWarning)
        {
            Announcer.SetText("WARNING: CLICK BUTTON AGAIN TO CLEAR ALL SAVE DATA", Color.red);
            eraseWarning = true;
            Invoke(nameof(ResetEraseWarning), 3f);
        }
        else
        {
            ClearAllSaveData();
        }
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    private void ResetEraseWarning()
    {
        eraseWarning = false;
    }

    private void ResetWorldWarning()
    {
        resetWarning = false;
    }

    public void ClearAllSaveData()
    {
        PlayerPrefs.DeleteAll();
        File.Delete(worldSaveFileName);
        File.Delete(worldSeedFileName);
        File.Delete(worldMobsFileName);
        Announcer.SetText("SAVA DATA ERASED");
        eraseWarning = false;
    }

    public void Save()//change all this to JSON at some point. That way we can do more things like have multiple save files :)
    {
        //onSave?.Invoke(this, EventArgs.Empty);
        Vector3 playerPos = player.transform.position;
        PlayerPrefs.SetFloat("playerPosX", playerPos.x);
        PlayerPrefs.SetFloat("playerPosY", playerPos.y);
        PlayerPrefs.SetInt("playerHealth", player.GetComponent<PlayerMain>().currentHealth);
        PlayerPrefs.SetInt("playerHunger", player.GetComponent<HungerManager>().currentHunger);
        SavePlayerInventory();
        SavePlayerObjects();
        SaveWorld();
        SaveTime();
        Announcer.SetText("SAVED");
        PlayerPrefs.Save();

        Debug.Log($"SAVED POSITION: {playerPos}");
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("playerPosX"))
        {
            float playerPosX = PlayerPrefs.GetFloat("playerPosX");
            float playerPosY = PlayerPrefs.GetFloat("playerPosY");

            player.GetComponent<PlayerMain>().currentHealth = PlayerPrefs.GetInt("playerHealth");
            player.GetComponent<PlayerMain>().TakeDamage(0);
            player.GetComponent<HungerManager>().currentHunger = PlayerPrefs.GetInt("playerHunger");

            Vector3 playerPos = new Vector3(playerPosX, playerPosY);
            player.transform.position = playerPos;
            player.gameObject.GetComponent<PlayerController>().ChangeTarget(playerPos);
            LoadPlayerInventory();
            LoadPlayerObjects();
            LoadWorld();
            LoadTime();
            TogglePause();
            Announcer.SetText("LOADED");
        }
        else
        {
            Announcer.SetText("ERROR: SAVE NOT FOUND", Color.red);
        }
    }

    private void SavePlayerInventory()
    {
        Inventory playerInv = player.GetComponent<PlayerMain>().inventory;
        for (int i = 0; i < playerInv.GetItemList().Length; i++)
        {
            if (playerInv.GetItemList()[i] != null)
            {
                PlayerPrefs.SetString($"SaveItemType{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].itemSO.itemType);//ah yes I see why we need an ID system for these scriptable objects to be saved... damnit
                PlayerPrefs.SetInt($"SaveItemAmount{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].amount);//TODO implement database for objs, items, and mobs... ugh
                PlayerPrefs.SetInt($"SaveItemUses{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].uses);//hah loser i just made a public list to search for SOs. Dict and ID syst would be cool still tho....
                PlayerPrefs.SetInt($"SaveItemAmmo{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].ammo);
            }
            else//if null
            {
                PlayerPrefs.SetString($"SaveItemType{i}", "Null");//if item is null, save empty string, and skip this slot when we load
            }
        }
        if (player.GetComponent<PlayerMain>().handSlot.item != null)
        {
            PlayerPrefs.SetString($"SaveHandItemType", player.GetComponent<PlayerMain>().handSlot.item.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveHandItemAmount", player.GetComponent<PlayerMain>().handSlot.item.amount);
            PlayerPrefs.SetInt($"SaveHandItemUses", player.GetComponent<PlayerMain>().handSlot.item.uses);
            PlayerPrefs.SetInt($"SaveHandItemAmmo", player.GetComponent<PlayerMain>().handSlot.item.ammo);
        }
        else
        {
            PlayerPrefs.SetString($"SaveHandItemType", "Null");
        }
        PlayerPrefs.SetInt("InventorySize", player.GetComponent<PlayerMain>().inventory.GetItemList().Length);//should always be 32 i believe
    }

    private void LoadPlayerInventory()
    {
        int i = 0;
        player.GetComponent<PlayerMain>().inventory.ClearArray();
        player.GetComponent<PlayerMain>().handSlot.RemoveItem();
        player.GetComponent<PlayerMain>().equippedHandItem = null;
        player.GetComponent<PlayerMain>().UnequipItem();//will spawn a null
        player.GetComponent<PlayerMain>().StopHoldingItem();//save held item later im lazy
        while (i < PlayerPrefs.GetInt("InventorySize"))//each item in inventory
        {
            if (PlayerPrefs.GetString($"SaveItemType{i}") != "Null")
            {
                //OH MY GOSH GOLLY THATS A LONG LINE
                player.GetComponent<PlayerMain>().inventory.GetItemList()[i] =  new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveItemType{i}")), amount = PlayerPrefs.GetInt($"SaveItemAmount{i}"), uses = PlayerPrefs.GetInt($"SaveItemUses{i}"), ammo = PlayerPrefs.GetInt($"SaveItemAmmo{i}") };
            }
            i++;
        }

        if (PlayerPrefs.GetString($"SaveHandItemType") != "Null")
        {
            player.GetComponent<PlayerMain>().handSlot.SetItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveHandItemType")), amount = PlayerPrefs.GetInt($"SaveHandItemAmount"), uses = PlayerPrefs.GetInt($"SaveHandItemUses"), ammo = PlayerPrefs.GetInt($"SaveHandItemAmmo") }, PlayerPrefs.GetInt($"SaveHandItemUses"));
            player.GetComponent<PlayerMain>().EquipItem(player.GetComponent<PlayerMain>().handSlot.item);
        } 
        
        player.GetComponent<PlayerMain>().uiInventory.RefreshInventoryItems();
    }

    private void SavePlayerObjects()
    {
        objTypeArray.Clear();
        objUsesArray.Clear();
        objTransformArray.Clear();


        int i = 0;
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("WorldObject"))
        {
            if (_obj.GetComponent<RealWorldObject>().obj.woso.isPlayerMade)
            {
                RealWorldObject _realObj = _obj.GetComponent<RealWorldObject>();
                objTypeArray.Add(_obj.GetComponent<RealWorldObject>().obj.woso.objType);
                objTransformArray.Add(_obj.transform.position);
                objUsesArray.Add(_obj.GetComponent<RealWorldObject>().actionsLeft);

                if (_realObj.obj.woso.isContainer)
                {
                    for (int index = 0; index < _realObj.inventory.GetItemList().Length; index++)//i = object index, index = inventory index
                    {
                        if (_realObj.inventory.GetItemList()[index] != null)
                        {
                            PlayerPrefs.SetString($"SaveObjectItemType{i}{index}", _realObj.inventory.GetItemList()[index].itemSO.itemType);//ah yes I see why we need an ID system for these scriptable objects to be saved... damnit
                            PlayerPrefs.SetInt($"SaveObjectItemAmount{i}{index}", _realObj.inventory.GetItemList()[index].amount);//TODO implement database for objs, items, and mobs... ugh
                            PlayerPrefs.SetInt($"SaveObjectItemUses{i}{index}", _realObj.inventory.GetItemList()[index].uses);//hah loser i just made a public list to search for SOs. Dict and ID syst would be cool still tho....
                            PlayerPrefs.SetInt($"SaveObjectItemAmmo{i}{index}", _realObj.inventory.GetItemList()[index].ammo);
                        }
                        else
                        {
                            PlayerPrefs.SetString($"SaveObjectItemType{i}{index}", "Null");//if item is null, save empty string, and skip this slot when we load
                        }
                    }
                }

                i++;//should only increase everytime we find playermade obj
            }

        }


        i = 0;
        foreach (string _type in objTypeArray)
        {
            PlayerPrefs.SetString($"SaveObjectType{i}", objTypeArray[i]);
            i++;
        }
        i = 0;
        foreach (Vector2 _transform in objTransformArray)
        {
            PlayerPrefs.SetFloat($"SaveObjectPosX{i}", objTransformArray[i].x);
            PlayerPrefs.SetFloat($"SaveObjectPosY{i}", objTransformArray[i].y);
            i++;
        }

        i = 0;
        foreach (int uses in objUsesArray)
        {
            PlayerPrefs.SetFloat($"SaveObjectUses{i}", objUsesArray[i]);
            i++;
        }

        PlayerPrefs.SetInt($"SaveObjectsAmount", objTypeArray.Count);
    }

    private void LoadPlayerObjects()
    {
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("WorldObject"))
        {
            if (_obj.GetComponent<RealWorldObject>().obj.woso.isPlayerMade)
            {
                if (_obj.GetComponent<RealWorldObject>().obj.woso.isContainer && _obj.GetComponent<RealWorldObject>().IsContainerOpen())
                {
                    _obj.GetComponent<RealWorldObject>().CloseContainer();
                }
                Destroy(_obj);
            }
        }
        

        if (PlayerPrefs.GetInt($"SaveObjectsAmount") > 0 && PlayerPrefs.HasKey("SaveObjectPosX0"))
        {
            int i = 0;
            while (i < PlayerPrefs.GetInt($"SaveObjectsAmount"))
            {
                Debug.Log($"Loading {PlayerPrefs.GetString($"SaveObjectType{i}")}");
                RealWorldObject _obj = RealWorldObject.SpawnWorldObject(new Vector3(PlayerPrefs.GetFloat($"SaveObjectPosX{i}"), PlayerPrefs.GetFloat($"SaveObjectPosY{i}"), 0), new WorldObject { woso = WosoArray.Instance.SearchWOSOList(PlayerPrefs.GetString($"SaveObjectType{i}")) }, true, PlayerPrefs.GetFloat($"SaveObjectUses{i}"));

                if (_obj.obj.woso.isContainer)
                {
                    for (int index = 0; index < _obj.inventory.GetItemList().Length; index++)//i = object index, index = inventory index
                    {
                        if (PlayerPrefs.GetString($"SaveObjectItemType{i}{index}") != "Null")
                        {
                            _obj.inventory.GetItemList()[index] = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveObjectItemType{i}{index}")), ammo = PlayerPrefs.GetInt($"SaveObjectItemAmmo{i}{index}"), amount = PlayerPrefs.GetInt($"SaveObjectItemAmount{i}{index}") , uses = PlayerPrefs.GetInt($"SaveObjectItemUses{i}{index}") };
                        }
                    }
                }

                i++;
            }
        }
        //onLoad?.Invoke(this, EventArgs.Empty);
    }

    private void SaveWorld()//CRITICAL Something with saving mobs broke so I have to fix that somewhere....
    {
        //PlayerPrefs.SetInt("RandomSeed", worldGenSeed);

        List<TileData> tileDataList = new List<TileData>();
        foreach (GameObject obj in world.TileObjList)
        {
            tileDataList.Add(obj.GetComponent<Cell>().tileData);
        }
        Debug.Log(tileDataList.Count);
        List<MobSaveData> mobSaveList = new List<MobSaveData>();
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Mob"))
        {
            RealMob _mob = _obj.GetComponent<RealMob>();
            _mob.mobSaveData.mobLocations[0] = _mob.transform.position;
            mobSaveList.Add(_mob.mobSaveData);
        }

        var mobListJson = JsonConvert.SerializeObject(mobSaveList, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(worldMobsFileName, string.Empty);
        File.WriteAllText(worldMobsFileName, mobListJson);

        var tileJson = JsonConvert.SerializeObject(tileDataList, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(worldSaveFileName, string.Empty);
        File.WriteAllText(worldSaveFileName, tileJson);

        var worldSeed = JsonConvert.SerializeObject(world.randomOffset);
        File.WriteAllText(worldSeedFileName, string.Empty);
        File.WriteAllText(worldSeedFileName, worldSeed);
        Debug.Log("done saving world");
    }

    private void LoadWorld()
    {
        if (File.Exists(worldSaveFileName))
        {
            //world.StopAllCoroutines();
            isLoading = true;
            //UnityEngine.Random.InitState(PlayerPrefs.GetInt("RandomSeed"));
            world.tileDictionary.Clear();
            //var gos = GameObject.FindGameObjectsWithTag("Tile");
            foreach (GameObject _obj in world.TileObjList)//need to search this list because we cant grab disabled objs without references + we never delete tiles mid-game
            {
                Debug.Log("Destroyed " + _obj);
                Destroy(_obj);
            }

            var mgos = GameObject.FindGameObjectsWithTag("Mob");//search all mobs in the scene, they get destroyed and stuff blah blah blah

            foreach (GameObject obj in mgos)
            {
                Debug.Log("Destroyed mob: " + obj);
                Destroy(obj);
            }

            world.mobList.Clear();
            world.TileObjList.Clear();

            //UnityEngine.Random.state
            var worldSaveJson = File.ReadAllText(worldSaveFileName);
            var tileListJson = JsonConvert.DeserializeObject<List<TileData>>(worldSaveJson);
            var mobSaveJson = File.ReadAllText(worldMobsFileName);
            var mobListJson = JsonConvert.DeserializeObject<List<MobSaveData>>(mobSaveJson);

            foreach(TileData _tileData in tileListJson)
            {
                if (!world.tileDictionary.ContainsKey(_tileData.tileLocation))//sucks we gotta do this hope it works tho 
                {
                    var _tile = Instantiate(world.groundTileObject);
                    _tile.GetComponent<SpriteRenderer>().sprite = world.LoadSprite(_tileData.biomeType);
                    _tile.transform.position = _tileData.tileLocation;//i hope these arent sharing the same pointers, nah theyre integers aint no way bruh
                    //Debug.Log(_tile.transform.position+ "   1");
                    _tile.transform.position = new Vector2(_tile.transform.position.x - world.worldSize, _tile.transform.position.y - world.worldSize);
                    //Debug.Log(_tile.transform.position+ "   2");
                    _tile.transform.position *= 25;
                    //Debug.Log(_tile.transform.position+ "   3");
                    _tile.GetComponent<Cell>().tileData.tileLocation = _tileData.tileLocation;
                    _tile.GetComponent<Cell>().tileData.biomeType = _tileData.biomeType;
                    world.tileDictionary.Add(_tileData.tileLocation, _tile);
                    world.TileObjList.Add(_tile);

                    int i = 0;
                    foreach (string obj in _tileData.objTypes)//we should save the object placement random seed but eh im lazy
                    {
                        var wObj = RealWorldObject.SpawnWorldObject(_tileData.objLocations[i], new WorldObject { woso = WosoArray.Instance.SearchWOSOList(_tileData.objTypes[i]) });
                        wObj.transform.parent = _tile.transform;
                        _tile.GetComponent<Cell>().tileData.objTypes.Add(wObj.obj.woso.objType);
                        _tile.GetComponent<Cell>().tileData.objLocations.Add(wObj.transform.position);
                        i++;
                    }
                    i = 0;
                    foreach (string item in _tileData.itemTypes)
                    {
                        var realItem = RealItem.SpawnRealItem(_tileData.itemLocations[i], new Item { itemSO = ItemObjectArray.Instance.SearchItemList(_tileData.itemTypes[i]), amount = 1 });
                        realItem.transform.parent = _tile.transform;
                        _tile.GetComponent<Cell>().tileData.itemTypes.Add(realItem.item.itemSO.itemType);
                        _tile.GetComponent<Cell>().tileData.itemLocations.Add(realItem.transform.position);
                        i++;
                    }
                }

            }
            foreach(MobSaveData _mob in mobListJson)
            {
                var realMob = RealMob.SpawnMob(_mob.mobLocations[0], new Mob { mobSO = MobObjArray.Instance.SearchMobList(_mob.mobTypes[0]) });
            }
            //var worldSaveJson = File.ReadAllText(worldSaveFileName);
            //var dictionaryJson = JsonConvert.DeserializeObject<Dictionary<Vector2, GameObject>>(worldSaveJson);//in future make a new class, we save that class and then load its dictionary, perlin noise seed, ETC!
            //world.tileDictionary = dictionaryJson;

            var worldSeedJson = File.ReadAllText(worldSeedFileName);
            var worldSeed = JsonConvert.DeserializeObject<float>(worldSeedJson);
            world.randomOffset = worldSeed;
            isLoading = false;
            //StartCoroutine(world.CheckPlayerPosition());
        }
        else
        {
            Debug.LogError("WORLD SAVE NOT FOUND ");
        }
    }

    private void SaveTime()
    {
        PlayerPrefs.SetInt("SaveCurrentTime", dayCycle.currentTime);
        PlayerPrefs.SetInt("SaveCurrentDay", dayCycle.currentDay);
        PlayerPrefs.SetInt("SaveCurrentDayOfYear", dayCycle.currentDayOfYear);
        PlayerPrefs.SetInt("SaveCurrentYear", dayCycle.currentYear);
    }

    private void LoadTime()
    {
        if (PlayerPrefs.HasKey("SaveCurrentTime"))
        {
        dayCycle.LoadNewTime(PlayerPrefs.GetInt("SaveCurrentTime"), PlayerPrefs.GetInt("SaveCurrentDay"), PlayerPrefs.GetInt("SaveCurrentDayOfYear"), PlayerPrefs.GetInt("SaveCurrentYear"));
        }
        else
        {
            Announcer.SetText("ERROR: COULD NOT FIND TIME", Color.red);
        }
    }
}
