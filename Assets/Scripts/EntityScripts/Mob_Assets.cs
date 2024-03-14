using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mob_Assets : MonoBehaviour
{
    public static Mob_Assets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Transform pfMobSpawner;

    public Sprite bunny;
    public Sprite wolf;
    public Sprite turkey;
    public Sprite eyeris;
    public GameObject summonEffect;
}
