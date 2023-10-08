using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DamageArgs : EventArgs
{
    public string damageSenderTag;
    public GameObject senderObject;
    //public DamageType dmgType;
}
