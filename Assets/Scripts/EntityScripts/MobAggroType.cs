using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobAggroType
{
    public enum AggroType
    {
        Passive,
        Neutral,//attacks on hit
        PassiveNeutral,//flees on hit
        Aggressive
        //special   for AI that will override everything pretty much
    }
}
