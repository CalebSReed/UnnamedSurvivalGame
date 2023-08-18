using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public BiomeType biomeType;

    public enum BiomeType
    {
        Forest,
        Savannah,
        Desert,
        Snowy,
        Rocky,
        Grasslands,
        MagicalForest
    }
}
