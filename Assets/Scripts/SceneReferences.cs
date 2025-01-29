using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneReferences : MonoBehaviour
{
    public static SceneReferences Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject HomeArrow;
    public UI_Inventory uiInventory;
    public UI_CraftMenu_Controller uiCrafter;
    public AudioManager audio;
    public GameObject starveVign;
    public GameObject freezeVign;
    public GameObject overheatVign;
    public Slider etherSlider;
    public UI_EquipSlot handSlot;
    public UI_EquipSlot headSlot;
    public UI_EquipSlot chestSlot;
    public UI_EquipSlot legsSlot;
    public UI_EquipSlot feetSlot;
    public Image heldItemImage;
    public TextMeshProUGUI heldItemTxt;
    public GameObject adrenalineIcon;
    public GameObject adrenalineVignette;
    public GameObject tuckeredOutVignette;
    public GameObject fullEtherChargeOutline;
    public SpriteRenderer deploySprite;
    public SpriteRenderer deployOutlineSprite;
    public Camera_Behavior mainCamBehavior;
}
