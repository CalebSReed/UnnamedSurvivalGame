using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemObjectArray : MonoBehaviour
{
    public static ItemObjectArray Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void OnValidate()
    {
        //SetNewIDs();
    }
#if UNITY_EDITOR
    //[MenuItem("ManageGameAssets/Set New IDs for Asset Type/WorldObjects")]
    public void SetNewIDs()
    {
        int i = 0;
        foreach (ItemSO SO in ItemSOList)
        {
            if (i == 0)
            {
                i++;
                continue;
            }
            else if (SO.itemID == 0)
            {
                Undo.RecordObject(SO, "Set Object ID");
                SO.itemID = i;
                EditorUtility.SetDirty(SO);
                Debug.Log($"SET {SO.itemType} TO ID: {SO.itemID}");
            }
            i++;
        }
    }
#endif

    public ItemSO[] ItemSOList;//if we create an ID Dictionary system, saving and loading would be lightning fast compared what this shit is rn lol

    public ItemSO SearchItemList(int _itemType)
    {
        foreach (ItemSO _itemSO in ItemSOList)
        {
            if (_itemType == _itemSO.itemID)
            {
                return _itemSO;
            }
        }
        Debug.LogError($"Cannot find item ID: {_itemType}!!");
        return null;
    }

    public ItemSO SearchItemList(string _itemType)
    {
        foreach (ItemSO _itemSO in ItemSOList)
        {
            if (_itemType == _itemSO.itemType)
            {
                return _itemSO;
            }
        }
        Debug.LogError($"Cannot find item type: {_itemType}!!");
        return null;
    }

    public Transform pfItem;
    public Transform pfProjectile;

    /*

    public ItemSO Null;
    public ItemSO StoneAxe;
    public ItemSO Rock;
    public ItemSO Twig;
    public ItemSO Axe;
    public ItemSO Pickaxe;
    public ItemSO Shovel;
    public ItemSO Campfire;
    public ItemSO CutGrass;
    public ItemSO Torch;
    public ItemSO Log;
    public ItemSO WoodenClub;
    public ItemSO Apple;
    public ItemSO BrownShroom;
    public ItemSO Charcoal;
    public ItemSO Clay;
    public ItemSO KilnKit;
    public ItemSO UnfiredClayBowl;
    public ItemSO UnfiredClayPlate;
    public ItemSO ClayBowl;
    public ItemSO ClayPlate;
    public ItemSO IronOre;
    public ItemSO WroughtIron;
    public ItemSO Fiber;
    public ItemSO Rope;
    public ItemSO BagBellows;
    public ItemSO WildParsnip;
    public ItemSO WildCarrot;
    public ItemSO StoneShovel;
    public ItemSO Arrow;
    public ItemSO Bow;
    public ItemSO Feather;
    public ItemSO RawRabbit;
    public ItemSO CookedRabbit;
    public ItemSO RabbitFur;
    public ItemSO RawMeat;
    public ItemSO CookedMeat;
    public ItemSO WolfFur;
    public ItemSO RawDrumstick;
    public ItemSO CookedDrumstick;
    public ItemSO Spear;
    public ItemSO DeadBunny;
    public ItemSO BowlOfWater;
    public ItemSO RawCopper;
    public ItemSO CopperIngot;
    public ItemSO BronzeIngot;
    public ItemSO Thread;
    public ItemSO Needle;
    public ItemSO Bone;
    public ItemSO StoneKnife;
    public ItemSO BunnyFurSheet;
    public ItemSO RawTin;
    public ItemSO TinIngot;
    public ItemSO RawTinBowl;
    public ItemSO RawCopperBowl;
    public ItemSO CopperBowl;
    public ItemSO CharcoalBowl;
    public ItemSO TinBowl;
    public ItemSO CopperAndTinBowl;
    public ItemSO BronzeCrucible;
    public ItemSO BronzeAxe;
    public ItemSO BronzePickaxe;
    public ItemSO BronzeShovel;
    public ItemSO BronzeSword;
    public ItemSO StoneHammer;
    public ItemSO BronzeAxeHead;
    public ItemSO BronzePickaxeHead;
    public ItemSO BronzeShovelHead;
    public ItemSO BronzeSwordHead;
    public ItemSO RawBread;
    public ItemSO PlatedRawBread;
    public ItemSO PlatedCookedBread;
    public ItemSO Wheat;
    public ItemSO RawPieCrust;
    public ItemSO RawMeatPie;
    public ItemSO PlatedRawMeatPie;
    public ItemSO PlatedCookedMeatPie;
    public ItemSO RawRabbitPie;
    public ItemSO PlatedRawRabbitPie;
    public ItemSO PlatedCookedRabbitPie;
    public ItemSO RawGold;
    public ItemSO RawGoldBowl;
    public ItemSO GoldIngot;
    public ItemSO GoldCrown;
    public ItemSO SheepWool;
    public ItemSO RawMutton;
    public ItemSO CookedMutton;
    public ItemSO DirtBeaconKit;
    public ItemSO BronzeBlade;
    public ItemSO BronzeBladeBlank;
    public ItemSO BronzeChisel;
    public ItemSO BronzeShears;
    public ItemSO BronzeKnife;
    public ItemSO WoodMallet;
    public ItemSO BronzeChiselAndWoodMallet;
    public ItemSO BronzeFileBlank;
    public ItemSO BronzeFile;
    public ItemSO KnittingNeedles;
    public ItemSO WoodenDisc;
    public ItemSO DropSpindle;
    public ItemSO WoolYarnSmall;
    public ItemSO WoolYarnMedium;
    public ItemSO WoolYarnLarge;
    public ItemSO BronzeSawHead;
    public ItemSO BronzeSaw;
    public ItemSO WoolHat;*/
}
