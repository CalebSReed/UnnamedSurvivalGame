using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public GameObject minigame;

    void Start()
    {
        minigame = GameObject.FindGameObjectWithTag("Bellow");
        minigame.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            Load();
        }
    }

    private void Save()
    {
        Vector3 playerPos = player.transform.position;
        PlayerPrefs.SetFloat("playerPosX", playerPos.x);
        PlayerPrefs.SetFloat("playerPosY", playerPos.y);
        PlayerPrefs.SetInt("playerHealth", player.GetComponent<PlayerMain>().currentHealth);
        PlayerPrefs.SetInt("playerHunger", player.GetComponent<HungerManager>().currentHunger);
        SavePlayerInventory();
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
            LoadPlayerInventory();
            Debug.Log($"LOADED POSITION: {playerPos}");
        }
        else
        {
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
            PlayerPrefs.SetInt($"SaveItemUses{i}", player.GetComponent<PlayerMain>().inventory.GetItemList()[i].uses);
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
            PlayerPrefs.SetInt($"SaveHandItemType", 0);
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
            //player.GetComponent<PlayerMain>().inventory.SimpleAddItem(new Item { itemSO = PlayerPrefs.GetInt($"SaveItemType{i}"), amount = PlayerPrefs.GetInt($"SaveItemAmount{i}"), uses = PlayerPrefs.GetInt($"SaveItemUses{i}"), ammo = PlayerPrefs.GetInt($"SaveItemAmmo{i}") });
            i++;//have list in separate script to drag n drop all SOs then have func where it searches for given string of so itemtype then return the SO itemtype string of that index hopefully this should work!
        }
        if (PlayerPrefs.GetInt($"SaveHandItemType") != 0)
        {
            //player.GetComponent<PlayerMain>().handSlot.SetItem(new Item { itemType = (Item.ItemType)PlayerPrefs.GetInt($"SaveHandItemType"), amount = PlayerPrefs.GetInt($"SaveHandItemAmount"), uses = PlayerPrefs.GetInt($"SaveHandItemUses"), ammo = PlayerPrefs.GetInt($"SaveHandItemAmmo") }, PlayerPrefs.GetInt($"SaveHandItemUses"));
            player.GetComponent<PlayerMain>().EquipItem(player.GetComponent<PlayerMain>().handSlot.item);
        }    
        else
        {

        }
        player.GetComponent<PlayerMain>().uiInventory.RefreshInventoryItems();
    }
}
