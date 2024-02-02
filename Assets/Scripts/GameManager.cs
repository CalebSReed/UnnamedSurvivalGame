using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.InputSystem;

//mob culling idea: all mobs should be a parent of MOBMANAGER. Save mobs's pos like tiles. Then check all "tiles" around player and if they contain a mob, enable it. Mobs too far will disable selves.

//would it be too expensive to simply change their parent to each tile they walk on? Yes probably...

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    public GameObject player;
    public Transform pfProjectile;
    private PlayerMain playerMain;
    public UI_EquipSlot playerHandSlot;
    public GameObject minigame;
    public GameObject chestUI;
    public WorldGeneration world;
    public DayNightCycle dayCycle;
    public MusicManager musicPlayer;
    public GameObject pauseMenu;
    public GameObject journal;

    public List<string> objTypeArray;//change name to list not array bro
    public List<Vector3> objTransformArray;
    public List<float> objUsesArray;
    public List<float> objHealthArray;
    public List<bool> attachmentArray;
    public List<int> attachmentIndexArray;
    public List<GameObject> tileList;

    private PlayerSaveData playerSave = new PlayerSaveData();

    public bool isLoading;

    private string playerInfoSaveFileName;
    public string worldSaveFileName;
    private string worldSeedFileName;
    private string worldMobsFileName;
    private string parasiteSaveFileName;
    private string journalSaveFileName;
    private string weatherSaveFileName;
    private string timeSaveFileName;

    //public event EventHandler onLoad;

    private bool eraseWarning = false;
    private bool resetWarning = false;

    private bool fastForward;

    private int worldGenSeed;

    public bool subMenuOpen = false;
    public GameObject optionsMenu;
    [SerializeField] private Animator craftingUIanimator;
    private bool uiActive = false;
    public Vector3 playerHome;

    void Start()
    {
        Instance = this;
        if (!Directory.Exists(Application.persistentDataPath + "/SaveFiles"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/SaveFiles");
        }
        playerInfoSaveFileName = Application.persistentDataPath + "/SaveFiles/PlayerInfo.json";
        worldSaveFileName = Application.persistentDataPath + "/SaveFiles/WorldSave.json";
        worldSeedFileName = Application.persistentDataPath + "/SaveFiles/WorldSeed.json";
        worldMobsFileName = Application.persistentDataPath + "/SaveFiles/MobSave.json";
        parasiteSaveFileName = Application.persistentDataPath + "/SaveFiles/ParasiteSave.json";
        journalSaveFileName = Application.persistentDataPath + "/SaveFiles/JournalSave.json";
        weatherSaveFileName = Application.persistentDataPath + "/SaveFiles/WeatherSave.json";
        timeSaveFileName = Application.persistentDataPath + "/SaveFiles/TimeSave.json";

        if (Application.isEditor)
        {
            dayCycle.currentTime = 222;//so we dont sit thru the slow ass sunrise

            if (!Directory.Exists(Application.persistentDataPath + "/SaveFiles/EDITORSAVES"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/SaveFiles/EDITORSAVES");
            }

            playerInfoSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/PlayerInfo.json";
            worldSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/WorldSave.json";//new save locations for editor
            worldSeedFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/WorldSeed.json";
            worldMobsFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/MobSave.json";
            parasiteSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/ParasiteSave.json";
            journalSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/JournalSave.json";
            RecipeSaveController.Instance.recipeCraftedSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/RecipeCraftedSave.json";
            RecipeSaveController.Instance.recipeDiscoverySaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/RecipeDiscoveriesSave.json";
            weatherSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/WeatherSave.json";
            timeSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/TimeSave.json";
        }

        playerMain = player.GetComponent<PlayerMain>();
        minigame = GameObject.FindGameObjectWithTag("Bellow");
        minigame.SetActive(false);
        chestUI.SetActive(false);
        journal.SetActive(false);
        craftingUIanimator.SetBool("Open", false);
        craftingUIanimator.SetBool("Close", true);
        //UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        Debug.Log("SEED SET!");
        worldGenSeed = (int)DateTime.Now.Ticks;
        world.GenerateWorld();
        DayNightCycle.Instance.OnDawn += DoDawnTasks;
    }

    private void DoDawnTasks(object sender, EventArgs e)
    {
        if (DayNightCycle.Instance.currentDay > 1 && !DayNightCycle.Instance.isLoading)//if loading, DONT SAVE AGAIN LOL!
        {
            StartCoroutine(DoDailySave());
        }
    }

    private IEnumerator DoDailySave()
    {
        Announcer.SetText("Saving...");
        yield return null;
        Save();
    }

    public void ResetGame(InputAction.CallbackContext context)
    {
        if (context.performed)
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
    }

    public void ToggleGodMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!playerMain.godMode)
            {
                Announcer.SetText("GOD MODE ENABLED");
                playerMain.hpManager.RestoreHealth(9999);
                playerMain.healthBar.SetHealth(playerMain.hpManager.currentHealth);
                playerMain.godMode = true;
            }
            else
            {
                Announcer.SetText("GOD MODE DISABLED");
                playerMain.godMode = false;
            }
        }
    }

    public void ToggleGodMode()
    {
        if (!playerMain.godMode)
        {
            Announcer.SetText("GOD MODE ENABLED");
            playerMain.hpManager.RestoreHealth(9999);
            playerMain.healthBar.SetHealth(playerMain.hpManager.currentHealth);
            playerMain.godMode = true;
        }
        else
        {
            Announcer.SetText("GOD MODE DISABLED");
            playerMain.godMode = false;
        }
    }

    public void ToggleSpeedMode(InputAction.CallbackContext context)
    {
        if (context.performed)
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
    }

    public void ToggleSpeedMode()
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

    public void ToggleFreeCrafting(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }
        if (!playerMain.freeCrafting)
        {
            Announcer.SetText("FREE CRAFTING ENABLED");
            playerMain.freeCrafting = true;
            playerMain.audio.Play("KilnLight1", player.transform.position, gameObject);
            playerMain.inventory.RefreshInventory();
        }
        else
        {
            Announcer.SetText("FREE CRAFTING DISABLED");
            playerMain.freeCrafting = false;
            playerMain.audio.Stop("KilnLight1");
            playerMain.audio.Play("KilnOut", player.transform.position, gameObject);
            playerMain.inventory.RefreshInventory();
        }
    }

    public void OnToggleFreeStuff(InputAction.CallbackContext context)
    {
        if (context.performed)//cheats
        {
            Announcer.SetText("ITEMS SPAWNED");
            StartCoroutine(NightEventManager.Instance.SummonDepthWalkers(true));
            return;

            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 5, 0, playerMain.transform.position.z + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("BronzeAxe"), amount = 1 });
            //RealItem.SpawnRealItem(new Vector3(3, -2), new Item { itemSO = ItemObjectArray.Instance.RawCopper, amount = 7 });
            //RealItem.SpawnRealItem(new Vector3(4, -2), new Item { itemSO = ItemObjectArray.Instance.Twig, amount = 11 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 2, 0, playerMain.transform.position.z + -4), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Spear"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 2, 0, playerMain.transform.position.z + -2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Clay"), amount = 20 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -4, 0, playerMain.transform.position.z + 5), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("decimator"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -6, 0, playerMain.transform.position.z + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Arrow"), amount = 13 });
            //RealItem.SpawnRealItem(new Vector3(12, 2), new Item { itemSO = ItemObjectArray.Instance.WoodenClub, amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 10, 0, playerMain.transform.position.z + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawMeat"), amount = 4 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -6, 0, playerMain.transform.position.z + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawRabbit"), amount = 6 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -6, 0, playerMain.transform.position.z + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawDrumstick"), amount = 6 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -6, 0, playerMain.transform.position.z + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayBowl"), amount = 10 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -6, 0, playerMain.transform.position.z + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("DeadBunny"), amount = 10 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 12, 0, playerMain.transform.position.z + 2), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Bone"), amount = 2 });
            //RealItem.SpawnRealItem(new Vector3(10, -15), new Item { itemSO = ItemObjectArray.Instance.RawTin, amount = 4 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 10, 0, playerMain.transform.position.z + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("TinIngot"), amount = 4 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 10, 0, playerMain.transform.position.z + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("CopperIngot"), amount = 4 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 10, 0, playerMain.transform.position.z + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ClayPlate"), amount = 4 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 15, 0, playerMain.transform.position.z + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Log"), amount = 20 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 15, 0, playerMain.transform.position.z + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("Charcoal"), amount = 20 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 25, 0, playerMain.transform.position.z + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("BronzeIngot"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 25, 0, playerMain.transform.position.z + -15), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("BagBellows"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 35, 0, playerMain.transform.position.z + -5), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawGold"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 45, 0, playerMain.transform.position.z + -25), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("RawMutton"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -25, 0, playerMain.transform.position.z + -45), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("ParsnipSeeds"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -25, 0, playerMain.transform.position.z + -45), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("CarrotSeeds"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -25, 0, playerMain.transform.position.z + -45), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("GoldCrown"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + -25, 0, playerMain.transform.position.z + -45), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("hardenedchestplate"), amount = 1 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 25, 0, playerMain.transform.position.z + 25), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("elderberryseeds"), amount = 10 });
            RealItem.SpawnRealItem(new Vector3(playerMain.transform.position.x + 25, 0, playerMain.transform.position.z + 25), new Item { itemSO = ItemObjectArray.Instance.SearchItemList("wheatseeds"), amount = 10 });
            RealMob.SpawnMob(new Vector3(playerMain.transform.position.x + 25, 0, playerMain.transform.position.z + 25), new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
            RealMob.SpawnMob(new Vector3(playerMain.transform.position.x + 25, 0, playerMain.transform.position.z + 25), new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });
            RealMob.SpawnMob(new Vector3(playerMain.transform.position.x + 25, 0, playerMain.transform.position.z + 25), new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
            RealMob.SpawnMob(new Vector3(playerMain.transform.position.x + 0, 0, playerMain.transform.position.z - 25), new Mob { mobSO = MobObjArray.Instance.SearchMobList("Mud Trekker") });
        }
    }

    public void OnToggleCraftingButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OpenCloseCraftingTab();
        }
    }

    public void OnToggleJournalButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleJournal();
        }
    }

    public void OnTogglePauseButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePause();
        }
    }

    public void OnToggleRetry(InputAction.CallbackContext context)
    {
        if (context.performed && playerMain.StateMachine.currentPlayerState == playerMain.deadState)
        {
            player.transform.position = playerHome;
            playerMain.hpManager.SetCurrentHealth(playerMain.maxHealth/4);
            playerMain.hungerManager.SetHunger(playerMain.maxHunger/4);
            playerMain.healthBar.SetHealth(playerMain.hpManager.currentHealth);
            playerMain.StateMachine.ChangeState(playerMain.defaultState, true);
        }
    }

    public void OpenCloseCraftingTab()
    {
        if (uiActive)
        {
            craftingUIanimator.SetBool("Open", false);
            craftingUIanimator.SetBool("Close", true);
            uiActive = false;
        }
        else
        {
            craftingUIanimator.SetBool("Open", true);
            craftingUIanimator.SetBool("Close", false);
            uiActive = true;
        }
    }

    public void ToggleJournal()
    {
        if (!subMenuOpen)
        {
            journal.SetActive(!journal.activeSelf);
            TogglePause(true);
            if (!journal.activeSelf)
            {
                CheckIfNewEntrySeen();
                pauseMenu.transform.localScale = new Vector3(.75f, .75f, .75f);
            }
            else
            {
                journal.GetComponentInParent<UI_JournalBehavior>().CheckIfNewEntrySeen();
            }
        }
        else
        {
            TogglePause();
        }
    }

    public void TogglePause(bool openJournal = false)
    {
        if (subMenuOpen)
        {
            subMenuOpen = false;
            optionsMenu.SetActive(false);
            journal.SetActive(false);
            CheckIfNewEntrySeen();
            pauseMenu.SetActive(true);
            pauseMenu.transform.localScale = new Vector3(.75f, .75f, .75f);
        }
        else if (!pauseMenu.activeSelf)
        {
            musicPlayer.audio.Pause("Music1");
            musicPlayer.audio.Pause("Music2");
            musicPlayer.audio.Pause("Music3");
            musicPlayer.audio.Pause("Battle");
            musicPlayer.audio.Pause("BattleLoop");
            pauseMenu.SetActive(true);
            if (openJournal)
            {
                pauseMenu.transform.localScale = new Vector3(.5f, .5f, .5f);
            }
            Time.timeScale = 0f;
        }
        else
        {
            journal.SetActive(false);
            pauseMenu.SetActive(false);
            pauseMenu.transform.localScale = new Vector3(.75f, .75f, .75f);

            CheckIfNewEntrySeen();

            Time.timeScale = 1f;
            subMenuOpen = false;
            fastForward = false;
            musicPlayer.audio.UnPause("Music1");
            musicPlayer.audio.UnPause("Music2");
            musicPlayer.audio.UnPause("Music3");
            musicPlayer.audio.UnPause("Battle");
            musicPlayer.audio.UnPause("BattleLoop");
        }
    }

    private void CheckIfNewEntrySeen()
    {
        if (UI_JournalBehavior.Instance.entrySeen)
        {
            if (UI_JournalBehavior.Instance.newEntry != null)
            {
                UI_JournalBehavior.Instance.newEntry.color = Color.black;
            }
            UI_JournalBehavior.Instance.entrySeen = false;
            UI_JournalBehavior.Instance.newEntry = null;
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
        File.Delete(playerInfoSaveFileName);
        File.Delete(worldSeedFileName);
        File.Delete(worldMobsFileName);
        File.Delete(parasiteSaveFileName);
        File.Delete(journalSaveFileName);
        File.Delete(RecipeSaveController.Instance.recipeCraftedSaveFileName);
        File.Delete(RecipeSaveController.Instance.recipeDiscoverySaveFileName);
        Announcer.SetText("SAVA DATA ERASED");
        eraseWarning = false;
    }

    public void Save()//change all this to JSON at some point. That way we can do more things like have multiple save files :)
    {
        //onSave?.Invoke(this, EventArgs.Empty);
        playerSave.playerPos = player.transform.position;
        playerSave.playerPos.y = 0;
        playerSave.health = player.GetComponentInParent<HealthManager>().currentHealth;
        playerSave.hunger = player.GetComponentInParent<HungerManager>().currentHunger;
        SavePlayerInventory();
        SavePlayerPlacedItems();
        SavePlayerAndParasiteObjects();
        SaveParasiteData();
        SaveWorld();
        SaveTime();
        SaveWeather();
        SaveJournal();
        RecipeSaveController.Instance.SaveRecipes();
        Announcer.SetText("SAVED");
        PlayerPrefs.Save();

        Debug.Log($"SAVED POSITION: {playerSave.playerPos}");
    }

    public void Load()
    {
        if (File.Exists(playerInfoSaveFileName))
        {
            if (playerMain.StateMachine.currentPlayerState == playerMain.deadState)
            {
                playerMain.StateMachine.ChangeState(playerMain.defaultState, true);
            }

            var playerSaveJson = File.ReadAllText(playerInfoSaveFileName);
            var playerJsonSave = JsonConvert.DeserializeObject<PlayerSaveData>(playerSaveJson);
            playerSave = playerJsonSave;

            player.GetComponent<PlayerMain>().hpManager.currentHealth = playerSave.health;
            player.GetComponent<PlayerMain>().healthBar.SetHealth(playerSave.health);
            player.GetComponent<HungerManager>().currentHunger = playerSave.hunger;

            player.transform.position = playerSave.playerPos;
            //player.gameObject.GetComponent<PlayerController>().ChangeTarget(playerPos);
            LoadPlayerInventory();
            LoadPlayerPlacedItems();
            LoadPlayerAndParasiteObjects();
            LoadWorld();
            LoadparasiteData();
            LoadTime();
            LoadWeather();
            LoadJournal();
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
        playerSave.playerInvTypes.Clear();
        playerSave.playerInvDurabilities.Clear();
        playerSave.playerInvAmounts.Clear();
        playerSave.playerInvAmmo.Clear();
        player.GetComponent<PlayerMain>().StopHoldingItem();
        Inventory playerInv = player.GetComponent<PlayerMain>().inventory;
        PlayerMain main = player.GetComponent<PlayerMain>();
        for (int i = 0; i < playerInv.GetItemList().Length; i++)
        {
            if (playerInv.GetItemList()[i] != null)
            {
                playerSave.playerInvTypes.Add(i, playerInv.GetItemList()[i].itemSO.itemType);
                playerSave.playerInvAmounts.Add(i, playerInv.GetItemList()[i].amount);
                playerSave.playerInvDurabilities.Add(i, playerInv.GetItemList()[i].uses);
                playerSave.playerInvAmmo.Add(i, playerInv.GetItemList()[i].ammo);

                /*PlayerPrefs.SetString($"SaveItemType{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].itemSO.itemType);//ah yes I see why we need an ID system for these scriptable objects to be saved... damnit
                PlayerPrefs.SetInt($"SaveItemAmount{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].amount);//TODO implement database for objs, items, and mobs... ugh
                PlayerPrefs.SetInt($"SaveItemUses{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].uses);//hah loser i just made a public list to search for SOs. Dict and ID syst would be cool still tho....
                PlayerPrefs.SetInt($"SaveItemAmmo{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].ammo);*/
            }
            else//if null
            {
                playerSave.playerInvTypes.Add(i, "Null");//if item is null, save empty string, and skip this slot when we load
            }
        }
        if (player.GetComponent<PlayerMain>().handSlot.currentItem != null)//handslot
        {
            playerSave.handItemType = main.handSlot.currentItem.itemSO.itemType;
            playerSave.handItemAmount = main.handSlot.currentItem.amount;
            playerSave.handItemUses = main.handSlot.currentItem.uses;
            playerSave.handItemAmmo = main.handSlot.currentItem.ammo;

            /*PlayerPrefs.SetString($"SaveHandItemType", player.GetComponent<PlayerMain>().handSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveHandItemAmount", player.GetComponent<PlayerMain>().handSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveHandItemUses", player.GetComponent<PlayerMain>().handSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveHandItemAmmo", player.GetComponent<PlayerMain>().handSlot.currentItem.ammo);*/
        }
        else
        {
            playerSave.handItemType = "Null";
        }

        if (player.GetComponent<PlayerMain>().headSlot.currentItem != null)//headslot
        {
            playerSave.headItemType = main.headSlot.currentItem.itemSO.itemType;
            playerSave.headItemAmount = main.headSlot.currentItem.amount;
            playerSave.headItemUses = main.headSlot.currentItem.uses;
            playerSave.headItemAmmo = main.headSlot.currentItem.ammo;

            /*PlayerPrefs.SetString($"SaveHeadItemType", player.GetComponent<PlayerMain>().headSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveHeadItemAmount", player.GetComponent<PlayerMain>().headSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveHeadItemUses", player.GetComponent<PlayerMain>().headSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveHeadItemAmmo", player.GetComponent<PlayerMain>().headSlot.currentItem.ammo);*/
        }
        else
        {
            playerSave.headItemType = "Null";
        }

        if (player.GetComponent<PlayerMain>().chestSlot.currentItem != null)//chestslot
        {
            playerSave.chestItemType = main.chestSlot.currentItem.itemSO.itemType;
            playerSave.chestItemAmount = main.chestSlot.currentItem.amount;
            playerSave.chestItemUses = main.chestSlot.currentItem.uses;
            playerSave.chestItemAmmo = main.chestSlot.currentItem.ammo;

            /*PlayerPrefs.SetString($"SaveChestItemType", player.GetComponent<PlayerMain>().chestSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveChestItemAmount", player.GetComponent<PlayerMain>().chestSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveChestItemUses", player.GetComponent<PlayerMain>().chestSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveChestItemAmmo", player.GetComponent<PlayerMain>().chestSlot.currentItem.ammo);*/
        }
        else
        {
            playerSave.chestItemType = "Null";
        }

        if (player.GetComponent<PlayerMain>().leggingsSlot.currentItem != null)
        {
            playerSave.legsItemType = main.leggingsSlot.currentItem.itemSO.itemType;
            playerSave.legsItemAmount = main.leggingsSlot.currentItem.amount;
            playerSave.legsItemUses = main.leggingsSlot.currentItem.uses;
            playerSave.legsItemAmmo = main.leggingsSlot.currentItem.ammo;

            /*PlayerPrefs.SetString($"SaveLeggingsItemType", player.GetComponent<PlayerMain>().leggingsSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveLeggingsItemAmount", player.GetComponent<PlayerMain>().leggingsSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveLeggingsItemUses", player.GetComponent<PlayerMain>().leggingsSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveLeggingsItemAmmo", player.GetComponent<PlayerMain>().leggingsSlot.currentItem.ammo);*/
        }
        else
        {
            playerSave.legsItemType = "Null";
        }

        if (player.GetComponent<PlayerMain>().feetSlot.currentItem != null)
        {
            playerSave.feetItemType = main.feetSlot.currentItem.itemSO.itemType;
            playerSave.feetItemAmount = main.feetSlot.currentItem.amount;
            playerSave.feetItemUses = main.feetSlot.currentItem.uses;
            playerSave.feetItemAmmo = main.feetSlot.currentItem.ammo;

            /*PlayerPrefs.SetString($"SaveFeetItemType", player.GetComponent<PlayerMain>().feetSlot.currentItem.itemSO.itemType);
            PlayerPrefs.SetInt($"SaveFeetItemAmount", player.GetComponent<PlayerMain>().feetSlot.currentItem.amount);
            PlayerPrefs.SetInt($"SaveFeetItemUses", player.GetComponent<PlayerMain>().feetSlot.currentItem.uses);
            PlayerPrefs.SetInt($"SaveFeetItemAmmo", player.GetComponent<PlayerMain>().feetSlot.currentItem.ammo);*/
        }
        else
        {
            playerSave.feetItemType = "Null";
        }

        var playerSaveJson = JsonConvert.SerializeObject(playerSave, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(playerInfoSaveFileName, string.Empty);
        File.WriteAllText(playerInfoSaveFileName, playerSaveJson);

        //PlayerPrefs.SetInt("InventorySize", player.GetComponent<PlayerMain>().inventory.GetItemList().Length);//should always be 32 i believe
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

        if (File.Exists(playerInfoSaveFileName))
        {
            var playerSaveJson = File.ReadAllText(playerInfoSaveFileName);
            var playerJsonSave = JsonConvert.DeserializeObject<PlayerSaveData>(playerSaveJson);

            while (i < player.GetComponent<PlayerMain>().inventory.GetItemList().Length)//each item in inventory
            {
                
                if (playerJsonSave.playerInvTypes.ContainsKey(i))
                {
                    playerJsonSave.playerInvTypes.TryGetValue(i, out string itemType);
                    playerJsonSave.playerInvAmounts.TryGetValue(i, out int itemAmount);
                    playerJsonSave.playerInvDurabilities.TryGetValue(i, out int itemUses);
                    playerJsonSave.playerInvAmmo.TryGetValue(i, out int itemAmmo);

                    if (itemType == "Null")
                    {
                        //do nothing
                    }
                    else
                    {
                        player.GetComponent<PlayerMain>().inventory.GetItemList()[i] = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(itemType), amount = itemAmount, uses = itemUses, ammo = itemAmmo, equipType = ItemObjectArray.Instance.SearchItemList(itemType).equipType };
                    }
                }
                i++;
            }

            if (playerJsonSave.handItemType != "Null")
            {
                player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(playerJsonSave.handItemType), amount = playerJsonSave.handItemAmount, uses = playerJsonSave.handItemUses, ammo = playerJsonSave.handItemAmmo, equipType = ItemObjectArray.Instance.SearchItemList(playerJsonSave.handItemType).equipType }, playerMain.handSlot);
            }

            if (playerJsonSave.headItemType != "Null")
            {
                player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(playerJsonSave.headItemType), amount = playerJsonSave.headItemAmount, uses = playerJsonSave.headItemUses, ammo = playerJsonSave.headItemAmmo, equipType = ItemObjectArray.Instance.SearchItemList(playerJsonSave.headItemType).equipType }, playerMain.headSlot);
            }

            if (playerJsonSave.chestItemType != "Null")
            {
                player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(playerJsonSave.chestItemType), amount = playerJsonSave.chestItemAmount, uses = playerJsonSave.chestItemUses, ammo = playerJsonSave.chestItemAmmo, equipType = ItemObjectArray.Instance.SearchItemList(playerJsonSave.chestItemType).equipType }, playerMain.chestSlot);
            }

            if (playerJsonSave.legsItemType != "Null")
            {
                player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(playerJsonSave.legsItemType), amount = playerJsonSave.legsItemAmount, uses = playerJsonSave.legsItemUses, ammo = playerJsonSave.legsItemAmmo, equipType = ItemObjectArray.Instance.SearchItemList(playerJsonSave.legsItemType).equipType }, playerMain.leggingsSlot);
            }

            if (playerJsonSave.feetItemType != "Null")
            {
                player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(playerJsonSave.feetItemType), amount = playerJsonSave.feetItemAmount, uses = playerJsonSave.feetItemUses, ammo = playerJsonSave.feetItemAmmo, equipType = ItemObjectArray.Instance.SearchItemList(playerJsonSave.feetItemType).equipType }, playerMain.feetSlot);
            }

            player.GetComponent<PlayerMain>().uiInventory.RefreshInventoryItems();
        }
        else
        {
            Debug.LogError("No player data found");
        }
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
                PlayerPrefs.SetFloat($"SaveGroundItemPosY{i}", _obj.transform.position.z);
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
            if (PlayerPrefs.GetString($"SaveGroundItemType{i}") != "")
            {
                RealItem _item = RealItem.SpawnRealItem(new Vector3(PlayerPrefs.GetFloat($"SaveGroundItemPosX{i}"), 0, PlayerPrefs.GetFloat($"SaveGroundItemPosY{i}")), new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveGroundItemType{i}")), ammo = PlayerPrefs.GetInt($"SaveGroundItemAmmo{i}"), amount = PlayerPrefs.GetInt($"SaveGroundItemAmount{i}"), uses = PlayerPrefs.GetInt($"SaveGroundItemUses{i}")}, true, true, PlayerPrefs.GetInt($"SaveGroundItemAmmo{i}"));
            }
            else
            {
                Debug.LogError("Skipped null item");
            }
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
            if (_obj.GetComponent<RealWorldObject>() != null && _obj.GetComponent<RealWorldObject>().obj.woso.isPlayerMade || _obj.GetComponent<RealWorldObject>() != null && _obj.GetComponent<RealWorldObject>().obj.woso.isParasiteMade)
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
            PlayerPrefs.SetFloat($"SaveObjectPosY{i}", objTransformArray[i].z);
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
            if (_obj.GetComponent<RealWorldObject>() != null && _obj.GetComponent<RealWorldObject>().obj.woso.isPlayerMade || _obj.GetComponent<RealWorldObject>() != null && _obj.GetComponent<RealWorldObject>().obj.woso.isParasiteMade)
            {
                if (_obj.GetComponent<RealWorldObject>().obj.woso.isContainer && _obj.GetComponent<RealWorldObject>().IsContainerOpen())
                {
                    _obj.GetComponent<Storage>().CloseContainer();
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
                RealWorldObject _obj = RealWorldObject.SpawnWorldObject(new Vector3(PlayerPrefs.GetFloat($"SaveObjectPosX{i}"), 0, PlayerPrefs.GetFloat($"SaveObjectPosY{i}")), new WorldObject { woso = WosoArray.Instance.SearchWOSOList(PlayerPrefs.GetString($"SaveObjectType{i}")) }, true, PlayerPrefs.GetFloat($"SaveObjectUses{i}"));
                _obj.GetComponent<HealthManager>().currentHealth = PlayerPrefs.GetFloat($"SaveObjectHealth{i}");
                if (_obj.GetComponent<HealthManager>().currentHealth == 0)
                {
                    _obj.GetComponent<HealthManager>().currentHealth = _obj.woso.maxHealth;
                }
                _obj.transform.localScale = new Vector3(1, 1, 1);
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
                            _obj.inventory.GetItemList()[index] = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveObjectItemType{i}{index}")), ammo = PlayerPrefs.GetInt($"SaveObjectItemAmmo{i}{index}"), amount = PlayerPrefs.GetInt($"SaveObjectItemAmount{i}{index}") , uses = PlayerPrefs.GetInt($"SaveObjectItemUses{i}{index}"), equipType = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveObjectItemType{i}{index}")).equipType };
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

            ParasiteFactionManager.parasiteData = parasiteSave;//omg u can just... do this???!?!!!

            if (ParasiteFactionManager.parasiteData.checkingPlayerLocation)
            {
                StartCoroutine(ParasiteFactionManager.Instance.CheckPlayerLocation());
            }

            ParasiteFactionManager.Instance.LoadParasiteData();
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

        for (int i = 0; i < MobManager.Instance.transform.childCount; i++)
        {
            var child = MobManager.Instance.transform.GetChild(i);
            RealMob _mob = child.GetComponent<RealMob>();
            _mob.mobSaveData.mobLocation = _mob.transform.position;
            mobSaveList.Add(_mob.mobSaveData);
        }

        var mobListJson = JsonConvert.SerializeObject(mobSaveList, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(worldMobsFileName, string.Empty);
        File.WriteAllText(worldMobsFileName, mobListJson);

        var tileJson = JsonConvert.SerializeObject(tileDataList, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(worldSaveFileName, string.Empty);
        File.WriteAllText(worldSaveFileName, tileJson);

        var worldSeed = JsonConvert.SerializeObject(world.worldSeed, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

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

            world.SetTileDataDictionary(tileListJson);

            var mobSaveJson = File.ReadAllText(worldMobsFileName);
            var mobListJson = JsonConvert.DeserializeObject<List<MobSaveData>>(mobSaveJson);

            foreach(MobSaveData _mob in mobListJson)
            {
                var realMob = RealMob.SpawnMob(_mob.mobLocation, new Mob { mobSO = MobObjArray.Instance.SearchMobList(_mob.mobType) });
            }
            //var worldSaveJson = File.ReadAllText(worldSaveFileName);
            //var dictionaryJson = JsonConvert.DeserializeObject<Dictionary<Vector2, GameObject>>(worldSaveJson);//in future make a new class, we save that class and then load its dictionary, perlin noise seed, ETC!
            //world.tileDictionary = dictionaryJson;

            var worldSeedJson = File.ReadAllText(worldSeedFileName);
            var worldSeed = JsonConvert.DeserializeObject<WorldSaveData>(worldSeedJson);
            world.worldSeed = worldSeed;
            world.LoadWorld();
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
        PlayerPrefs.SetInt("SaveDayType", (int)dayCycle.dayType);
    }

    private void LoadTime()
    {
        if (PlayerPrefs.HasKey("SaveCurrentTime"))
        {
        dayCycle.LoadNewTime(PlayerPrefs.GetInt("SaveCurrentTime"), PlayerPrefs.GetInt("SaveCurrentDay"), PlayerPrefs.GetInt("SaveCurrentDayOfYear"), PlayerPrefs.GetInt("SaveCurrentYear"), PlayerPrefs.GetInt("SaveCurrentSeason"), PlayerPrefs.GetInt("SaveSeasonProgress"), PlayerPrefs.GetInt("SaveDayType"));
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

    private void SaveJournal()
    {
        var journalJson = JsonConvert.SerializeObject(JournalNoteController.Instance.existingEntries, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
        File.WriteAllText(journalSaveFileName, string.Empty);
        File.WriteAllText(journalSaveFileName, journalJson);
    }

    private void LoadJournal()
    {
        if (File.Exists(journalSaveFileName))
        {
            var journalJson = File.ReadAllText(journalSaveFileName);
            var journalSave = JsonConvert.DeserializeObject<List<JournalEntry>>(journalJson);
            List<JournalEntry> newList = journalSave;
            JournalNoteController.Instance.LoadEntries(newList);
        }
    }
}
