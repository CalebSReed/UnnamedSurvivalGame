using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_EquipSlot : MonoBehaviour, IPointerClickHandler
{
    private PlayerMain player;
    private EquipmentManager playerEquipment;
    public Image itemSpr;
    public Light bodyLight;
    public SpriteRenderer bodySprite;
    public SpriteRenderer sideBodySprite;
    public SpriteRenderer backBodySprite;
    [SerializeField] private Hoverable hoverBehavior;
    public Item currentItem { get; private set; }

    public TextMeshProUGUI itemDataText;
    [SerializeField] public Item.EquipType slotEquipType;

    [SerializeField] private Image outline;

    private void Start()
    {
        itemDataText.SetText("");
        hoverBehavior.SpecialCase = true;
        hoverBehavior.specialCaseModifier.AddListener(CheckCurrentItem);
        GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        player = GameManager.Instance.localPlayerMain;
        playerEquipment = player.GetComponent<EquipmentManager>();
    }

    public void SetItem(Item item)
    {
        currentItem = item;

        if (currentItem.itemSO.equipType == Item.EquipType.HandGear)
        {
            outline.color = Color.red;
        }
        if (item.ammo > 0)
        {
            UpdateSprite(item.itemSO.loadedSprite);
        }
        else
        {
            UpdateSprite(item.itemSO.itemSprite);
        }
    }

    public void RemoveItem()
    {
        currentItem = null;
        itemDataText.SetText("");
        itemSpr.color = new Color(0f, 0f, 0f, 0f);
        outline.color = Color.black;
    }

    private void CheckCurrentItem()
    {
        if (currentItem != null)
        {
            hoverBehavior.Prefix = "RMB: Unequip ";
            hoverBehavior.Name = currentItem.itemSO.itemName;
        }
        else
        {
            Debug.Log("No item!");
            hoverBehavior.Prefix = "";
            hoverBehavior.Name = "";
        }
    }

    public void UpdateSprite(Sprite spr)
    {
        itemSpr.sprite = spr;
        itemSpr.color = new Color(1f, 1f, 1f, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (player.StateMachine.currentPlayerState == player.deployState)
        {
            Debug.Log("Bye");
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Left)
        {

        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Debug.Log("Middle click");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (player.isHoldingItem)
            {
                player.LoadHandItem(player.equippedHandItem, player.heldItem);
            }
            else
            {
                player.UnequipItem(slotEquipType);
            }
        }
    }
}