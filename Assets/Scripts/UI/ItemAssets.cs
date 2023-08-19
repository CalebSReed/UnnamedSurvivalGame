using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAssets : MonoBehaviour //stores item sprites
{
    public static ItemAssets Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public Transform pfRealItem;

    public Sprite rockSpr;
    public Sprite twigSpr;
    public Sprite axeSpr;
    public Sprite pickaxeSpr;
    public Sprite shovelSpr;
    public Sprite cutGrassSpr;
    public Sprite campFireSpr;
    public Sprite torchSpr;
    public Sprite logSpr;
    public Sprite woodenClubSpr;
    public Sprite appleSpr;
    public Sprite brownShroomSpr;
    public Sprite charcoalSpr;
    public Sprite claySpr;
    public Sprite kilnKitSpr;
    public Sprite unfiredClayBowlSpr;
    public Sprite unfiredClayPlateSpr;
    public Sprite clayBowlSpr;
    public Sprite clayPlateSpr;
    public Sprite ironOreSpr;
    public Sprite wroughtIronSpr;
    public Sprite fiberSpr;
    public Sprite ropeSpr;
    public Sprite stoneAxeSpr;
    public Sprite stoneShovelSpr;
    public Sprite bowSpr;
    public Sprite bowFacingRightSpr;
    public Sprite bowAndArrowSpr;
    public Sprite bowAndArrowFacingRightSpr;
    public Sprite arrowSpr;
    public Sprite arrowFacingRightSpr;
    public Sprite featherSpr;
    public Sprite wildParsnipSpr;
    public Sprite wildCarrotSpr;
    public Sprite rawRabbitSpr;
    public Sprite cookedRabbitSpr;
    public Sprite rawDrumstickSpr;
    public Sprite cookedDrumstickSpr;
    public Sprite rawMeatSpr;
    public Sprite cookedMeatSpr;
    public Sprite rabbitFurSpr;
    public Sprite wolfFurSpr;
    public Sprite spearSpr;
    public Sprite spearFacingRightSpr;
    public Sprite deadBunnySpr;
    public Sprite bowlOfWaterSpr;
    public Sprite rawCopperSpr;
    public Sprite copperIngotSpr;
    public Sprite bronzeIngotSpr;
    public Sprite boneSpr;
    public Sprite needleSpr;
    public Sprite threadSpr;
    public Sprite needleAndThreadSpr;
    public Sprite stoneKnifeSpr;
    public Sprite bagBellowsSpr;
    public Sprite bunnyFurSheet;
}
