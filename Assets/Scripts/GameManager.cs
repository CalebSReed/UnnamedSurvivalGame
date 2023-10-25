using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

//mob culling idea: all mobs should be a parent of MOBMANAGER. Save mobs's pos like tiles. Then check all "tiles" around player and if they contain a mob, enable it. Mobs too far will disable selves.

//would it be too expensive to simply change their parent to each tile they walk on? Yes probably...

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public UI_EquipSlot playerHandSlot;
    public GameObject minigame;
    public GameObject chestUI;
    public WorldGeneration world;
    public DayNightCycle dayCycle;
    public MusicManager musicPlayer;
    public GameObject pauseMenu;

    public List<string> objTypeArray;//change name to list not array bro
    public List<Vector2> objTransformArray;
    public List<float> objUsesArray;
    public List<float> objHealthArray;
    public List<bool> attachmentArray;
    public List<int> attachmentIndexArray;
    public List<GameObject> tileList;

    public bool isLoading;

    private string worldSaveFileName;
    private string worldSeedFileName;
    private string worldMobsFileName;
    private string parasiteSaveFileName;

    //public event EventHandler onLoad;

    private bool eraseWarning = false;
    private bool resetWarning = false;

    private bool fastForward;

    private int worldGenSeed;

    public bool subMenuOpen = false;
    public GameObject optionsMenu;

    void Start()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/SaveFiles"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/SaveFiles");
        }
        worldSaveFileName = Application.persistentDataPath + "/SaveFiles/WorldSave.json";
        worldSeedFileName = Application.persistentDataPath + "/SaveFiles/WorldSeed.json";
        worldMobsFileName = Application.persistentDataPath + "/SaveFiles/MobSave.json";
        parasiteSaveFileName = Application.persistentDataPath + "/SaveFiles/ParasiteSave.json";

        if (Application.isEditor)
        {
            dayCycle.currentTime = 222;//so we dont sit thru the slow ass sunrise

            if (!Directory.Exists(Application.persistentDataPath + "/SaveFiles/EDITORSAVES"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/SaveFiles/EDITORSAVES");
            }

            worldSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/WorldSave.json";//new save locations for editor
            worldSeedFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/WorldSeed.json";
            worldMobsFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/MobSave.json";
            parasiteSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/ParasiteSave.json";
            RecipeSaveController.Instance.recipeCraftedSaveFileName = Application.persistentDataPath  + "/SaveFiles/EDITORSAVES/RecipeDiscoveriesSave.json";
            RecipeSaveController.Instance.recipeDiscoverySaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/RecipeCraftedSave.json";
        }

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

        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (fastForward)
            {
                Announcer.SetText("SUPER SPEED DISABLED");
                Time.timeScale = 1;
                fastForward = false;
            }
            else
            {
                Announcer.SetText("SUPER SPEED ENABLED");
                Time.timeScale = 5;
                fastForward = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (subMenuOpen)
            {
                optionsMenu.SetActive(false);
                pauseMenu.SetActive(true);
                subMenuOpen = false;
            }
            else
            {
                TogglePause();
            }
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
            fastForward = false;
            musicPlayer.audio.UnPause("Music1");
            musicPlayer.audio.UnPause("Music2");
        }
    }

    public void SetSubMenuBool(bool open)
    {
        subMenuOpen = open;
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
        File.Delete(parasiteSaveFileName);
        File.Delete(RecipeSaveController.Instance.recipeCraftedSaveFileName);
        File.Delete(RecipeSaveController.Instance.recipeDiscoverySaveFileName);
        Announcer.SetText("SAVA DATA ERASED");
        eraseWarning = false;
    }

    public void Save()//change all this to JSON at some point. That way we can do more things like have multiple save files :)
    {
        //onSave?.Invoke(this, EventArgs.Empty);
        Vector3 playerPos = player.transform.position;
        PlayerPrefs.SetFloat("playerPosX", playerPos.x);
        PlayerPrefs.SetFloat("playerPosY", playerPos.y);
        PlayerPrefs.SetFloat("playerHealth", player.GetComponent<PlayerMain>().hpManager.currentHealth);
        PlayerPrefs.SetInt("playerHunger", player.GetComponent<HungerManager>().currentHunger);
        SavePlayerInventory();
        SavePlayerPlacedItems();
        SavePlayerAndParasiteObjects();
        SaveParasiteData();
        SaveWorld();
        SaveTime();
        SaveWeather();
        RecipeSaveController.Instance.SaveRecipes();
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

            player.GetComponent<PlayerMain>().hpManager.currentHealth = PlayerPrefs.GetFloat("playerHealth");
            player.GetComponent<PlayerMain>().healthBar.SetHealth(player.GetComponent<PlayerMain>().hpManager.currentHealth);
            player.GetComponent<HungerManager>().currentHunger = PlayerPrefs.GetInt("playerHunger");

            Vector3 playerPos = new Vector3(playerPosX, playerPosY);
            player.transform.position = playerPos;
            player.gameObject.GetComponent<PlayerController>().ChangeTarget(playerPos);
            LoadPlayerInventory();
            LoadPlayerPlacedItems();
            LoadPlayerAndParasiteObjects();
            LoadparasiteData();
            LoadWorld();
            LoadTime();
            LoadWeather();
            RecipeSaveController.Instance.LoadRecipes();
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
        player.GetComponent<PlayerMain>().StopHoldingItem();
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
        if (player.GetComponent<PlayerMain>().handSlot.currentItem != null)//handslot
        {
            PlayerPrefs.SetString($"SaveHandItemType", player.GetComponent<PlayerMain>().handSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveHandItemAmount", player.GetComponent<PlayerMain>().handSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveHandItemUses", player.GetComponent<PlayerMain>().handSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveHandItemAmmo", player.GetComponent<PlayerMain>().handSlot.currentItem.ammo);
        }
        else
        {
            PlayerPrefs.SetString($"SaveHandItemType", "Null");
        }

        if (player.GetComponent<PlayerMain>().headSlot.currentItem != null)//headslot
        {
            PlayerPrefs.SetString($"SaveHeadItemType", player.GetComponent<PlayerMain>().headSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveHeadItemAmount", player.GetComponent<PlayerMain>().headSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveHeadItemUses", player.GetComponent<PlayerMain>().headSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveHeadItemAmmo", player.GetComponent<PlayerMain>().headSlot.currentItem.ammo);
        }
        else
        {
            PlayerPrefs.SetString($"SaveHeadItemType", "Null");
        }

        if (player.GetComponent<PlayerMain>().chestSlot.currentItem != null)//chestslot
        {
            PlayerPrefs.SetString($"SaveChestItemType", player.GetComponent<PlayerMain>().chestSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveChestItemAmount", player.GetComponent<PlayerMain>().chestSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveChestItemUses", player.GetComponent<PlayerMain>().chestSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveChestItemAmmo", player.GetComponent<PlayerMain>().chestSlot.currentItem.ammo);
        }
        else
        {
            PlayerPrefs.SetString($"SaveChestItemType", "Null");
        }

        if (player.GetComponent<PlayerMain>().leggingsSlot.currentItem != null)
        {
            PlayerPrefs.SetString($"SaveLeggingsItemType", player.GetComponent<PlayerMain>().leggingsSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveLeggingsItemAmount", player.GetComponent<PlayerMain>().leggingsSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveLeggingsItemUses", player.GetComponent<PlayerMain>().leggingsSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveLeggingsItemAmmo", player.GetComponent<PlayerMain>().leggingsSlot.currentItem.ammo);
        }
        else
        {
            PlayerPrefs.SetString($"SaveLeggingsItemType", "Null");
        }

        if (player.GetComponent<PlayerMain>().feetSlot.currentItem != null)
        {
            PlayerPrefs.SetString($"SaveFeetItemType", player.GetComponent<PlayerMain>().feetSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveFeetItemAmount", player.GetComponent<PlayerMain>().feetSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveFeetItemUses", player.GetComponent<PlayerMain>().feetSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveFeetItemAmmo", player.GetComponent<PlayerMain>().feetSlot.currentItem.ammo);
        }
        else
        {
            PlayerPrefs.SetString($"SaveFeetItemType", "Null");
        }

        PlayerPrefs.SetInt("InventorySize", player.GetComponent<PlayerMain>().inventory.GetItemList().Length);//should always be 32 i believe
    }

    private void LoadPlayerInventory()
    {
        int i = 0;
        player.GetComponent<PlayerMain>().inventory.ClearArray();
        player.GetComponent<PlayerMain>().UnequipItem(player.GetComponent<PlayerMain>().handSlot, false);
        player.GetComponent<PlayerMain>().UnequipItem(player.GetComponent<PlayerMain>().headSlot, false);
        player.GetComponent<PlayerMain>().UnequipItem(player.GetComponent<PlayerMain>().chestSlot, false);
        player.GetComponent<PlayerMain>().UnequipItem(player.GetComponent<PlayerMain>().leggingsSlot, false);
        player.GetComponent<PlayerMain>().UnequipItem(player.GetComponent<PlayerMain>().feetSlot, false);
        player.GetComponent<PlayerMain>().heldItem = null;
        player.GetComponent<PlayerMain>().StopHoldingItem();//save held item later im lazy
        player.GetComponent<PlayerMain>().inventory.ClearArray();
        while (i < PlayerPrefs.GetInt("InventorySize"))//each item in inventory
        {
            if (PlayerPrefs.GetString($"SaveItemType{i}") != "Null")
            {
                //OH MY GOSH GOLLY THATS A LONG LINE
                player.GetComponent<PlayerMain>().inventory.GetItemList()[i] =  new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveItemType{i}")), amount = PlayerPrefs.GetInt($"SaveItemAmount{i}"), uses = PlayerPrefs.GetInt($"SaveItemUses{i}"), ammo = PlayerPrefs.GetInt($"SaveItemAmmo{i}"), equipType = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveItemType{i}")).equipType };
            }
            i++;
        }

        if (PlayerPrefs.GetString($"SaveHandItemType") != "Null")
        {
            player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveHandItemType")), amount = PlayerPrefs.GetInt($"SaveHandItemAmount"), uses = PlayerPrefs.GetInt($"SaveHandItemUses"), ammo = PlayerPrefs.GetInt($"SaveHandItemAmmo"), equipType = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveHandItemType")).equipType });
        }

        if (PlayerPrefs.GetString($"SaveHeadItemType") != "Null")
        {
            player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveHeadItemType")), amount = PlayerPrefs.GetInt($"SaveHeadItemAmount"), uses = PlayerPrefs.GetInt($"SaveHeadItemUses"), ammo = PlayerPrefs.GetInt($"SaveHeadItemAmmo"), equipType = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveHeadItemType")).equipType });
        }

        if (PlayerPrefs.GetString($"SaveChestItemType") != "Null")
        {
            player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveChestItemType")), amount = PlayerPrefs.GetInt($"SaveChestItemAmount"), uses = PlayerPrefs.GetInt($"SaveChestItemUses"), ammo = PlayerPrefs.GetInt($"SaveChestItemAmmo"), equipType = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveChestItemType")).equipType });
        }

        if (PlayerPrefs.GetString($"SaveLeggingsItemType") != "Null")
        {
            player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveLeggingsItemType")), amount = PlayerPrefs.GetInt($"SaveLeggingsItemAmount"), uses = PlayerPrefs.GetInt($"SaveLeggingsItemUses"), ammo = PlayerPrefs.GetInt($"SaveLeggingsItemAmmo"), equipType = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveLeggingsItemType")).equipType });
        }

        if (PlayerPrefs.GetString($"SaveFeetItemType") != "Null")
        {
            player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveFeetItemType")), amount = PlayerPrefs.GetInt($"SaveFeetItemAmount"), uses = PlayerPrefs.GetInt($"SaveFeetItemUses"), ammo = PlayerPrefs.GetInt($"SaveFeetItemAmmo"), equipType = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveFeetItemType")).equipType });
        }

        player.GetComponent<PlayerMain>().uiInventory.RefreshInventoryItems();
    }

    private void SavePlayerPlacedItems()
    {
        int i = 0;
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Item"))
        {
            if (!_obj.transform.parent)//if we ever give them a parent... CHANGE THIS!
            {
                PlayerPrefs.SetString($"SaveGroundItemType{i}", _obj.GetComponent<RealItem>().item.itemSO.itemType);
                PlayerPrefs.SetInt($"SaveGroundItemAmount{i}", _obj.GetComponent<RealItem>().item.amount);
                PlayerPrefs.SetInt($"SaveGroundItemUses{i}", _obj.GetComponent<RealItem>().item.uses);
                PlayerPrefs.SetInt($"SaveGroundItemAmmo{i}", _obj.GetComponent<RealItem>().item.ammo);
                PlayerPrefs.SetFloat($"SaveGroundItemPosX{i}", _obj.transform.position.x);
                PlayerPrefs.SetFloat($"SaveGroundItemPosY{i}", _obj.transform.position.y);
                i++;
            }
        }
        PlayerPrefs.SetInt("SaveTotalAmountOfGroundItemsInWorld", i);
    }

    private void LoadPlayerPlacedItems()
    {
        foreach(GameObject _obj in GameObject.FindGameObjectsWithTag("Item"))
        {
            if (!_obj.transform.parent)
            {
                Destroy(_obj);
            }
        }

        int i = 0;
        while (i < PlayerPrefs.GetInt("SaveTotalAmountOfGroundItemsInWorld"))
        {
            RealItem _item = RealItem.SpawnRealItem(new Vector3(PlayerPrefs.GetFloat($"SaveGroundItemPosX{i}"), PlayerPrefs.GetFloat($"SaveGroundItemPosY{i}")), new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveGroundItemType{i}")), ammo = PlayerPrefs.GetInt($"SaveGroundItemAmmo{i}"), amount = PlayerPrefs.GetInt($"SaveGroundItemAmount{i}"), uses = PlayerPrefs.GetInt($"SaveGroundItemUses{i}")});
            i++;
        }
    }

    private void SavePlayerAndParasiteObjects()
    {
        objTypeArray.Clear();
        objUsesArray.Clear();
        objTransformArray.Clear();
        objHealthArray.Clear();
        attachmentArray.Clear();
        attachmentIndexArray.Clear();


        int i = 0;
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("WorldObject"))
        {
            if (_obj.GetComponent<RealWorldObject>().obj.woso.isPlayerMade || _obj.GetComponent<RealWorldObject>().obj.woso.isParasiteMade)
            {
                if (_obj.GetComponent<DoorBehavior>() != null && _obj.GetComponent<DoorBehavior>().isOpen)
                {
                    _obj.GetComponent<DoorBehavior>().ToggleOpen();
                }

                RealWorldObject _realObj = _obj.GetComponent<RealWorldObject>();
                objTypeArray.Add(_obj.GetComponent<RealWorldObject>().obj.woso.objType);
                objTransformArray.Add(_obj.transform.position);
                objUsesArray.Add(_obj.GetComponent<RealWorldObject>().actionsLeft);
                objHealthArray.Add(_obj.GetComponent<HealthManager>().currentHealth);
                attachmentArray.Add(_obj.GetComponent<RealWorldObject>().hasAttachment);
                attachmentIndexArray.Add(_obj.GetComponent<RealWorldObject>().attachmentIndex);
                SaveObjectData(_obj, i);


                if (_realObj.obj.woso.isContainer)
                {
                    for (int index = 0; index < _realObj.inventory.GetItemList().Length; index++)//i = object index, index = inventory index
                    {
                        if (_realObj.inventory.GetItemList()[index] != null)
                        {
                            PlayerPrefs.SetString($"SaveObjectItemType{i}{index}", _realObj.inventory.GetItemList()[index].itemSO.itemType);
                            PlayerPrefs.SetInt($"SaveObjectItemAmount{i}{index}", _realObj.inventory.GetItemList()[index].amount);
                            PlayerPrefs.SetInt($"SaveObjectItemUses{i}{index}", _realObj.inventory.GetItemList()[index].uses);
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
        foreach (float uses in objUsesArray)
        {
            PlayerPrefs.SetFloat($"SaveObjectUses{i}", objUsesArray[i]);
            i++;
        }
        i = 0;
        foreach (float hp in objHealthArray)
        {
            PlayerPrefs.SetFloat($"SaveObjectHealth{i}", objHealthArray[i]);
            i++;
        }
        i = 0;
        foreach (bool _bool in attachmentArray)
        {
            PlayerPrefs.SetInt($"SaveIfObjectHasAttachment{i}", Convert.ToInt32(attachmentArray[i]));
            i++;
        }
        i = 0;
        foreach (int index in attachmentIndexArray)
        {
            PlayerPrefs.SetInt($"SaveObjectAttachmentIndex{i}", attachmentIndexArray[i]);
            i++;
        }

        PlayerPrefs.SetInt($"SaveObjectsAmount", objTypeArray.Count);
    }

    private void LoadPlayerAndParasiteObjects()
    {
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("WorldObject"))
        {
            if (_obj.GetComponent<RealWorldObject>().obj.woso.isPlayerMade || _obj.GetComponent<RealWorldObject>().obj.woso.isParasiteMade)
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
                _obj.GetComponent<HealthManager>().currentHealth = PlayerPrefs.GetFloat($"SaveObjectHealth{i}");
                LoadObjectData(_obj, i);

                if (Convert.ToBoolean(PlayerPrefs.GetInt($"SaveIfObjectHasAttachment{i}")))
                {
                    _obj.AttachItem(new Item { itemSO = _obj.obj.woso.itemAttachments[PlayerPrefs.GetInt($"SaveObjectAttachmentIndex{i}")] }, false );//attach item
                    Debug.Log("Attaching item to loaded object");
                }

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

    private void SaveObjectData(GameObject _obj, int i)//this dont work cuz the list changes upon loading, so move this inside of playermade objects save and use this for each object we save and load thanks
    {
        if (_obj.GetComponent<FarmingManager>() != null)//it feels really bad having to save VERY specific variables 
        {
            if (_obj.GetComponent<FarmingManager>().seed != null)
            {
                PlayerPrefs.SetString($"SaveFarmSeedType{i}", _obj.GetComponent<FarmingManager>().seed.itemType);
            }
            else
            {
                PlayerPrefs.SetString($"SaveFarmSeedType{i}", "");
            }
            PlayerPrefs.SetInt($"SaveFarmProgress{i}", _obj.GetComponent<FarmingManager>().growthTimer);
            PlayerPrefs.SetInt($"SaveIfFarmHarvestable{i}", Convert.ToInt32(_obj.GetComponent<FarmingManager>().isHarvestable));
            PlayerPrefs.SetInt($"SaveIfGrowing{i}", Convert.ToInt32(_obj.GetComponent<FarmingManager>().isGrowing));
            PlayerPrefs.SetInt($"SaveIfPlanted{i}", Convert.ToInt32(_obj.GetComponent<FarmingManager>().isPlanted));
        }
    }

    private void LoadObjectData(RealWorldObject _obj, int i)
    {
        if (_obj.GetComponent<FarmingManager>() != null)//it feels really bad having to save VERY specific variables 
        {
            var farm = _obj.GetComponent<FarmingManager>();
            farm.seed = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveFarmSeedType{i}"));
            farm.growthTimer = PlayerPrefs.GetInt($"SaveFarmProgress{i}");
            farm.isHarvestable = Convert.ToBoolean(PlayerPrefs.GetInt($"SaveIfFarmHarvestable{i}"));
            farm.isGrowing = Convert.ToBoolean(PlayerPrefs.GetInt($"SaveIfGrowing{i}"));
            farm.isPlanted = Convert.ToBoolean(PlayerPrefs.GetInt($"SaveIfPlanted{i}"));
            if (farm.seed != null)
            {
                farm.plantSpr.sprite = farm.seed.itemSprite;
            }

            if (farm.isGrowing)
            {
                StartCoroutine(farm.GrowPlant());
            }
            else if (farm.isHarvestable)
            {
                farm.BecomeHarvestable();
            }
        }
    }

    private void SaveParasiteData()
    {
        var parasiteSaveJson = JsonConvert.SerializeObject(ParasiteFactionManager.parasiteData, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(parasiteSaveFileName, string.Empty);
        File.WriteAllText(parasiteSaveFileName, parasiteSaveJson);
    }

    private void LoadparasiteData()
    {
        if (File.Exists(parasiteSaveFileName))
        {
            ParasiteFactionManager.Instance.StopAllCoroutines();//stop checking player
            var parasiteSaveJson = File.ReadAllText(parasiteSaveFileName);
            var parasiteSave = JsonConvert.DeserializeObject<ParasiteFactionData>(parasiteSaveJson);

            ParasiteFactionManager.parasiteData.PlayerBase = parasiteSave.PlayerBase;
            ParasiteFactionManager.parasiteData.ParasiteBase = parasiteSave.ParasiteBase;
            ParasiteFactionManager.parasiteData.PlayerBaseExists = parasiteSave.PlayerBaseExists;
            ParasiteFactionManager.parasiteData.ParasiteBaseExists = parasiteSave.ParasiteBaseExists;
            ParasiteFactionManager.parasiteData.ParasiteTechLevel = parasiteSave.ParasiteTechLevel;
            ParasiteFactionManager.parasiteData.checkingPlayerLocation = parasiteSave.checkingPlayerLocation;
            ParasiteFactionManager.parasiteData.raidCooldown = parasiteSave.raidCooldown;
            ParasiteFactionManager.parasiteData.raidDifficultyMult = parasiteSave.raidDifficultyMult;
            ParasiteFactionManager.parasiteData.scouterDifficultyMult = parasiteSave.scouterDifficultyMult;

            if (ParasiteFactionManager.parasiteData.checkingPlayerLocation)
            {
                StartCoroutine(ParasiteFactionManager.Instance.CheckPlayerLocation());
            }
        }
        else
        {
            Debug.LogError("No parasite data found");
        }
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

        var worldSeed = JsonConvert.SerializeObject(new Vector2(world.randomOffsetX, world.randomOffsetY), Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
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
                    _tile.transform.position = _tileData.tileLocation;
                    _tile.transform.position = new Vector2(_tile.transform.position.x - world.worldSize, _tile.transform.position.y - world.worldSize);
                    _tile.transform.position *= 25;
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
                        var realItem = RealItem.SpawnRealItem(_tileData.itemLocations[i], new Item { itemSO = ItemObjectArray.Instance.SearchItemList(_tileData.itemTypes[i]), amount = 1, equipType = ItemObjectArray.Instance.SearchItemList(_tileData.itemTypes[i]).equipType });
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
            var worldSeed = JsonConvert.DeserializeObject<Vector2>(worldSeedJson);
            world.randomOffsetX = worldSeed.x;
            world.randomOffsetX = worldSeed.y;
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
        PlayerPrefs.SetInt("SaveCurrentSeason", (int)dayCycle.currentSeason);
        PlayerPrefs.SetInt("SaveSeasonProgress", dayCycle.currentSeasonProgress);
    }

    private void LoadTime()
    {
        if (PlayerPrefs.HasKey("SaveCurrentTime"))
        {
        dayCycle.LoadNewTime(PlayerPrefs.GetInt("SaveCurrentTime"), PlayerPrefs.GetInt("SaveCurrentDay"), PlayerPrefs.GetInt("SaveCurrentDayOfYear"), PlayerPrefs.GetInt("SaveCurrentYear"), PlayerPrefs.GetInt("SaveCurrentSeason"), PlayerPrefs.GetInt("SaveSeasonProgress"));
        }
        else
        {
            Debug.LogError("NO TIME SAVE FOUND");
        }
    }

    private void SaveWeather()
    {
        PlayerPrefs.SetInt("SaveRainProgress", WeatherManager.Instance.rainProgress);
        PlayerPrefs.SetInt("SaveThunderProgress", WeatherManager.Instance.thunderProgress);
        PlayerPrefs.SetInt("SaveStormCooldown", WeatherManager.Instance.stormCooldown);
        PlayerPrefs.SetInt("SaveRainingBool", Convert.ToInt32(WeatherManager.Instance.isRaining));
        PlayerPrefs.SetInt("SaveTargetBool", Convert.ToInt32(WeatherManager.Instance.targetReached));
    }

    private void LoadWeather()
    {
        if (PlayerPrefs.HasKey("SaveRainProgress"))
        {
            WeatherManager.Instance.LoadWeatherData(PlayerPrefs.GetInt("SaveRainProgress"), PlayerPrefs.GetInt("SaveThunderProgress"), PlayerPrefs.GetInt("SaveStormCooldown"), Convert.ToBoolean(PlayerPrefs.GetInt("SaveRainingBool")), Convert.ToBoolean(PlayerPrefs.GetInt("SaveTargetBool")));
        }
        else
        {
            Debug.LogError("NO WEATHER SAVE FOUND");
        }
    }
}
