using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParasiteFactionManager : MonoBehaviour//SAVE EVERYTHING HERE!!!
{
    public static ParasiteFactionManager Instance { get; private set; }
    public static ParasiteFactionData parasiteData = new ParasiteFactionData();
    public GameObject player { get; set; }//ooh if all parasites use this, if player somehow tricks parasites they will all be tricked :o 

    public Transform ParasiteBaseLvl1;

    private AudioManager audio;

    public List<GameObject> researchedObjectList = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        parasiteData.PlayerBase = Vector3.zero;
        parasiteData.PlayerBaseExists = false;
        player = GameObject.FindGameObjectWithTag("Player");
        audio = Instance.GetComponent<AudioManager>();
        DayNightCycle.Instance.OnDawn += DoDawnTasks;
    }

    private void DoDawnTasks(object sender, System.EventArgs e)
    {
        if (DayNightCycle.Instance.currentDay == 1)//temp but lets give testers a day to prepare
        {
            return;
        }

        if (parasiteData.PlayerBaseExists && parasiteData.ParasiteBaseExists && !parasiteData.checkingPlayerLocation)
        {
            CheckToStartRaid();
        }
        else if (!parasiteData.PlayerBaseExists)
        {
            SpawnScouters();
        }
        else if (parasiteData.PlayerBaseExists && !parasiteData.ParasiteBaseExists)
        {
            SpawnNewParasiteBase();
        }
    }

    public void SpawnNewParasiteBase()
    {
        Vector3 _newPos = CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 1000, 2000);
        Instantiate(ParasiteBaseLvl1, _newPos, Quaternion.identity);
        parasiteData.ParasiteBase = _newPos;
        parasiteData.ParasiteBaseExists = true;
        _newPos.y += 25;
        var mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        _newPos.x += 5;
        RealItem.SpawnRealItem(_newPos, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("decimator"), amount = 1 });
        _newPos.x -= 10;
        RealItem.SpawnRealItem(_newPos, new Item { itemSO = ItemObjectArray.Instance.SearchItemList("hardenedchestplate"), amount = 1 });
    }

    private void CheckToStartRaid()
    {
        if (parasiteData.raidCooldown > 0)
        {
            parasiteData.raidCooldown--;
        }
        else
        {
            StartCoroutine(CheckPlayerLocation());
        }
    }

    public IEnumerator CheckPlayerLocation()
    {
        parasiteData.checkingPlayerLocation = true;
        yield return new WaitForSeconds(10);
        if (Vector3.Distance(parasiteData.PlayerBase, player.transform.position) < 100)
        {
            StartParasiteRaid();
            parasiteData.checkingPlayerLocation = false;
            yield break;
        }
        StartCoroutine(CheckPlayerLocation());
    }

    public static void StartParasiteRaid()
    {
        Debug.Log($"Parasite raid started at {parasiteData.PlayerBase}");
        Instance.audio.Play("ParasiteWaveStinger", Instance.transform.position, Instance.gameObject);
        Vector3 _newPos = Vector3.zero;
        _newPos = CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 400);//change later
        var mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);
        mob.GetComponent<MobMovementBase>().wanderTarget = parasiteData.PlayerBase;

        _newPos = CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 400);//change later
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);
        mob.GetComponent<MobMovementBase>().wanderTarget = parasiteData.PlayerBase;

        _newPos = CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 400);//change later
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
        mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);
        mob.GetComponent<MobMovementBase>().wanderTarget = parasiteData.PlayerBase;

        _newPos = CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 400);//change later
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
        mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);
        mob.GetComponent<MobMovementBase>().wanderTarget = parasiteData.PlayerBase;

        int difficulty = parasiteData.raidDifficultyMult;
        int max = 15;
        while (difficulty > 0 && max > 0)
        {
            int _rand = Random.Range(0, 2);
            _newPos = CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 500);//change later
            if (_rand == 0)
            {
                mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Soldier") });
            }
            else
            {
                mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
            }          
            mob.GetComponent<MobMovementBase>().SwitchMovement(MobMovementBase.MovementOption.MoveTowards);
            mob.GetComponent<MobMovementBase>().wanderTarget = parasiteData.PlayerBase;
            difficulty--;
            max--;
        }

        Instance.ResetRaidParameters();
    }

    private void ResetRaidParameters()
    {
        parasiteData.raidCooldown = 3;
        parasiteData.raidDifficultyMult += 3;
    }

    private void SpawnScouters()
    {
        int count = 0;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Mob"))
        {
            if (obj.GetComponent<RealMob>().mob.mobSO.isScouter)
            {
                count++;
            }
        }
        if (count > 4)
        {
            Debug.Log("Dont spawn scouters, too many!!");
            return;
        }
        Debug.Log("spawned scouters");
        Vector3 _newPos = Vector3.zero;
        _newPos = CalebUtils.RandomPositionInRadius(player.transform.position, 500, 1000);//change later
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });

        _newPos = CalebUtils.RandomPositionInRadius(player.transform.position, 500, 1000);
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });

        int difficulty = parasiteData.scouterDifficultyMult;
        while (difficulty > 0)
        {
            _newPos = CalebUtils.RandomPositionInRadius(player.transform.position, 500, 1000);//change later
            RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });
            difficulty--;
        }
        parasiteData.scouterDifficultyMult++;
    }
}
