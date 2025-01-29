using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private PlayerMain player;

    public Item handItem;
    public Item headItem;
    public Item chestItem;
    public Item legsItem;
    public Item feetItem;

    public SpriteRenderer headSpr;
    public SpriteRenderer headSideSpr;
    public SpriteRenderer headBackSpr;
    public SpriteRenderer chestSpr;
    public SpriteRenderer chestSideSpr;
    public SpriteRenderer chestBackSpr;
    public SpriteRenderer legSpr;
    public SpriteRenderer footSpr;

    public Light handLight;
    public Light headLight;

    private Coroutine handRoutine;
    private Coroutine headRoutine;
    private Coroutine chestRoutine;
    private Coroutine legsRoutine;
    private Coroutine feetRoutine;

    private bool handDecreasingDurability;
    private bool headDecreasingDurability;
    private bool chestDecreasingDurability;
    private bool legsDecreasingDurability;
    private bool feetDecreasingDurability;

    public void SetItem(Item _item)
    {
        UpdateSprite(_item.itemSO.itemSprite, _item.equipType);

        //itemSpr.color = new Color(1f, 1f, 1f, 1f);
        switch (_item.equipType)
        {
            case Item.EquipType.HandGear:
                handItem = _item;
                player.handSlot.SetItem(_item);
                break;
            case Item.EquipType.HeadGear:
                headItem = _item;
                player.headSlot.SetItem(_item);
                break;
            case Item.EquipType.ChestGear:
                chestItem = _item;
                player.chestSlot.SetItem(_item);
                break;
            case Item.EquipType.LegGear:
                legsItem = _item;
                player.leggingsSlot.SetItem(_item);
                break;
            case Item.EquipType.FootGear:
                feetItem = _item;
                player.feetSlot.SetItem(_item);
                break;
        }



        UpdateDurability(_item);

        if (_item.itemSO.doActionType == Action.ActionType.Till)
        {
            player.StateMachine.ChangeState(player.tillingState);
        }

        if (_item.itemSO.doActionType == Action.ActionType.Burn || _item.itemSO.insulationValue > 0 || _item.itemSO.rainProtectionValue > 0)
        {
            if (!handDecreasingDurability && _item.equipType == Item.EquipType.HandGear)
            {
                handRoutine = StartCoroutine(DecreaseItemUsesOverTime(_item.equipType));
                handDecreasingDurability = true;
            }
            else if (!headDecreasingDurability && _item.equipType == Item.EquipType.HeadGear)
            {
                headRoutine = StartCoroutine(DecreaseItemUsesOverTime(_item.equipType));
                headDecreasingDurability = true;
            }
            else if (!chestDecreasingDurability && _item.equipType == Item.EquipType.ChestGear)
            {
                chestRoutine = StartCoroutine(DecreaseItemUsesOverTime(_item.equipType));
                chestDecreasingDurability = true;
            }
            else if (!legsDecreasingDurability && _item.equipType == Item.EquipType.LegGear)
            {
                legsRoutine = StartCoroutine(DecreaseItemUsesOverTime(_item.equipType));
                legsDecreasingDurability = true;
            }
            else if (!feetDecreasingDurability && _item.equipType == Item.EquipType.FootGear)
            {
                feetRoutine = StartCoroutine(DecreaseItemUsesOverTime(_item.equipType));
                feetDecreasingDurability = true;
            }
        }
        else
        {
            if (handDecreasingDurability && _item.equipType == Item.EquipType.HandGear)
            {
                StopCoroutine(handRoutine);
                handDecreasingDurability = false;
            }
            else if (headDecreasingDurability && _item.equipType == Item.EquipType.HeadGear)
            {
                StopCoroutine(headRoutine);
                headDecreasingDurability = false;
            }
            else if (chestDecreasingDurability && _item.equipType == Item.EquipType.ChestGear)
            {
                StopCoroutine(chestRoutine);
                chestDecreasingDurability = false;
            }
            else if (legsDecreasingDurability && _item.equipType == Item.EquipType.LegGear)
            {
                StopCoroutine(legsRoutine);
                legsDecreasingDurability = false;
            }
            else if (feetDecreasingDurability && _item.equipType == Item.EquipType.FootGear)
            {
                StopCoroutine(feetRoutine);
                feetDecreasingDurability = false;
            }
        }
    }

    public void UpdateSprite(Sprite spr, Item.EquipType equipType)
    {
        switch (equipType)
        {
            case Item.EquipType.HeadGear:
                headSpr.sprite = spr;
                headSideSpr.sprite = spr;
                headBackSpr.sprite = spr;
                break;
            case Item.EquipType.ChestGear:
                chestSpr.sprite = spr;
                chestSideSpr.sprite = spr;
                chestBackSpr.sprite = spr;
                break;
            case Item.EquipType.LegGear:
                legSpr.sprite = spr;
                break;
            case Item.EquipType.FootGear:
                footSpr.sprite = spr;
                break;
        }
    }

    public void UpdateDurability(Item item)
    {
        if (item.uses <= 0)
        {
            BreakItem(item);
            player.inventory.RefreshEmptySlots();
            return;
        }

        int newDurability;
        if (item.amount > 1 && item.itemSO.maxUses <= 1)
        {
            newDurability = item.amount;
        }
        else
        {
            newDurability = Mathf.RoundToInt((float)item.uses / item.itemSO.maxUses * 100);//100 / 200 = .5 * 100 = 50%
        }

        switch (item.equipType)
        {
            case Item.EquipType.HandGear:
                player.handSlot.itemDataText.SetText($"{newDurability}%");
                break;
            case Item.EquipType.HeadGear:
                player.headSlot.itemDataText.SetText($"{newDurability}%");
                break;
            case Item.EquipType.ChestGear:
                player.chestSlot.itemDataText.SetText($"{newDurability}%");
                break;
            case Item.EquipType.LegGear:
                player.leggingsSlot.itemDataText.SetText($"{newDurability}%");
                break;

            case Item.EquipType.FootGear:
                player.feetSlot.itemDataText.SetText($"{newDurability}%");
                break;
        }

        player.inventory.RefreshInventory();
    }

    private IEnumerator DecreaseItemUsesOverTime(Item.EquipType equipType)//fix! set coroutines!
    {
        Item currentItem = null;
        switch (equipType)
        {
            case Item.EquipType.HandGear:
                currentItem = handItem;
                break;
            case Item.EquipType.HeadGear:
                currentItem = headItem;
                break;
            case Item.EquipType.ChestGear:
                currentItem = chestItem;
                break;
            case Item.EquipType.LegGear:
                currentItem = legsItem;
                break;
            case Item.EquipType.FootGear:
                currentItem = feetItem;
                break;
        }
        if (currentItem != null)
        {
            if (currentItem.itemSO.doActionType == Action.ActionType.Burn)
            {
                if (currentItem.itemSO.equipType == Item.EquipType.HandGear)
                {
                    handLight.intensity = 100;
                }
                else
                {
                    headLight.intensity = 100;
                }
            }

            if (WeatherManager.Instance.isRaining)
            {
                currentItem.uses -= 2;
            }
            else
            {
                currentItem.uses--;
            }
            UpdateDurability(currentItem);
        }
        else
        {
            Debug.Log("Equipped item is null cant decrease uses!");
        }
        yield return new WaitForSeconds(1);

        switch (currentItem.equipType)
        {
            case Item.EquipType.HandGear:
                handRoutine = StartCoroutine(DecreaseItemUsesOverTime(currentItem.equipType));
                handDecreasingDurability = true;
                break;
            case Item.EquipType.HeadGear:
                headRoutine = StartCoroutine(DecreaseItemUsesOverTime(currentItem.equipType));
                headDecreasingDurability = true;
                break;
            case Item.EquipType.ChestGear:
                chestRoutine = StartCoroutine(DecreaseItemUsesOverTime(currentItem.equipType));
                chestDecreasingDurability = true;
                break;
            case Item.EquipType.LegGear:
                legsRoutine = StartCoroutine(DecreaseItemUsesOverTime(currentItem.equipType));
                legsDecreasingDurability = true;
                break;
            case Item.EquipType.FootGear:
                feetRoutine = StartCoroutine(DecreaseItemUsesOverTime(currentItem.equipType));
                feetDecreasingDurability = true;
                break;
        }
    }

    public void TurnOffLight(Item item)
    {
        switch (item.equipType)
        {
            case Item.EquipType.HandGear:
                handLight.intensity = 0;
                break;
            case Item.EquipType.HeadGear:
                headLight.intensity = 0;
                break;
        }
    }

    public Item ReturnItemByEquipType(Item.EquipType equipType)
    {
        switch (equipType)
        {
            case Item.EquipType.HandGear:
                return handItem;
            case Item.EquipType.HeadGear:
                return headItem;
            case Item.EquipType.ChestGear:
                return chestItem;
            case Item.EquipType.LegGear:
                return legsItem;
            case Item.EquipType.FootGear:
                return feetItem;
        }
        return null;
    }

    public void RemoveItem(Item item)
    {
        if (item.equipType == Item.EquipType.HandGear && item.itemSO.doActionType == Action.ActionType.Till || item.equipType == Item.EquipType.HandGear && item.itemSO.doActionType == Action.ActionType.Shoot || item.equipType == Item.EquipType.HandGear && item.itemSO.doActionType == Action.ActionType.Throw)
        {
            player.StateMachine.ChangeState(player.defaultState);
        }


        UpdateSlotBool(false, item.equipType);

        switch (item.equipType)
        {
            case Item.EquipType.HandGear:
                handItem = null;
                player.handSlot.RemoveItem();
                break;
            case Item.EquipType.HeadGear:
                headItem = null;
                player.headSlot.RemoveItem();
                break;
            case Item.EquipType.ChestGear:
                chestItem = null;
                player.chestSlot.RemoveItem();
                break;
            case Item.EquipType.LegGear:
                legsItem = null;
                player.leggingsSlot.RemoveItem();
                break;
            case Item.EquipType.FootGear:
                feetItem = null;
                player.feetSlot.RemoveItem();
                break;
        }

        if (handDecreasingDurability && item.equipType == Item.EquipType.HandGear)
        {
            StopCoroutine(handRoutine);
            handDecreasingDurability = false;
        }
        else if (headDecreasingDurability && item.equipType == Item.EquipType.HeadGear)
        {
            StopCoroutine(headRoutine);
            headDecreasingDurability = false;
        }
        else if (chestDecreasingDurability && item.equipType == Item.EquipType.ChestGear)
        {
            StopCoroutine(chestRoutine);
            chestDecreasingDurability = false;
        }
        else if (legsDecreasingDurability && item.equipType == Item.EquipType.LegGear)
        {
            StopCoroutine(legsRoutine);
            legsDecreasingDurability = false;
        }
        else if (feetDecreasingDurability && item.equipType == Item.EquipType.FootGear)
        {
            StopCoroutine(feetRoutine);
            feetDecreasingDurability = false;
        }
    }

    public Item.EquipType ReturnNullItem()
    {
        if (handItem == null)
        {
            return Item.EquipType.HandGear;
        }
        else if (headItem == null)
        {
            return Item.EquipType.HeadGear;
        }
        else if (chestItem == null)
        {
            return Item.EquipType.ChestGear;
        }
        else if (legsItem == null)
        {
            return Item.EquipType.LegGear;
        }
        else if (feetItem == null)
        {
            return Item.EquipType.FootGear;
        }
        else
        {
            Debug.LogError("An item was presumably null, but we're incorrect!");
            return 0;
        }
    }

    private void BreakItem(Item item)
    {
        Debug.Log("Equipped item broke");
        if (item.itemSO.needsAmmo)
        {
            player.inventory.AddItem(new Item { itemSO = item.itemSO.validAmmo, amount = item.ammo }, player.transform.position, false);
        }
        player.UnequipItem(item.equipType, false);
    }

    public bool ItemAlreadyEquipped(Item item)
    {
        if (item == handItem)
        {
            return true;
        }
        else if (item == headItem)
        {
            return true;
        }
        else if (item == chestItem)
        {
            return true;
        }
        else if (item == legsItem)
        {
            return true;
        }
        else if (item == feetItem)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool SlotAlreadyFull(Item.EquipType equipType)
    {
        switch (equipType)
        {
            case Item.EquipType.HandGear:
                if (handItem != null)
                {
                    return true;
                }
                break;
            case Item.EquipType.HeadGear:
                if (headItem != null)
                {
                    return true;
                }
                break;
            case Item.EquipType.ChestGear:
                if (chestItem != null)
                {
                    return true;
                }
                break;
            case Item.EquipType.LegGear:
                if (legsItem != null)
                {
                    return true;
                }
                break;
            case Item.EquipType.FootGear:
                if (feetItem != null)
                {
                    return true;
                }
                break;
        }
        return false;
    }

    public void UpdateSlotBool(bool isEquipped, Item.EquipType equipType)
    {
        switch (equipType)
        {
            case Item.EquipType.HandGear:
                player.isHandItemEquipped = isEquipped;
                break;
            case Item.EquipType.HeadGear:
                player.isHeadItemEquipped = isEquipped;
                break;
            case Item.EquipType.ChestGear:
                player.isChestItemEquipped = isEquipped;
                break;
            case Item.EquipType.LegGear:
                player.isLeggingItemEquipped = isEquipped;
                break;
            case Item.EquipType.FootGear:
                player.isFootItemEquipped = isEquipped;
                break;
        }
    }
}
