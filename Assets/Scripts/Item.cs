using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SerializeField]
public class Item //You know... scriptable items aren't looking too bad rn Morty....
{
    public enum EquipType
    {
        Null,
        HandGear,
        HeadGear,
        ChestGear,
        LegGear,
        FootGear
    }

    public ItemSO itemSO;
    //public ItemType itemType;//change to itemso.itemtype     nvm u cant do that....
    public EquipType equipType;
    public int amount;
    public int uses;
    public int ammo;
    /*
    public Sprite GetSprite() //method we can call in any script to return sprite for which type of item object is.
    {
        switch (itemType)
        {
            default:
            case ItemType.Rock: return ItemAssets.Instance.rockSpr;
            case ItemType.Twig: return ItemAssets.Instance.twigSpr;
            case ItemType.Axe: return ItemAssets.Instance.axeSpr;
            case ItemType.Pickaxe: return ItemAssets.Instance.pickaxeSpr;
            case ItemType.Shovel: return ItemAssets.Instance.shovelSpr;
            case ItemType.CutGrass: return ItemAssets.Instance.cutGrassSpr;
            case ItemType.Campfire: return ItemAssets.Instance.campFireSpr;
            case ItemType.Torch: return ItemAssets.Instance.torchSpr;
            case ItemType.Log: return ItemAssets.Instance.logSpr;
            case ItemType.WoodenClub: return ItemAssets.Instance.woodenClubSpr;
            case ItemType.Apple: return ItemAssets.Instance.appleSpr;
            case ItemType.BrownShroom: return ItemAssets.Instance.brownShroomSpr;
            case ItemType.Charcoal: return ItemAssets.Instance.charcoalSpr;
            case ItemType.Clay: return ItemAssets.Instance.claySpr;
            case ItemType.KilnKit: return ItemAssets.Instance.kilnKitSpr;
            case ItemType.UnfiredClayBowl: return ItemAssets.Instance.unfiredClayBowlSpr;
            case ItemType.ClayBowl: return ItemAssets.Instance.clayBowlSpr;
            case ItemType.UnfiredClayPlate: return ItemAssets.Instance.unfiredClayPlateSpr;
            case ItemType.ClayPlate: return ItemAssets.Instance.clayPlateSpr;
            case ItemType.IronOre: return ItemAssets.Instance.ironOreSpr;
            case ItemType.WroughtIron: return ItemAssets.Instance.wroughtIronSpr;
            case ItemType.Fiber: return ItemAssets.Instance.fiberSpr;
            case ItemType.Rope: return ItemAssets.Instance.ropeSpr;
            case ItemType.StoneAxe: return ItemAssets.Instance.stoneAxeSpr;
            case ItemType.StoneShovel: return ItemAssets.Instance.stoneShovelSpr;
            case ItemType.Arrow: return ItemAssets.Instance.arrowSpr;
            case ItemType.Bow: return ItemAssets.Instance.bowSpr;
            case ItemType.WildCarrot: return ItemAssets.Instance.wildCarrotSpr;
            case ItemType.WildParsnip: return ItemAssets.Instance.wildParsnipSpr;
            case ItemType.Feather: return ItemAssets.Instance.featherSpr;
            case ItemType.RawRabbit: return ItemAssets.Instance.rawRabbitSpr;
            case ItemType.CookedRabbit: return ItemAssets.Instance.cookedRabbitSpr;
            case ItemType.RabbitFur: return ItemAssets.Instance.rabbitFurSpr;
            case ItemType.RawMeat: return ItemAssets.Instance.rawMeatSpr;
            case ItemType.CookedMeat: return ItemAssets.Instance.cookedMeatSpr;
            case ItemType.RawDrumstick: return ItemAssets.Instance.rawDrumstickSpr;
            case ItemType.CookedDrumstick: return ItemAssets.Instance.cookedDrumstickSpr;
            case ItemType.WolfFur: return ItemAssets.Instance.wolfFurSpr;
            case ItemType.Spear: return ItemAssets.Instance.spearSpr;
            case ItemType.DeadBunny: return ItemAssets.Instance.deadBunnySpr;
            case ItemType.BowlOfWater: return ItemAssets.Instance.bowlOfWaterSpr;
            case ItemType.RawCopper: return ItemAssets.Instance.rawCopperSpr;
            case ItemType.CopperIngot: return ItemAssets.Instance.copperIngotSpr;
            case ItemType.BronzeIngot: return ItemAssets.Instance.bronzeIngotSpr;
            case ItemType.Thread: return ItemAssets.Instance.threadSpr;
            case ItemType.Needle: return ItemAssets.Instance.needleSpr;
            case ItemType.BagBellows: return ItemAssets.Instance.bagBellowsSpr;
            case ItemType.Bone: return ItemAssets.Instance.boneSpr;
            case ItemType.StoneKnife: return ItemAssets.Instance.stoneKnifeSpr;
            case ItemType.BunnyFurSheet: return ItemAssets.Instance.bunnyFurSheet;
            case ItemType.RawTin: return ItemAssets.Instance.rawTinSpr;
            case ItemType.TinIngot: return ItemAssets.Instance.tinIngotSpr;
            case ItemType.RawTinBowl: return ItemAssets.Instance.rawTinBowlSpr;
            case ItemType.RawCopperBowl: return ItemAssets.Instance.rawCopperBowlSpr;
            case ItemType.CopperBowl: return ItemAssets.Instance.copperBowlSpr;
            case ItemType.TinBowl: return ItemAssets.Instance.tinBowlSpr;
            case ItemType.CopperAndTinBowl: return ItemAssets.Instance.copperAndTinBowlSpr;
            case ItemType.BronzeCrucible: return ItemAssets.Instance.crucibleSpr;
            case ItemType.BronzeAxe: return ItemAssets.Instance.bronzeAxeSpr;
            case ItemType.BronzePickaxe: return ItemAssets.Instance.bronzePickaxeSpr;
            case ItemType.BronzeShovel: return ItemAssets.Instance.bronzeShovelSpr;
            case ItemType.BronzeSword: return ItemAssets.Instance.bronzeSwordSpr;
            case ItemType.StoneHammer: return ItemAssets.Instance.stoneHammerSpr;
            case ItemType.BronzeAxeHead: return ItemAssets.Instance.bronzeAxeHeadSpr;
            case ItemType.BronzePickaxeHead: return ItemAssets.Instance.bronzePickaxeHeadSpr;
            case ItemType.BronzeShovelHead: return ItemAssets.Instance.bronzeShovelHeadSpr;
            case ItemType.BronzeSwordHead: return ItemAssets.Instance.bronzeSwordHeadSpr;
        }
    }

    public bool isStackable()
    {
        switch(itemType)
        {
            default:
                return true;
            case ItemType.Axe:
            case ItemType.Pickaxe:
            case ItemType.Shovel:
            case ItemType.Campfire:
            case ItemType.Torch:
            case ItemType.WoodenClub:
            case ItemType.KilnKit:
            case ItemType.StoneAxe:
            case ItemType.StoneShovel:
            case ItemType.Bow:
            case ItemType.Spear:
            case ItemType.BowlOfWater:
            case ItemType.BagBellows:
            case ItemType.Needle:
            case ItemType.StoneKnife:
            case ItemType.CopperAndTinBowl:
            case ItemType.CopperBowl:
            case ItemType.RawCopperBowl:
            case ItemType.RawTinBowl:
            case ItemType.TinBowl:
            case ItemType.BronzeCrucible:
            case ItemType.BronzeAxe:
            case ItemType.BronzePickaxe:
            case ItemType.BronzeShovel:
            case ItemType.BronzeSword:
            case ItemType.StoneHammer:
            case ItemType.BronzeAxeHead:
            case ItemType.BronzePickaxeHead:
            case ItemType.BronzeShovelHead:
            case ItemType.BronzeSwordHead:
                return false;
        }
    }
    public int GetItemStackSize() //if unstackable dont put them in here
    {
        switch(itemType)
        {
            default:
            case ItemType.Twig: return 20;
            case ItemType.Rock: return 20;
            case ItemType.CutGrass: return 20;
            case ItemType.Log: return 20;
            case ItemType.Apple: return 10;
            case ItemType.BrownShroom: return 10;
            case ItemType.Charcoal: return 20;
            case ItemType.Clay: return 20;
            case ItemType.UnfiredClayBowl: return 20;
            case ItemType.UnfiredClayPlate: return 20;
            case ItemType.ClayBowl: return 20;
            case ItemType.ClayPlate: return 20;
            case ItemType.IronOre: return 20;
            case ItemType.WroughtIron: return 20;
            case ItemType.Fiber: return 20;
            case ItemType.Rope: return 20;
            case ItemType.Arrow: return 20;
            case ItemType.Feather: return 20;
            case ItemType.WildCarrot: return 10;
            case ItemType.WildParsnip: return 10;
            case ItemType.RawRabbit: return 10; 
            case ItemType.CookedRabbit: return 10;
            case ItemType.RabbitFur: return 10;
            case ItemType.RawMeat: return 10;
            case ItemType.CookedMeat: return 10;
            case ItemType.RawDrumstick: return 10;
            case ItemType.CookedDrumstick: return 10;
            case ItemType.WolfFur: return 10;
            case ItemType.DeadBunny: return 10;
            case ItemType.RawCopper: return 20;
            case ItemType.CopperIngot: return 20;
            case ItemType.BronzeIngot: return 20;
            case ItemType.Bone: return 20;
            case ItemType.Thread: return 20;
            case ItemType.BunnyFurSheet: return 10;
        }
    }

    public Action.ActionType GetDoableAction()//rename to GetAction imma use this for stuff that gets actioned on too
    {
        switch (itemType)
        {
            default:
            case ItemType.Axe: return Action.ActionType.Chop;
            case ItemType.Pickaxe: return Action.ActionType.Mine;
            case ItemType.Shovel: return Action.ActionType.Dig;
            case ItemType.WoodenClub: return Action.ActionType.Melee;
            case ItemType.Torch: return Action.ActionType.Burn;
            case ItemType.StoneAxe: return Action.ActionType.Chop;
            case ItemType.StoneShovel: return Action.ActionType.Dig;
            case ItemType.Spear: return Action.ActionType.Throw;
            case ItemType.Bow: return Action.ActionType.Shoot;
            case ItemType.BowlOfWater: return Action.ActionType.Water;
            case ItemType.ClayBowl: return Action.ActionType.Scoop;
            case ItemType.StoneKnife: return Action.ActionType.Cut;
            case ItemType.RabbitFur: return Action.ActionType.Sew;
            case ItemType.DeadBunny: return Action.ActionType.Cut;
            case ItemType.Needle: return Action.ActionType.Sew;
            case ItemType.Bone: return Action.ActionType.Cut;
            case ItemType.BronzeAxe: return Action.ActionType.Chop;
            case ItemType.BronzePickaxe: return Action.ActionType.Mine;
            case ItemType.BronzeShovel: return Action.ActionType.Dig;
            case ItemType.BronzeSword: return Action.ActionType.Melee;
            case ItemType.StoneHammer: return Action.ActionType.Hammer;
            case ItemType.BronzeIngot: return Action.ActionType.Hammer;
            case ItemType.BronzeAxeHead: return Action.ActionType.Hammer;
            case ItemType.BronzePickaxeHead: return Action.ActionType.Hammer;
            case ItemType.BronzeShovelHead: return Action.ActionType.Hammer;
            case ItemType.BronzeSwordHead: return Action.ActionType.Hammer;
        }
    }

    public bool CanStoreItems()
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.ClayBowl:
            case ItemType.CopperAndTinBowl:
            case ItemType.CopperBowl:
            case ItemType.RawCopperBowl:
            case ItemType.RawTinBowl:
            case ItemType.TinBowl:
                return true;
        }
    }

    public ItemType[] StorableItems()//bro u have GOT to fix this shit
    {
        switch (itemType)
        {
            default: return new ItemType[] { ItemType.Null };
            case ItemType.ClayBowl: return new ItemType[] { ItemType.RawCopper, ItemType.CopperIngot, ItemType.TinIngot, ItemType.RawTin };//maybe we dont need a list if a BOWL is the only thing that has multiple cases...
            case ItemType.CopperAndTinBowl: return new ItemType[] { ItemType.ClayPlate };
            case ItemType.CopperBowl: return new ItemType[] { ItemType.TinIngot, ItemType.TinBowl };
            case ItemType.TinBowl: return new ItemType[] { ItemType.CopperIngot, ItemType.CopperBowl };
        }
    }

    public ItemType[] StoredItemReward()//omg fucking genius using a list for the rewards... but srsly scriptable object Items looking rly cool rn.  case ItemType.: return new ItemType[] { ItemType. };
    {
        switch (itemType)
        {
            default: return new ItemType[] { ItemType.Null };
            case ItemType.ClayBowl: return new ItemType[] { ItemType.RawCopperBowl, ItemType.CopperBowl, ItemType.TinBowl, ItemType.RawTinBowl };
            case ItemType.CopperAndTinBowl: return new ItemType[] { ItemType.BronzeCrucible };
            case ItemType.CopperBowl: return new ItemType[] { ItemType.CopperAndTinBowl, ItemType.CopperAndTinBowl };
            case ItemType.TinBowl: return new ItemType[] { ItemType.CopperAndTinBowl, ItemType.CopperAndTinBowl };
        }
    }

    public ItemType[] GetActionReward()//make an array, or list or whatever, so we can have multiple items as rewards
    {
        switch (itemType)
        {
            default: return new ItemType[] { ItemType.Null };
            case ItemType.DeadBunny: return new ItemType[] { ItemType.RawRabbit, ItemType.Bone, ItemType.RabbitFur};
            case ItemType.RabbitFur: return new ItemType[] { ItemType.BunnyFurSheet};
            case ItemType.Bone: return new ItemType[] { ItemType.Needle };
            case ItemType.BronzeIngot: return new ItemType[] { ItemType.BronzeAxeHead };
        }
    }

    public bool NeedsToBeHot()
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.BronzeIngot:
            case ItemType.BronzeAxeHead:
            case ItemType.BronzePickaxeHead:
            case ItemType.BronzeShovelHead:
            case ItemType.BronzeSwordHead:
                return true;
        }

    }

    public bool NeedsAmmo()
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.Bow: return true;
            case ItemType.Needle: return true;
        }
    }

    public ItemType ValidAmmo()
    {
        switch (itemType)
        {
            default: return ItemType.Null;
            case ItemType.Bow: return ItemType.Arrow;
            case ItemType.Needle: return ItemType.Thread;
        }
    }

    public int GetMaxAmmo()
    {
        switch (itemType)
        {
            default: return 0;
            case ItemType.Bow: return 1;
            case ItemType.Needle: return 1;
        }
    }

    public Sprite GetLoadedSprite()
    {
        switch (itemType)
        {
            default: return null;
            case ItemType.Bow: return ItemAssets.Instance.bowAndArrowSpr;
            case ItemType.Needle: return ItemAssets.Instance.needleAndThreadSpr;
        }
    }

    public Sprite GetAimingSprite()
    {
        switch (itemType)
        {
            default: return null;
            case ItemType.Bow: return ItemAssets.Instance.bowFacingRightSpr;
            case ItemType.Arrow: return ItemAssets.Instance.arrowFacingRightSpr;
            case ItemType.Spear: return ItemAssets.Instance.spearFacingRightSpr;
        }
    }

    public Sprite GetLoadedHandSprite()
    {
        switch (itemType)
        {
            default: return null;
            case ItemType.Bow: return ItemAssets.Instance.bowAndArrowFacingRightSpr;
        }
    }

    public bool isAmmo()//change from iscombinable to isAmmoableWeapon and check if ammoable, and if holding item is ammo, then add ammo, change ONLY SPRITE, 
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.Arrow: return true;
            case ItemType.Thread: return true;
        }
    }

    public bool isSmeltable()
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.UnfiredClayBowl: return true;
            case ItemType.UnfiredClayPlate: return true;
            case ItemType.IronOre: return true;
            case ItemType.RawCopperBowl: return true;
            case ItemType.CopperIngot: return true;
            case ItemType.RawTinBowl: return true;
            case ItemType.BronzeCrucible: return true;
            case ItemType.BronzeIngot: return true;
        }
    }

    public int GetSmeltValue()//add burnvalue, where too high burns the metal or even burns the crucible itself!! :O
    {
        switch (itemType)
        {
            default: return 0;
            case ItemType.UnfiredClayBowl: return 760;
            case ItemType.UnfiredClayPlate: return 760;
            case ItemType.IronOre: return 1540;//needs citation
            case ItemType.RawCopperBowl: return 1085;
            case ItemType.CopperIngot: return 1085;//literally barely doable in default kiln with charcoal... player will NEED to use an upgrade if they wish not to spend an overwhelming amount of charcoal
            case ItemType.RawTinBowl: return 230;
            case ItemType.BronzeCrucible: return 1085;
            case ItemType.BronzeIngot: return 950;
        }
    }

    public int GetRequiredSmeltingTime()
    {
        switch (itemType)
        {
            default: return 0;
            case ItemType.UnfiredClayBowl: return 5;
            case ItemType.UnfiredClayPlate: return 5;
            case ItemType.IronOre: return 30;
            case ItemType.RawCopperBowl: return 10;
            case ItemType.CopperIngot: return 10;
            case ItemType.RawTinBowl: return 5;
            case ItemType.BronzeCrucible: return 10;
            case ItemType.BronzeIngot: return 10;
        }
    }

    public ItemType GetSmeltReward()
    {
        switch (itemType)
        {
            default: return ItemType.Null;
            case ItemType.UnfiredClayBowl: return ItemType.ClayBowl;
            case ItemType.UnfiredClayPlate: return ItemType.ClayPlate;
            case ItemType.IronOre: return ItemType.WroughtIron;
            case ItemType.RawCopperBowl: return ItemType.CopperIngot;//ingots should return hot ingots, add hot bool to realitem, if hot, cant pick up, but can be interacted with a hammer??
            case ItemType.RawTinBowl: return ItemType.TinIngot;
            case ItemType.CopperIngot: return ItemType.CopperIngot;
            case ItemType.TinIngot: return ItemType.TinIngot;
            case ItemType.BronzeCrucible: return ItemType.BronzeIngot;
        }
    }

    public bool IsFuel()
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.Log: return true;
            case ItemType.Charcoal: return true;
        }
    }

    public int GetFuelValue()
    {
        switch (itemType)
        {
            default: return 0;
            case ItemType.Log: return 10;
            case ItemType.Charcoal: return 30;
        }
    }

    public int GetTemperatureBurnValue()
    {
        switch (itemType)
        {
            default: return 0;
            case ItemType.Log: return 900;
            case ItemType.Charcoal: return 1100;
        }
    }

    public bool isEatable()
    {
        switch (itemType)
        {
            default:
                return false;
            case ItemType.Apple:
            case ItemType.BrownShroom:
            case ItemType.WildParsnip:
            case ItemType.WildCarrot:
            case ItemType.RawDrumstick:
            case ItemType.CookedDrumstick:
            case ItemType.RawRabbit:
            case ItemType.CookedRabbit:
            case ItemType.RawMeat:
            case ItemType.CookedMeat:
                return true;
        }
    }

    public int[] GetRestorationValues()//0 = hp, 1 = hunger, 2 = sanity (coming soon)
    {
        switch (itemType)
        {
            default: return new int[] { 0, 0, 0 };
            case ItemType.Apple: return new int[] { 1, 10, 0 };
            case ItemType.BrownShroom: return new int[] { 0, 10, 0 };
            case ItemType.WildCarrot: return new int[] { 2, 15, 0 };
            case ItemType.WildParsnip: return new int[] { 2, 20, 0 };
            case ItemType.RawDrumstick: return new int[] { -5, 10, 0 };
            case ItemType.CookedDrumstick: return new int[] { 4, 25, 0 };
            case ItemType.RawRabbit: return new int[] { -5, 15, 0 };
            case ItemType.CookedRabbit: return new int[] { 4, 30, 0 };
            case ItemType.RawMeat: return new int[] { -5, 20, 0 };
            case ItemType.CookedMeat: return new int[] { 4, 40, 0 };
        }
    }

    public bool IsCookable()
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.RawDrumstick:
            case ItemType.RawMeat:
            case ItemType.RawRabbit:
                return true;
        }
    }

    public ItemType GetCookingReward()
    {
        switch (itemType)
        {
            default: return ItemType.Null;
            case ItemType.RawDrumstick: return ItemType.CookedDrumstick;
            case ItemType.RawMeat: return ItemType.CookedMeat;
            case ItemType.RawRabbit: return ItemType.CookedRabbit;
        }
    }

    public bool isEquippable()
    {
        switch(itemType)
        {
            default:
                return false;
            case ItemType.Axe:
            case ItemType.Pickaxe:
            case ItemType.Shovel:
            case ItemType.WoodenClub:
            case ItemType.Torch:
            case ItemType.StoneAxe:
            case ItemType.StoneShovel:
            case ItemType.Bow:
            case ItemType.Spear:
            case ItemType.BronzeAxe:
            case ItemType.BronzePickaxe:
            case ItemType.BronzeShovel:
            case ItemType.BronzeSword:
                return true;
        }
    }

    public bool isDeployable()
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.Campfire:
            case ItemType.KilnKit:
                return true;
        }
    }

    public WorldObject.worldObjectType GetDeployableObject()
    {
        switch (itemType)
        {
            default:
            case ItemType.Campfire: return WorldObject.worldObjectType.Campfire;
            case ItemType.KilnKit: return WorldObject.worldObjectType.Kiln;
        }
    }

    public int GetMaxItemUses()
    {
        switch (itemType)
        {
            default:
                return 0;
            case ItemType.Axe: return 100;
            case ItemType.Pickaxe: return 100;
            case ItemType.Shovel: return 100;
            case ItemType.WoodenClub: return 100;
            case ItemType.Torch: return 100;
            case ItemType.StoneAxe: return 50;
            case ItemType.StoneShovel: return 50;
            case ItemType.Bow: return 50;
            case ItemType.Spear: return 50;
            case ItemType.StoneKnife: return 25;
            case ItemType.Needle: return 25;
            case ItemType.BronzeAxe: return 150;
            case ItemType.BronzePickaxe: return 150;
            case ItemType.BronzeShovel: return 150;
            case ItemType.BronzeSword: return 150;
        }
    }

    public int GetDamage()
    {
        switch (itemType)
        {
            default: return 0;
            case ItemType.Arrow: return 50;
            case ItemType.WoodenClub: return 25;
            case ItemType.Spear: return 25;
            case ItemType.BronzeSword: return 60;
        }
    }

    public bool IsBowl()//i think this is dumb and specific change to iscontainer??? and shift sprites around???? hmmmm...
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.ClayBowl: return true;
            case ItemType.BowlOfWater: return true;
            case ItemType.RawCopperBowl: return true;
            case ItemType.RawTinBowl: return true;
            case ItemType.TinBowl: return true;
            case ItemType.CopperBowl: return true;
            case ItemType.BronzeCrucible: return true;
        }
    }*/
}
