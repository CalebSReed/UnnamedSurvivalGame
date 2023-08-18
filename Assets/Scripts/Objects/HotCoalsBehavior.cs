using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class HotCoalsBehavior : MonoBehaviour
{
    public Item item;
    public bool isCooking;
    private int currentCookingTime = 0;
    private int cookingTimeRequired = 10;//change to get roasting time from item class probs
    private Inventory inventory;

    public event EventHandler OnFinishedCooking; 

    public void StartCooking(Item _item, Inventory _inventory)
    {
        if (!isCooking)
        {
            item = new Item { itemType = _item.itemType, amount = 1 };
            currentCookingTime = 0;
            isCooking = true;
            StartCoroutine(Cook());
            inventory = _inventory;
        }
    }

    private IEnumerator Cook()
    {
        yield return new WaitForSeconds(1f);
        currentCookingTime++;
        if (currentCookingTime >= cookingTimeRequired)
        {
            int i = 0;
            foreach (Item _item in inventory.GetItemList())
            {
                if (_item.itemType == item.itemType)
                {
                    inventory.RemoveItemBySlot(i);
                    break;
                }
                i++;
            }
            item.itemType = item.GetCookingReward();
            item.amount = 1;
            Vector2 direction = new Vector2((float)Random.Range(-1000, 1000), (float)Random.Range(-1000, 1000));
            RealItem newItem = RealItem.SpawnRealItem(transform.position, item, true);
            newItem.GetComponent<Rigidbody2D>().AddForce(direction * 5f);

            item = null;
            isCooking = false;
            OnFinishedCooking?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            StartCoroutine(Cook());
        }
    }
}
