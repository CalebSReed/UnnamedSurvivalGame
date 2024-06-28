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
using TMPro;

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
    public TextMeshProUGUI dayCountTxt;
    public LayerMask tileMask;
    public enum DifficultyOptions
    {
        forgiving,
        normal,
        hardcore,
    }

    public DifficultyOptions difficulty;

    public List<string> objTypeArray;//change name to list not array bro
    public List<Vector3> objTransformArray;
    public List<float> objUsesArray;
    public List<float> objHealthArray;
    public List<bool> attachmentArray;
    public List<int> attachmentIndexArray;

    private PlayerSaveData playerSave = new PlayerSaveData();
    private List<WorldObjectData> worldObjectDataList = new List<WorldObjectData>();

    public bool isLoading;

    private string playerInfoSaveFileName;
    public string worldSaveFileName;
    public string itemsSaveFileName;
    public string objectsSaveFileName;
    public string naturalObjectsSaveFileName;
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
    [SerializeField] GameObject gameUI;

    void Start()
    {
        Instance = this;
        if (!Directory.Exists(Application.persistentDataPath + "/SaveFiles"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/SaveFiles");
        }
        playerInfoSaveFileName = Application.persistentDataPath + "/SaveFiles/PlayerInfo.json";
        worldSaveFileName = Application.persistentDataPath + "/SaveFiles/WorldSave.json";
        itemsSaveFileName = Application.persistentDataPath + "/SaveFiles/ItemsSave.json";
        objectsSaveFileName = Application.persistentDataPath + "/SaveFiles/ObjectsSave.json";
        naturalObjectsSaveFileName = Application.persistentDataPath + "/SaveFiles/NaturalObjectsSave.json";
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
            worldSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/WorldSave.json";
            itemsSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/ItemsSave.json";
            objectsSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/ObjectsSave.json";
            naturalObjectsSaveFileName = Application.persistentDataPath + "/SaveFiles/EDITORSAVES/NaturalObjectsSave.json";
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
        yield return new WaitForSeconds(1);
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
                SceneManager.LoadScene(1);
            }
        }
    }

    public void ToggleGodMode(InputAction.CallbackContext context)
    {
        if (context.performed && Application.isEditor)
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

    public void ToggleGodMode(bool isForced = false)
    {
        if (!Application.isEditor && !isForced)
        {
            return;
        }
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
        if (!Application.isEditor)
        {
            return;
        }
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

    public void ToggleSpeedMode(bool forced = false)
    {
        if (!Application.isEditor && !forced)
        {
            return;
        }
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
        if (!context.performed || !Application.isEditor)
        {
            return;
        }

        ToggleFreeCrafting();
    }

    public void ToggleFreeCrafting()
    {
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
            gameUI.SetActive(!gameUI.activeSelf);
            return;

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
            if (difficulty == DifficultyOptions.hardcore)
            {
                return;
            }

            player.transform.position = playerHome;
            playerMain.hpManager.SetCurrentHealth(playerMain.maxHealth/4);
            playerMain.hungerManager.SetHunger(playerMain.maxHunger/4);
            playerMain.healthBar.SetHealth(playerMain.hpManager.currentHealth);
            playerMain.StateMachine.ChangeState(playerMain.defaultState, true);
            playerMain.GetComponent<TemperatureReceiver>().ResetTemperature();

            if (playerMain.speedRoutine != null)
            {
                StopCoroutine(playerMain.speedRoutine);
            }
            playerMain.speedMult = 1;
            playerMain.playerAnimator.Rebind();
            playerMain.playerAnimator.Update(0f);
            playerMain.playerAnimator.Play("Front_Idle", 0, 0f);

            playerMain.playerSideAnimator.Rebind();
            playerMain.playerSideAnimator.Update(0f);
            playerMain.playerSideAnimator.Play("Side_Idle", 0, 0f);

            playerMain.playerBackAnimator.Rebind();
            playerMain.playerBackAnimator.Update(0f);
            playerMain.playerBackAnimator.Play("Side_Idle", 0, 0f);
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
            dayCountTxt.text = $"Day {DayNightCycle.Instance.currentDay}";
            musicPlayer.audio.Pause("Music1");
            musicPlayer.audio.Pause("Music2");
            musicPlayer.audio.Pause("Music3");
            musicPlayer.audio.Pause("Music4");
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
            musicPlayer.audio.UnPause("Music4");
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

    private void OnApplicationQuit()
    {
        if (difficulty == DifficultyOptions.hardcore && playerMain.StateMachine.currentPlayerState != playerMain.deadState)
        {
            Save();
        }
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
        SaveParasiteData();
        SaveWorld();
        SaveObjects();
        SaveTime();
        SaveWeather();
        SaveJournal();
        RecipeSaveController.Instance.SaveRecipes();
        Announcer.SetText("SAVED");
        PlayerPrefs.Save();

        Debug.Log($"SAVED POSITION: {playerSave.playerPos}");
    }

    public void TryToLoad()//called from button
    {
        if (difficulty == DifficultyOptions.hardcore)
        {
            Announcer.SetText("CANNOT LOAD ON HARDCORE!", Color.red);
            return;
        }
        Load();
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

            playerMain.enemyList.Clear();

            player.GetComponent<PlayerMain>().hpManager.currentHealth = playerSave.health;
            player.GetComponent<PlayerMain>().healthBar.SetHealth(playerSave.health);
            player.GetComponent<HungerManager>().currentHunger = playerSave.hunger;

            player.transform.position = playerSave.playerPos;

            playerMain.cellPosition = new int[] { Mathf.RoundToInt(player.transform.position.x / 25), Mathf.RoundToInt(player.transform.position.z / 25) };

            //player.gameObject.GetComponent<PlayerController>().ChangeTarget(playerPos);
            LoadPlayerInventory();
            LoadPlayerPlacedItems();
            LoadObjects();
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
        playerSave.playerInvContainedTypes.Clear();
        player.GetComponent<PlayerMain>().StopHoldingItem();
        Inventory playerInv = player.GetComponent<PlayerMain>().inventory;
        PlayerMain main = player.GetComponent<PlayerMain>();

        main.uiInventory.CloseContainers();

        playerSave.difficulty = (int)difficulty;

        if (playerMain.mobRide != null)
        {
            playerSave.mobRide = playerMain.mobRide;
        }
        else
        {
            playerSave.mobRide = null;
        }

        for (int i = 0; i < playerInv.GetItemList().Length; i++)
        {
            if (playerInv.GetItemList()[i] != null)
            {
                playerSave.playerInvTypes.Add(i, playerInv.GetItemList()[i].itemSO.itemType);
                playerSave.playerInvAmounts.Add(i, playerInv.GetItemList()[i].amount);
                playerSave.playerInvDurabilities.Add(i, playerInv.GetItemList()[i].uses);
                playerSave.playerInvAmmo.Add(i, playerInv.GetItemList()[i].ammo);

                if (playerInv.GetItemList()[i].containedItems != null && playerInv.GetItemList()[i].containedItems.Length > 0)
                {
                    string[] containedTypes = new string[playerInv.GetItemList()[i].containedItems.Length];

                    for (int j = 0; j < playerInv.GetItemList()[i].containedItems.Length; j++)
                    {
                        if (playerInv.GetItemList()[i].containedItems[j] != null)
                        {
                            containedTypes[j] = (playerInv.GetItemList()[i].containedItems[j].itemSO.itemType);
                        }
                    }
                    containedTypes.Reverse();
                    playerSave.playerInvContainedTypes.Add(i, containedTypes);
                }
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
        }
        else
        {
            playerSave.feetItemType = "Null";
        }

        var playerSaveJson = JsonConvert.SerializeObject(playerSave, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(playerInfoSaveFileName, string.Empty);
        File.WriteAllText(playerInfoSaveFileName, playerSaveJson);
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

            difficulty = (DifficultyOptions)playerJsonSave.difficulty;

            if (playerMain.mobRide != null)
            {
                playerMain.UnrideCreature();
            }

            if (playerJsonSave.mobRide != null)
            {
                playerMain.mobRide = playerJsonSave.mobRide;
                var mob = RealMob.SpawnMob(transform.position, new Mob { mobSO = MobObjArray.Instance.SearchMobList(playerJsonSave.mobRide.mobType) });
                mob.mobSaveData = playerMain.mobRide;
                playerMain.RideCreature(mob);
            }

            while (i < player.GetComponent<PlayerMain>().inventory.GetItemList().Length)//each item in inventory
            {
                if (playerJsonSave.playerInvTypes.ContainsKey(i))
                {
                    playerJsonSave.playerInvTypes.TryGetValue(i, out string itemType);
                    playerJsonSave.playerInvAmounts.TryGetValue(i, out int itemAmount);
                    playerJsonSave.playerInvDurabilities.TryGetValue(i, out int itemUses);
                    playerJsonSave.playerInvAmmo.TryGetValue(i, out int itemAmmo);
                    playerJsonSave.playerInvContainedTypes.TryGetValue(i, out string[] containedTypes);
                    
                    if (itemType == "Null")
                    {
                        //do nothing
                    }
                    else
                    {
                        player.GetComponent<PlayerMain>().inventory.GetItemList()[i] = new Item
                        {
                            itemSO = ItemObjectArray.Instance.SearchItemList(itemType),
                            amount = itemAmount,
                            uses = itemUses,
                            ammo = itemAmmo,
                            equipType = ItemObjectArray.Instance.SearchItemList(itemType).equipType,
                            containedItems = new Item[ItemObjectArray.Instance.SearchItemList(itemType).maxStorageSpace]                            
                        };

                        if (containedTypes != null && containedTypes.Length > 0)
                        {
                            for (int j = 0; j < containedTypes.Length; j++)
                            {
                                if (containedTypes[j] != null)
                                {
                                    player.GetComponent<PlayerMain>().inventory.GetItemList()[i].containedItems[j] = new Item
                                    {
                                        itemSO = ItemObjectArray.Instance.SearchItemList(containedTypes[j]),
                                        amount = 1,
                                    };
                                }
                            }
                        }
                    }
                }
                i++;
            }

            if (playerJsonSave.handItemType != "Null")
            {
                //dont load hand item anymore since they share itemslots
                //player.GetComponent<PlayerMain>().EquipItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(playerJsonSave.handItemType), amount = playerJsonSave.handItemAmount, uses = playerJsonSave.handItemUses, ammo = playerJsonSave.handItemAmmo, equipType = ItemObjectArray.Instance.SearchItemList(playerJsonSave.handItemType).equipType }, playerMain.handSlot);
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
        var itemList = new List<ItemsSaveData>();
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Item"))
        {
            if (!_obj.transform.parent)//if we ever give them a parent... CHANGE THIS!
            {
                _obj.GetComponent<RealItem>().Save();
                itemList.Add(_obj.GetComponent<RealItem>().saveData);
            }
        }

        var itemSaveJson = JsonConvert.SerializeObject(itemList, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(itemsSaveFileName, string.Empty);
        File.WriteAllText(itemsSaveFileName, itemSaveJson);
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

        if (File.Exists(itemsSaveFileName))
        {
            var itemSavesJson = File.ReadAllText(itemsSaveFileName);
            var itemSaves = JsonConvert.DeserializeObject<List<ItemsSaveData>>(itemSavesJson);

            foreach (var save in itemSaves)
            {
                if (save.itemType != "NULL")
                {
                    var item = RealItem.SpawnRealItem(save.pos, new Item 
                    { 
                        itemSO = ItemObjectArray.Instance.SearchItemList(save.itemType),
                        ammo = save.ammo,
                        amount = save.amount,
                        uses = save.uses
                    }, 
                    true, true, save.ammo);

                    if (item.item.itemSO.canStoreItems)
                    {
                        item.item.containedItems = new Item[item.item.itemSO.maxStorageSpace];
                    }

                    if (save.containedTypes != null)
                    {
                        Item[] containedTypes = new Item[save.containedTypes.Length];
                        for (int i = 0; i < save.containedTypes.Length; i++)
                        {
                            if (save.containedTypes[i] != null)
                            {
                                containedTypes[i] = new Item
                                {
                                    itemSO = ItemObjectArray.Instance.SearchItemList(save.containedTypes[i]),
                                    amount = 1
                                };
                            }
                        }
                        item.item.containedItems = containedTypes;
                    }
                }
                else
                {
                    Debug.LogError("Skipped null item during load!");
                    //in future wipe all NULL items in save file?
                }
            }
        }
        else
        {
            Debug.LogError("No items save found!");
        }

    }

    private void SaveObjects()
    {
        worldObjectDataList.Clear();
        foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("WorldObject"))
        {
            if (_obj.GetComponent<RealWorldObject>() != null && _obj.GetComponent<RealWorldObject>().woso.isPlayerMade || _obj.GetComponent<RealWorldObject>() != null && _obj.GetComponent<RealWorldObject>().woso.isParasiteMade)
            {
                if (_obj.GetComponent<DoorBehavior>() != null && _obj.GetComponent<DoorBehavior>().isOpen)
                {
                    _obj.GetComponent<DoorBehavior>().ToggleOpen();
                }

                RealWorldObject _realObj = _obj.GetComponent<RealWorldObject>();
                _realObj.SaveData();
                worldObjectDataList.Add(_realObj.saveData);
            }
        }

        var naturalObjList = world.naturalObjectSaveList;

        foreach (GameObject tile in world.TileObjList)
        {
            for (int i = 0; i < tile.transform.childCount; i++)
            {
                if (tile.transform.GetChild(i).GetComponent<RealWorldObject>() != null)
                {
                    tile.transform.GetChild(i).GetComponent<RealWorldObject>().SaveData();

                    //naturalObjList.Add(tile.transform.GetChild(i).GetComponent<RealWorldObject>().saveData);
                }
            }
        }

        var objectSaveJson = JsonConvert.SerializeObject(worldObjectDataList, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
        File.WriteAllText(objectsSaveFileName, string.Empty);
        File.WriteAllText(objectsSaveFileName, objectSaveJson);

        var naturalObjJson = JsonConvert.SerializeObject(naturalObjList, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(naturalObjectsSaveFileName, string.Empty);
        File.WriteAllText(naturalObjectsSaveFileName, naturalObjJson);
    }

    private void LoadObjects()
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

        if (File.Exists(objectsSaveFileName))
        {
            var objSavesJson = File.ReadAllText(objectsSaveFileName);
            var objSaves = JsonConvert.DeserializeObject<List<WorldObjectData>>(objSavesJson);

            foreach (var saveObj in objSaves)
            {
                var obj = RealWorldObject.SpawnWorldObject(saveObj.pos, new WorldObject { woso = WosoArray.Instance.SearchWOSOList(saveObj.objType)}, true);
                obj.LoadData(saveObj);
            }
        }
        else
        {
            Debug.LogError("No Objects Save Found!");
        }

        foreach (var tile in world.TileObjList)
        {
            for (int i = 0; i < tile.transform.childCount; i++)
            {
                if (tile.transform.GetChild(i).GetComponent<RealWorldObject>() != null)
                {
                    Destroy(tile.transform.GetChild(i).gameObject);
                }
            }
        }

        if (File.Exists(naturalObjectsSaveFileName))
        {
            var converter = new JsonVec2Converter();
            var naturalObjJson = File.ReadAllText(naturalObjectsSaveFileName);
            var naturalObjSaves = JsonConvert.DeserializeObject<List<WorldObjectData>>(naturalObjJson, converter);
            world.GenerateNewNaturalObjDict(naturalObjSaves);
        }
        else
        {
            Debug.LogError("No Natural Objects Save Found!");
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
        List<TileData> tileDataList = new List<TileData>();
        foreach (var tile in world.TileDataList)
        {
            tileDataList.Add(tile);
        }
        Debug.Log(tileDataList.Count);
        List<MobSaveData> mobSaveList = new List<MobSaveData>();

        for (int i = 0; i < MobManager.Instance.transform.childCount; i++)
        {
            var child = MobManager.Instance.transform.GetChild(i);
            RealMob _mob = child.GetComponent<RealMob>();
            _mob.SaveData();
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
            world.existingTileDictionary.Clear();
            //var gos = GameObject.FindGameObjectsWithTag("Tile");
            foreach (var _obj in world.TileObjList)//need to search this list because we cant grab disabled objs without references + we never delete tiles mid-game
            {
                Debug.Log("Destroyed " + _obj);
                Destroy(_obj);
            }

            world.TileObjList.Clear();

            for (int i = 0; i < MobManager.Instance.transform.childCount; i++)
            {
                var mob = MobManager.Instance.transform.GetChild(i);
                Destroy(mob.gameObject);
            }

            world.mobList.Clear();
            world.TileDataList.Clear();

            //UnityEngine.Random.state
            var worldSaveJson = File.ReadAllText(worldSaveFileName);
            var tileListJson = JsonConvert.DeserializeObject<List<TileData>>(worldSaveJson);

            world.SetTileDataDictionary(tileListJson);

            var mobSaveJson = File.ReadAllText(worldMobsFileName);
            var mobListJson = JsonConvert.DeserializeObject<List<MobSaveData>>(mobSaveJson);

            foreach(MobSaveData _mob in mobListJson)
            {
                var realMob = RealMob.SpawnMob(_mob.mobLocation, new Mob { mobSO = MobObjArray.Instance.SearchMobList(_mob.mobType) });
                realMob.GetComponent<HealthManager>().currentHealth = _mob.currentHealth;
                realMob.mobSaveData = _mob;
                if (_mob.isRidable)
                {
                    realMob.GetComponent<Ridable>().GetSaddled(ItemObjectArray.Instance.SearchItemList(_mob.saddle));
                }
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
        DayNightCycle.Instance.SaveTime();
        var timeSave = JsonConvert.SerializeObject(DayNightCycle.Instance.saveData, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(timeSaveFileName, string.Empty);
        File.WriteAllText(timeSaveFileName, timeSave);
    }

    private void LoadTime()
    {
        if (File.Exists(timeSaveFileName))
        {
            var timeSaveJson = File.ReadAllText(timeSaveFileName);
            var timeSave = JsonConvert.DeserializeObject<TimeSaveData>(timeSaveJson);

            dayCycle.LoadNewTime(timeSave.currentTime, timeSave.currentDay, timeSave.currentDayOfYear, timeSave.currentYear, timeSave.currentSeason, timeSave.currentSeasonProgress, timeSave.currentDayType);
        }
        else
        {
            Debug.LogError("NO TIME SAVE FOUND");
        }
    }

    private void SaveWeather()
    {
        WeatherManager.Instance.SaveWeather();
        var weatherSave = JsonConvert.SerializeObject(WeatherManager.Instance.weatherSave, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        File.WriteAllText(weatherSaveFileName, string.Empty);
        File.WriteAllText(weatherSaveFileName, weatherSave);
    }

    private void LoadWeather()
    {
        if (File.Exists(weatherSaveFileName))
        {
            var weatherSaveJson = File.ReadAllText(weatherSaveFileName);
            var weatherSave = JsonConvert.DeserializeObject<WeatherSaveData>(weatherSaveJson);

            WeatherManager.Instance.LoadWeatherData(weatherSave.rainProgress, weatherSave.thunderProgress, weatherSave.stormCooldown, weatherSave.isRaining, weatherSave.targetReached);
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
