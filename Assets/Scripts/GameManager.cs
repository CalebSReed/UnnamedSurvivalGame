using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public GameObject minigame;
    public List<RealWorldObject> objList;//clone these objs??? sounds awful

    public event EventHandler onLoad;

    private bool eraseWarning = false;
    private bool resetWarning = false;

    void Start()
    {
        minigame = GameObject.FindGameObjectWithTag("Bellow");
        minigame.SetActive(false);
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
                SceneManager.LoadScene(0);
            }
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            Load();
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (!eraseWarning)
            {
                Announcer.SetText("WARNING: HIT F10 AGAIN TO CLEAR ALL SAVE DATA", Color.red);
                eraseWarning = true;
                Invoke(nameof(ResetEraseWarning), 3f);
            }
            else
            {                
                ClearAllSaveData();
            }
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
        Announcer.SetText("SAVA DATA ERASED");
        eraseWarning = false;
    }

    private void Save()//change all this to JSON at some point. That way we can do more things like have multiple save files :)
    {
        //onSave?.Invoke(this, EventArgs.Empty);
        Vector3 playerPos = player.transform.position;
        PlayerPrefs.SetFloat("playerPosX", playerPos.x);
        PlayerPrefs.SetFloat("playerPosY", playerPos.y);
        PlayerPrefs.SetInt("playerHealth", player.GetComponent<PlayerMain>().currentHealth);
        PlayerPrefs.SetInt("playerHunger", player.GetComponent<HungerManager>().currentHunger);
        SavePlayerInventory();
        SavePlayerObjects();
        Announcer.SetText("SAVED");
        PlayerPrefs.Save();

        Debug.Log($"SAVED POSITION: {playerPos}");
    }

    private void Load()
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
            Announcer.SetText("LOADED");
            Debug.Log($"LOADED POSITION: {playerPos}");
        }
        else
        {
            Announcer.SetText("ERROR: SAVE NOT FOUND");
            Debug.LogError("SAVE NOT FOUND");
        }
    }

    private void SavePlayerInventory()
    {
        int i = 0;
        foreach (Item _item in player.GetComponent<PlayerMain>().inventory.GetItemList())//each item in inventory
        {
            PlayerPrefs.SetString($"SaveItemType{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].itemSO.itemType);//ah yes I see why we need an ID system for these scriptable objects to be saved... damnit
            PlayerPrefs.SetInt($"SaveItemAmount{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].amount);//TODO implement database for objs, items, and mobs... ugh
            PlayerPrefs.SetInt($"SaveItemUses{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].uses);//hah loser i just made a public list to search for SOs. Dict and ID syst would be cool still tho....
            PlayerPrefs.SetInt($"SaveItemAmmo{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].ammo);
            i++;
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
        PlayerPrefs.SetInt("InventorySize", player.GetComponent<PlayerMain>().inventory.GetItemList().Count);
    }

    private void LoadPlayerInventory()
    {
        int i = 0;
        player.GetComponent<PlayerMain>().inventory.GetItemList().Clear();
        player.GetComponent<PlayerMain>().handSlot.RemoveItem();
        player.GetComponent<PlayerMain>().equippedHandItem = null;
        player.GetComponent<PlayerMain>().UnequipItem();//will spawn a null
        player.GetComponent<PlayerMain>().StopHoldingItem();//save held item later im lazy
        while (i < PlayerPrefs.GetInt("InventorySize"))//each item in inventory
        {
            //OH MY GOSH GOLLY THATS A LONG LINE
            player.GetComponent<PlayerMain>().inventory.SimpleAddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveItemType{i}")), amount = PlayerPrefs.GetInt($"SaveItemAmount{i}"), uses = PlayerPrefs.GetInt($"SaveItemUses{i}"), ammo = PlayerPrefs.GetInt($"SaveItemAmmo{i}") });
            i++;
//have list in separate script to drag n drop all SOs then have func where it searches for given string of so itemtype then return the SO itemtype string of that index hopefully this should work!
        }
        if (PlayerPrefs.GetString($"SaveHandItemType") != "Null")
        {
            player.GetComponent<PlayerMain>().handSlot.SetItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(PlayerPrefs.GetString($"SaveHandItemType")), amount = PlayerPrefs.GetInt($"SaveHandItemAmount"), uses = PlayerPrefs.GetInt($"SaveHandItemUses"), ammo = PlayerPrefs.GetInt($"SaveHandItemAmmo") }, PlayerPrefs.GetInt($"SaveHandItemUses"));
            player.GetComponent<PlayerMain>().EquipItem(player.GetComponent<PlayerMain>().handSlot.item);
        }    
        else
        {

        }
        player.GetComponent<PlayerMain>().uiInventory.RefreshInventoryItems();
    }

    private void SavePlayerObjects()
    {
        int i = 0;
        foreach (RealWorldObject _obj in objList)
        {
            PlayerPrefs.SetFloat($"SaveObjectPosX{i}", objList[i].transform.position.x);
            PlayerPrefs.SetFloat($"SaveObjectPosY{i}", objList[i].transform.position.y);
            PlayerPrefs.SetString($"SaveObjectType{i}", objList[i].obj.woso.objType);
            PlayerPrefs.SetFloat($"SaveObjectUses{i}", objList[i].actionsLeft);
            i++;
        }
    }

    private void LoadPlayerObjects()
    {
        if (objList.Count > 0 && PlayerPrefs.HasKey("SaveObjectPosX0"))
        {
            int i = 0;
            List<RealWorldObject> tempList = new List<RealWorldObject>(objList);
            foreach (RealWorldObject _obj in tempList)
            {
                Debug.Log($"Loading {PlayerPrefs.GetString($"SaveObjectType{i}")}");
                RealWorldObject.SpawnWorldObject(new Vector3(PlayerPrefs.GetFloat($"SaveObjectPosX{i}"), PlayerPrefs.GetFloat($"SaveObjectPosY{i}"), 0), new WorldObject { woso = WosoArray.Instance.SearchWOSOList(PlayerPrefs.GetString($"SaveObjectType{i}")) }, true, PlayerPrefs.GetFloat($"SaveObjectUses{i}"));
                i++;
            }
        }
        onLoad?.Invoke(this, EventArgs.Empty);
    }
}
