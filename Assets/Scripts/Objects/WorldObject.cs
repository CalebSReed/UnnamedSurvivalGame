using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

[SerializeField]
public class WorldObject
{
    public enum worldObjectType
    {
        Tree,
        Boulder,
        ClayDeposit,
        Campfire,
        Kiln,
        Sapling,
        Milkweed,
        WildParsnip,
        WildCarrot,
        BrownShroom,
        BunnyHole,
        MagicalTree,
        HotCoals,
        Pond
    }

    public worldObjectType objType;

    public Sprite GetSprite()
    {
        switch (objType)
        {
            default:
            case worldObjectType.Tree: return WorldObject_Assets.Instance.tree;
            case worldObjectType.Boulder: return WorldObject_Assets.Instance.boulder;
            case worldObjectType.ClayDeposit: return WorldObject_Assets.Instance.clayDeposit;
            case worldObjectType.Campfire: return WorldObject_Assets.Instance.campFire;
            case worldObjectType.Kiln: return WorldObject_Assets.Instance.kiln;
            case worldObjectType.Sapling: return WorldObject_Assets.Instance.sapling;
            case worldObjectType.Milkweed: return WorldObject_Assets.Instance.milkweed;
            case worldObjectType.BrownShroom: return WorldObject_Assets.Instance.brownShroom;
            case worldObjectType.WildParsnip: return WorldObject_Assets.Instance.wildParsnip;
            case worldObjectType.WildCarrot: return WorldObject_Assets.Instance.wildCarrot;
            case worldObjectType.BunnyHole: return WorldObject_Assets.Instance.bunnyHole;
            case worldObjectType.MagicalTree: return WorldObject_Assets.Instance.magicalTree;
            case worldObjectType.HotCoals: return WorldObject_Assets.Instance.hotCoals;
            case worldObjectType.Pond: return WorldObject_Assets.Instance.pond;
        }
    }

    public Action.ActionType GetAction()
    {
        switch(objType)
        {
            default: return 0;
            case worldObjectType.Tree: return Action.ActionType.Chop;
            case worldObjectType.Boulder: return Action.ActionType.Mine;
            case worldObjectType.ClayDeposit: return Action.ActionType.Default;
            case worldObjectType.Kiln: return Action.ActionType.Mine;
            case worldObjectType.Sapling: return Action.ActionType.Default;
            case worldObjectType.Milkweed: return Action.ActionType.Default;
            case worldObjectType.BrownShroom: return Action.ActionType.Default;
            case worldObjectType.WildCarrot: return Action.ActionType.Default;
            case worldObjectType.WildParsnip: return Action.ActionType.Default;
            case worldObjectType.BunnyHole: return Action.ActionType.Dig;
            case worldObjectType.MagicalTree: return Action.ActionType.Chop;
            case worldObjectType.HotCoals: return Action.ActionType.Cook;
            case worldObjectType.Campfire: return Action.ActionType.Water;
            case worldObjectType.Pond: return Action.ActionType.Scoop;
        }
    }

    public bool WillTransition()
    {
        switch (objType)
        {
            default: return false;
            case worldObjectType.Campfire: return true;
        }
    }

    public worldObjectType ObjectTransition()
    {
        switch (objType)
        {
            default: return worldObjectType.MagicalTree;//too lazy to make a null object already....
            case worldObjectType.Campfire: return worldObjectType.HotCoals;
        }
    }

    public bool IsInteractable()
    {
        switch (objType)
        {
            default:
                return false;
            case worldObjectType.Kiln:
            case worldObjectType.Campfire:
            case worldObjectType.HotCoals:
            case worldObjectType.Pond:
                return true;
        }  
    }

    public Item.ItemType[] GetAcceptableFuelGiven()
    {
        switch (objType)
        {
            default: 
                return new Item.ItemType[] { Item.ItemType.Null };
            case WorldObject.worldObjectType.Kiln:
                return new Item.ItemType[] { Item.ItemType.Log, Item.ItemType.Charcoal };
            //return Item.ItemType.Charcoal;
            case WorldObject.worldObjectType.Campfire:
                return new Item.ItemType[] { Item.ItemType.Log, Item.ItemType.Charcoal };
        }
    }

    public Item.ItemType[] GetAcceptableSmeltingItems()//this might be overkill items already have a smeltable bool
    {
        switch (objType)
        {
            default:
                return new Item.ItemType[] { Item.ItemType.Null };
            case WorldObject.worldObjectType.Kiln:
                return new Item.ItemType[] { Item.ItemType.UnfiredClayBowl, Item.ItemType.UnfiredClayPlate, Item.ItemType.IronOre , Item.ItemType.RawCopper};
                //return Item.ItemType.Charcoal;
        }
    }

    public int GetMaxTemperature()
    {
        switch (objType)
        {
            default: return 0;
            case worldObjectType.Kiln: return 3000;//fireclay is 4000 tho
        }
    }

    public int GetMintemperature()
    {
        switch (objType)
        {
            default: return 0;
            case worldObjectType.Kiln: return 400; //set to minimum burning temp of wood... kinda 
        }
    }

    public int GetMaxFuel()
    {
        switch (objType)
        {
            default: return 0;
            case worldObjectType.Kiln: return 30;
            case worldObjectType.Campfire: return 60;
        }
    }

    public bool IsBurnable()
    {
        switch (objType)
        {
            default:
                return false;
            case worldObjectType.Campfire:
            case worldObjectType.HotCoals:
                return true;
        }
    }

    public int GetLightIntensity()
    {
        switch (objType)
        {
            default: return 0;
            case WorldObject.worldObjectType.Campfire: return 1;
        }
    }

    public List<Item> GetLootTable()
    {
        switch(objType)
        {
            default:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Null, amount = 1 }
            };
            case worldObjectType.Tree: return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Log, amount = 1 },
                new Item { itemType = Item.ItemType.Log, amount = 1 },
                new Item { itemType = Item.ItemType.Log, amount = 1 },
                new Item { itemType = Item.ItemType.Apple, amount = 1 }
            };
            case worldObjectType.Boulder: return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Rock, amount = 1 },
                new Item { itemType = Item.ItemType.Rock, amount = 1 },
                new Item { itemType = Item.ItemType.Rock, amount = 1 },
                new Item { itemType = Item.ItemType.Rock, amount = 1 }
            };
            case worldObjectType.ClayDeposit:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 }
            };
            case worldObjectType.Campfire:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Charcoal, amount = 1 },
                new Item { itemType = Item.ItemType.Charcoal, amount = 1 }
            };
            case worldObjectType.Kiln:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 },
                new Item { itemType = Item.ItemType.Clay, amount = 1 }
            };
            case worldObjectType.Sapling:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Twig, amount = 1 }
            };
            case worldObjectType.Milkweed:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Fiber, amount = 1 }
            };
            case worldObjectType.BrownShroom:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.BrownShroom, amount = 1 }
            };
            case worldObjectType.WildParsnip:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.WildParsnip, amount = 1 }
            };
            case worldObjectType.WildCarrot:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.WildCarrot, amount = 1 }
            };
            case worldObjectType.MagicalTree:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Log, amount = 1 },
                new Item { itemType = Item.ItemType.Log, amount = 1 },
                new Item { itemType = Item.ItemType.Log, amount = 1 },
                new Item { itemType = Item.ItemType.Log, amount = 1 },
                new Item { itemType = Item.ItemType.Log, amount = 1 },
                new Item { itemType = Item.ItemType.Log, amount = 1 },
                new Item { itemType = Item.ItemType.IronOre, amount = 1 }
            };
            case worldObjectType.BunnyHole:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Clay, amount = 1 }
            };
            case worldObjectType.HotCoals:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Charcoal, amount = 1 }
            };
            case worldObjectType.Pond:
                return new List<Item>()
            {
                new Item { itemType = Item.ItemType.Clay, amount = 1 }
            };
        }
    }

    public int GetMaxActionUses()
    {
        switch (objType)
        {
            default: return 0;
            case worldObjectType.Tree: return 10;
            case worldObjectType.Boulder: return 15;
            case worldObjectType.ClayDeposit: return 5;
            case worldObjectType.Campfire: return 100;
            case worldObjectType.Kiln: return 5;
            case worldObjectType.Sapling: return 1;
            case worldObjectType.Milkweed: return 1;
            case worldObjectType.BrownShroom: return 1;
            case worldObjectType.WildCarrot: return 1;
            case worldObjectType.WildParsnip: return 1;
            case worldObjectType.BunnyHole: return 1;
            case worldObjectType.MagicalTree: return 25;
            case worldObjectType.HotCoals: return 100;
            case worldObjectType.Pond: return 5;
        }
    }
}
