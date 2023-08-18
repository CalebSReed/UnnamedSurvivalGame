using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SerializeField]
public class Item //stores item type data
{
    public enum ItemType //declares abstract types of items
    {
        Null,
        Rock,
        Twig,
        Axe,
        Pickaxe,
        Shovel,
        Campfire,
        CutGrass,
        Torch,
        Log,
        WoodenClub,
        Apple,
        BrownShroom,
        Charcoal,
        Clay,
        KilnKit,
        StoneAxe,
        UnfiredClayBowl,
        UnfiredClayPlate,
        ClayBowl,
        ClayPlate,
        IronOre,
        WroughtIron,
        Fiber,
        Rope,
        Bellows,
        WildParsnip,
        WildCarrot,
        StoneShovel,
        Arrow,
        Bow,
        Feather,
        RawRabbit,
        CookedRabbit,
        RabbitFur,
        RawMeat,
        CookedMeat,
        WolfFur,
        RawDrumstick,
        CookedDrumstick,
        Spear,
        DeadBunny,
        BowlOfWater
    }

    public ItemType itemType;
    public int amount;
    public int uses;
    public int ammo;

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
        }
    }

    public Action.ActionType GetDoableAction()//move to item class
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
        }
    }

    public bool NeedsAmmo()
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.Bow: return true;
        }
    }

    public ItemType ValidAmmo()
    {
        switch (itemType)
        {
            default: return ItemType.Null;
            case ItemType.Bow: return ItemType.Arrow;
        }
    }

    public int GetMaxAmmo()
    {
        switch (itemType)
        {
            default: return 0;
            case ItemType.Bow: return 1;
        }
    }

    public Sprite GetLoadedSprite()
    {
        switch (itemType)
        {
            default: return null;
            case ItemType.Bow: return ItemAssets.Instance.bowAndArrowSpr;
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
        }
    }

    public int GetSmeltValue()
    {
        switch (itemType)
        {
            default: return 0;
            case ItemType.UnfiredClayBowl: return 760;
            case ItemType.UnfiredClayPlate: return 760;
            case ItemType.IronOre: return 1540;//needs citation
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
        }
    }

    public ItemType GetSmeltReward()
    {
        switch (itemType)
        {
            default: return Item.ItemType.Null;
            case ItemType.UnfiredClayBowl: return ItemType.ClayBowl;
            case ItemType.UnfiredClayPlate: return ItemType.ClayPlate;
            case ItemType.IronOre: return ItemType.WroughtIron;
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

    public int GetCalories()
    {
        switch (itemType)
        {
            default: return 0;
            case ItemType.Apple: return 10;
            case ItemType.BrownShroom: return 15;
            case ItemType.WildCarrot: return 25;
            case ItemType.WildParsnip: return 25;
            case ItemType.RawDrumstick: return 10;
            case ItemType.CookedDrumstick: return 30;
            case ItemType.RawRabbit: return 10;
            case ItemType.CookedRabbit: return 30;
            case ItemType.RawMeat: return 15;
            case ItemType.CookedMeat: return 40;
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
        }
    }

    public bool IsBowl()
    {
        switch (itemType)
        {
            default: return false;
            case ItemType.ClayBowl: return true;
            case ItemType.BowlOfWater: return true;
        }
    }
}
