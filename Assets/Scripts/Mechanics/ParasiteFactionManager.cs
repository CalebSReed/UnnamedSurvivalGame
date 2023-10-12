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

    private bool checkingPlayerLocation;

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
        if (PlayerBaseExists && !checkingPlayerLocation)
        {
            StartCoroutine(CheckPlayerLocation());
        }
        else
        {
            SpawnScouters();
        }
    }

    private IEnumerator CheckPlayerLocation()
    {
        checkingPlayerLocation = true;
        yield return new WaitForSeconds(10);
        if (Vector3.Distance(PlayerBase, player.transform.position) < 100)
        {
            StartParasiteRaid();
            checkingPlayerLocation = false;
            yield break;
        }
        StartCoroutine(CheckPlayerLocation());
    }

    public static void StartParasiteRaid()
    {
        Debug.Log("Parasite raid started");
        Vector3 _newPos = Vector3.zero;
        _newPos = CalebUtils.RandomPositionInRadius(Instance.PlayerBase, 250, 500);//change later
        var mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob.GetComponent<MobMovementBase>().wanderTarget = Instance.PlayerBase;
        mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);

        _newPos = CalebUtils.RandomPositionInRadius(Instance.PlayerBase, 250, 500);//change later
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        mob.GetComponent<MobMovementBase>().wanderTarget = Instance.PlayerBase;
        mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);

        _newPos = CalebUtils.RandomPositionInRadius(Instance.PlayerBase, 250, 500);//change later
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob.GetComponent<MobMovementBase>().wanderTarget = Instance.PlayerBase;
        mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);

        _newPos = CalebUtils.RandomPositionInRadius(Instance.PlayerBase, 250, 500);//change later
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        mob.GetComponent<MobMovementBase>().wanderTarget = Instance.PlayerBase;
        mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);

        _newPos = CalebUtils.RandomPositionInRadius(Instance.PlayerBase, 250, 500);//change later
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });
        mob.GetComponent<MobMovementBase>().wanderTarget = Instance.PlayerBase;
        mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);
    }

    private void SpawnScouters()
    {
        Debug.Log("spawned scouters");
        Vector3 _newPos = Vector3.zero;
        _newPos = CalebUtils.RandomPositionInRadius(player.transform.position, 500, 1000);//change later
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });

        _newPos = Vector3.zero;
        _newPos = CalebUtils.RandomPositionInRadius(player.transform.position, 500, 1000);
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });
    }
}
