using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject_Assets : MonoBehaviour
{
    public static WorldObject_Assets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Transform pfWorldObjectSpawner;

    public Sprite tree;
    public Sprite clayDeposit;
    public Sprite boulder;
    public Sprite campFire;
    public Sprite kiln;
    public Sprite kilnFueled;
    public Sprite kilnLit;
    public Sprite kilnCovered;
    public Sprite sapling;
    public Sprite milkweed;
    public Sprite brownShroom;
    public Sprite wildParsnip;
    public Sprite wildCarrot;
    public Sprite bunnyHole;
    public Sprite magicalTree;
    public Sprite hotCoals;
    public Sprite pond;
    public Sprite kilnSmelting;
    public Sprite bellowAttachment;
    public Sprite bellow2Attachment;
    public Sprite tanningskin;
    public Sprite kilnlvl2;
    public Sprite brickkilnlvl2;
    public Sprite brickkilnlvl3;
}
