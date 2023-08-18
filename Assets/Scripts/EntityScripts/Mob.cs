using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mob
{
    public enum MobType
    {
        Bunny,
        Wolf,
        Turkey,
        Eyeris
    }

    public MobType mobType;

    public Sprite GetSprite()
    {
        switch (mobType)
        {
            default: return null;
            case MobType.Bunny: return Mob_Assets.Instance.bunny;
            case MobType.Wolf: return Mob_Assets.Instance.wolf;
            case MobType.Turkey: return Mob_Assets.Instance.turkey;
            case MobType.Eyeris: return Mob_Assets.Instance.eyeris;
        }
    }

    public int GetMaxHealth()
    {
        switch (mobType)
        {
            default: return 0;
            case MobType.Bunny: return 25;
            case MobType.Turkey: return 100;
            case MobType.Wolf: return 250;
            case MobType.Eyeris: return 400;
        }
    }

    public List<Item> GetLootTable()
    {
        switch (mobType)
        {
            default: return null;
            case MobType.Bunny:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.RabbitFur, amount = 1 },
                new Item { itemType = Item.ItemType.RawRabbit, amount = 1 },
            };
            case MobType.Wolf:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.WolfFur, amount = 1 },
                new Item { itemType = Item.ItemType.RawMeat, amount = 1 },
                new Item { itemType = Item.ItemType.RawMeat, amount = 1 },
            };
            case MobType.Turkey:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Feather, amount = 1 },
                new Item { itemType = Item.ItemType.Feather, amount = 1 },
                new Item { itemType = Item.ItemType.Feather, amount = 1 },
                new Item { itemType = Item.ItemType.Feather, amount = 1 },
                new Item { itemType = Item.ItemType.Feather, amount = 1 },
                new Item { itemType = Item.ItemType.Feather, amount = 1 },
                new Item { itemType = Item.ItemType.RawMeat, amount = 1 },
                new Item { itemType = Item.ItemType.RawDrumstick, amount = 1 },
                new Item { itemType = Item.ItemType.RawDrumstick, amount = 1 },
            };
        }
    }
}
