using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParasiteFactionManager : MonoBehaviour//SAVE EVERYTHING HERE!!!
{
    public static ParasiteFactionManager Instance { get; private set; }

    public GameObject player { get; set; }//ooh if all parasites use this, if player somehow tricks parasites they will all be tricked :o 
    public Vector3 PlayerBase { get; set; }
    public bool PlayerBaseExists { get; set; }
    public bool ParasiteBaseExists { get; set; }
    public int ParasiteTechLevel { get; set; }

    public List<GameObject> researchedObjectList = new List<GameObject>();
    private void Awake()
    {
        Instance = this;
        PlayerBase = Vector3.zero;
        PlayerBaseExists = false;
        player = GameObject.FindGameObjectWithTag("Player");
        DayNightCycle.Instance.OnDawn += DoDawnTasks;
    }

    private void DoDawnTasks(object sender, System.EventArgs e)
    {
        SpawnScouters();
    }

    private void StartParasiteRaid()
    {

    }

    private void SpawnScouters()
    {
        Vector3 _newPos = Vector3.zero;
        _newPos = CalebUtils.RandomPositionInRadius(_newPos, 500, 1000);//change later
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });

        _newPos = Vector3.zero;
        _newPos = CalebUtils.RandomPositionInRadius(_newPos, 500, 1000);
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });
    }
}
