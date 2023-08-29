using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WosoArray : MonoBehaviour
{
    public static WosoArray Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public Transform pfWorldObject;

    public WOSO Tree;
    public WOSO Boulder;
    public WOSO Kiln;
    public WOSO Campfire;
    public WOSO HotCoals;
    public WOSO ClayDeposit;
    public WOSO Sapling;
    public WOSO Milkweed;
    public WOSO WildParsnip;
    public WOSO WildCarrot;
    public WOSO BrownShroom;
    public WOSO BunnyHole;
    public WOSO MagicalTree;
    public WOSO Pond;
    public WOSO BirchTree;
    public WOSO Wheat;
    public WOSO CypressTree;
    public WOSO Oven;
    public WOSO GoldBoulder;
    public WOSO DirtBeacon;
}
