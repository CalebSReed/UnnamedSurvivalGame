using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectData
{
    public string objType;
    public float actionsLeft;
    public float currentHealth;
    public Vector3 pos;
    public Vector2Int dictKey;

    public List<string> invItemTypes = new List<string>();
    public List<int> invItemAmounts = new List<int>();
    public List<int> invItemUses = new List<int>();
    public List<int> invItemAmmos = new List<int>();

    public string heldItemType;
    public int timerProgress;
    public bool isOpen;
    public int currentFuel;
    public float currentTemp;
    public int temperatureTarget;

    public int currentInhabitants;
}
