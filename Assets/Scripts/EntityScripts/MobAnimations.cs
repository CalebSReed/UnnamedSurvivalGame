using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAnimations : MonoBehaviour
{
    public static MobAnimations Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public RuntimeAnimatorController parasiteScouterAnim;
}
