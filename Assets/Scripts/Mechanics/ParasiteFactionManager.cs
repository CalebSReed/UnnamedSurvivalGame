using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParasiteFactionManager : MonoBehaviour//SAVE EVERYTHING HERE!!!
{
    public static ParasiteFactionManager Instance { get; private set; }


    public Vector3 PlayerBase { get; set; }
    public bool ParasiteBaseExists { get; set; }
    public int ParasiteTechLevel { get; set; }

    public List<GameObject> researchedObjectList = new List<GameObject>();
    private void Awake()
    {
        Instance = this;
        PlayerBase = Vector3.zero;
    }

    private void StartParasiteRaid()
    {

    }
}
