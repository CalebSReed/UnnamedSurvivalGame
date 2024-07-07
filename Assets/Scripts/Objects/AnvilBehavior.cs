using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnvilBehavior : MonoBehaviour
{
    RealWorldObject obj;
    Item storedItem;
    [SerializeField] private GameObject vfx;

    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.hasSpecialInteraction = true;
        obj.interactEvent.AddListener(OnInteract);
        obj.storedItemRenderer.transform.localPosition = new Vector3(0, 2.5f, -0.1f);

        obj.hoverBehavior.SpecialCase = true;
        obj.hoverBehavior.specialCaseModifier.AddListener(CheckItems);

        vfx = obj.vfx;

        obj.onLoaded += OnLoad;
    }

    private void OnInteract()
    {

        if (obj.playerMain.isHandItemEquipped && obj.playerMain.equippedHandItem.heldItem != null && storedItem == null)//placing tongs item on anvil
        {
            if (obj.playerMain.equippedHandItem.heldItem.itemSO.isReheatable)
            {
                storedItem = obj.playerMain.equippedHandItem.heldItem;
                obj.saveData.heldItemType = storedItem.itemSO.itemType;
                obj.playerMain.RemoveContainedItem();
                obj.playerMain.equippedHandItem.heldItem = null;
                obj.storedItemRenderer.sprite = storedItem.itemSO.itemSprite;
                if (storedItem.isHot)
                {
                    vfx.SetActive(true);
                    StartCoroutine(CheckHotness());
                }
                return;
            }
            foreach (ItemSO item in obj.woso.acceptableSmeltItems)
            {
                if (item == obj.playerMain.equippedHandItem.heldItem.itemSO)
                {
                    storedItem = obj.playerMain.equippedHandItem.heldItem;
                    obj.saveData.heldItemType = storedItem.itemSO.itemType;
                    obj.playerMain.RemoveContainedItem();
                    obj.playerMain.equippedHandItem.heldItem = null;
                    obj.storedItemRenderer.sprite = storedItem.itemSO.itemSprite;
                    if (storedItem.isHot)
                    {
                        vfx.SetActive(true);
                        StartCoroutine(CheckHotness());
                    }
                    break;
                }
            }
        }
        else if (obj.playerMain.doAction == Action.ActionType.Hammer && storedItem != null && storedItem.itemSO.actionReward.Length > 0 && storedItem.itemSO.actionReward[0] != null && storedItem.isHot)//hammer action
        {
            storedItem.itemSO = storedItem.itemSO.actionReward[0];
            storedItem.equipType = storedItem.itemSO.equipType;
            storedItem.uses = storedItem.itemSO.maxUses;
            obj.storedItemRenderer.sprite = storedItem.itemSO.itemSprite;
            obj.saveData.heldItemType = storedItem.itemSO.itemType;
            obj.playerMain.UseEquippedItemDurability();
            var rand = Random.Range(1, 4);
            AudioManager.Instance.Play($"Chop{rand}", transform.position);
        }
        else if (!obj.playerMain.hasTongs && storedItem != null && !storedItem.isHot)//empty hands
        {
            obj.playerMain.inventory.AddItem(storedItem, transform.position);
            storedItem = null;
            obj.saveData.heldItemType = null;
            obj.storedItemRenderer.sprite = null;
        }
        else if (obj.playerMain.hasTongs && obj.playerMain.equippedHandItem.heldItem == null && storedItem != null)//picking up item with tongs
        {
            obj.playerMain.equippedHandItem.heldItem = storedItem;
            obj.playerMain.UpdateContainedItem(storedItem);
            storedItem = null;
            obj.saveData.heldItemType = null;
            obj.storedItemRenderer.sprite = null;
        }
    }

    private IEnumerator CheckHotness()
    {
        yield return null;
        if (storedItem == null || !storedItem.isHot)
        {
            vfx.SetActive(false);
            yield break;
        }
        else
        {
            StartCoroutine(CheckHotness());
        }
    }

    private void CheckItems()
    {
        if (obj.playerMain.isHandItemEquipped && obj.playerMain.equippedHandItem.heldItem != null && storedItem == null)
        {
            if (obj.playerMain.equippedHandItem.heldItem.itemSO.isReheatable)
            {
                obj.hoverBehavior.Prefix = $"RMB: Place {obj.playerMain.equippedHandItem.heldItem.itemSO.itemName} on ";
                obj.hoverBehavior.Name = obj.woso.objName;
                return;
            }
            foreach (ItemSO item in obj.woso.acceptableSmeltItems)
            {
                if (item == obj.playerMain.equippedHandItem.heldItem.itemSO)
                {
                    obj.hoverBehavior.Prefix = $"RMB: Place {obj.playerMain.equippedHandItem.heldItem.itemSO.itemName} on ";
                    obj.hoverBehavior.Name = obj.woso.objName;
                    return;
                }
            }
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
        else if (obj.playerMain.doAction == Action.ActionType.Hammer && storedItem != null && storedItem.itemSO.actionReward.Length > 0 && storedItem.itemSO.actionReward[0] != null && storedItem.isHot)//hammer action
        {
            obj.hoverBehavior.Prefix = $"RMB: Hammer {storedItem}";
            obj.hoverBehavior.Name = "";
        }
        else if (!obj.playerMain.hasTongs && storedItem != null && !storedItem.isHot)//empty hands
        {
            obj.hoverBehavior.Prefix = $"RMB: Collect {storedItem}";
            obj.hoverBehavior.Name = "";
        }
        else if (obj.playerMain.hasTongs && obj.playerMain.equippedHandItem.heldItem == null && storedItem != null)//picking up item with tongs
        {
            obj.hoverBehavior.Prefix = $"RMB: Collect {storedItem}";
            obj.hoverBehavior.Name = "";
        }
        else
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
    }

    private void OnLoad(object sender, System.EventArgs e)
    {
        if (obj.saveData.heldItemType != null)
        {
            storedItem = new Item { itemSO = ItemObjectArray.Instance.SearchItemList(obj.saveData.heldItemType), amount = 1 };
            obj.storedItemRenderer.sprite = storedItem.itemSO.itemSprite;
        }
    }
}
