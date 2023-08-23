using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxAssets : MonoBehaviour
{
    public static HitBoxAssets Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public Collider2D treeCollider;
}
