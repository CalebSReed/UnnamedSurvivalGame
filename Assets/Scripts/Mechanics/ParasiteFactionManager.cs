using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParasiteFactionManager : MonoBehaviour//SAVE EVERYTHING HERE!!!
{
    public static ParasiteFactionManager Instance { get; private set; }
    public static ParasiteFactionData parasiteData;
    public GameObject player { get; set; }//ooh if all parasites use this, if player somehow tricks parasites they will all be tricked :o 

    public Transform ParasiteBaseLvl1;

    [SerializeField] private AudioManager audio;

    public List<GameObject> researchedObjectList = new List<GameObject>();

    [SerializeField] private RaidProgress raidSlider;

    private List<Vector2Int> listToCorrupt = new List<Vector2Int>();

    private List<Vector2Int> checkedTiles = new List<Vector2Int>();

    private void Awake()
    {
        Instance = this;
        parasiteData = new ParasiteFactionData();
        parasiteData.PlayerBase = Vector3.zero;
        parasiteData.PlayerBaseExists = false;
        player = GameObject.FindGameObjectWithTag("Player");
        DayNightCycle.Instance.OnDawn += DoDawnTasks;

    }

    private void Start()
    {
        GameManager.Instance.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    private void OnLocalPlayerSpawned(object sender, System.EventArgs e)
    {
        player = GameManager.Instance.localPlayer;
    }

    private void DoDawnTasks(object sender, System.EventArgs e)
    {       
        if (DayNightCycle.Instance.currentDay < 5 || !GameManager.Instance.isServer)
        {
            return;
        }

        if (parasiteData.ParasiteBaseExists)
        {
            SpreadParasiteBiome();
            SpreadParasiteBiome();
            SpreadParasiteBiome();
            if (GameManager.Instance.difficulty == GameManager.DifficultyOptions.hardcore)
            {
                Debug.Log("Spreading faster due to hardcore!");
                SpreadParasiteBiome();
                SpreadParasiteBiome();
                SpreadParasiteBiome();
            }
        }

        if (parasiteData.PlayerBaseExists && parasiteData.ParasiteBaseExists && !parasiteData.checkingPlayerLocation)
        {
            CheckToStartRaid();
        }
        else if (!parasiteData.PlayerBaseExists || parasiteData.checkingPlayerLocation)//If we are STILL checking for player by dawn parasites should assume they got the base location wrong and check again
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
        Debug.Log("base spawned!");
        Vector3 _newPos = CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 1000, 2000);
        Instantiate(ParasiteBaseLvl1, _newPos, Quaternion.identity);
        parasiteData.ParasiteBase = _newPos;
        parasiteData.ParasiteBaseExists = true;
        _newPos.z += 25;
        var mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });
        mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("reinforcement") });

        RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("parasiticheartlvl1") });

        SpreadParasiteBiome();
        SpreadParasiteBiome();
        SpreadParasiteBiome();
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
        parasiteData.isRaidInProgress = true;

        Debug.Log($"Parasite raid started at {parasiteData.PlayerBase}");
        Instance.audio.Play("ParasiteWaveStinger", Instance.transform.position, Instance.gameObject);
        Vector3 _newPos = Vector3.zero;
        RealMob mob = null;

        foreach (var player in GameManager.Instance.playerList)//spawn for each player
        {
            _newPos = CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 400);//change later
            mob = RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Skirmisher") });
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
        }

        foreach (var player in GameManager.Instance.playerList)
        {
            int _randnum = Random.Range(0, 3);
            if (_randnum == 0)
            {
                RealMob.SpawnMob(CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 500), new Mob { mobSO = MobObjArray.Instance.SearchMobList("destroyer") });
            }
            else if (_randnum == 1)
            {
                RealMob.SpawnMob(CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 500), new Mob { mobSO = MobObjArray.Instance.SearchMobList("mercenary") });
            }
            else
            {
                RealMob.SpawnMob(CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 500), new Mob { mobSO = MobObjArray.Instance.SearchMobList("ravager") });
            }
        }


        int difficulty = parasiteData.raidDifficultyMult;
        int max = 25;
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

            if (parasiteData.raidDifficultyMult > 5 && difficulty > 1)
            {
                int _randnum = 0;
                _randnum = Random.Range(0, 3);
                foreach (var player in GameManager.Instance.playerList)
                {
                    if (_randnum == 0)
                    {
                        RealMob.SpawnMob(CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 500), new Mob { mobSO = MobObjArray.Instance.SearchMobList("destroyer") });
                    }
                    else if (_randnum == 1)
                    {
                        RealMob.SpawnMob(CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 500), new Mob { mobSO = MobObjArray.Instance.SearchMobList("mercenary") });
                    }
                    else
                    {
                        RealMob.SpawnMob(CalebUtils.RandomPositionInRadius(parasiteData.PlayerBase, 250, 500), new Mob { mobSO = MobObjArray.Instance.SearchMobList("ravager") });
                    }
                }
                difficulty -= 2;
                max -= 2;
            }
        }

        Instance.raidSlider.SetMaxRaidHealth();
        Instance.StartCoroutine(Instance.raidSlider.TrackRaidProgress());
        Announcer.SetText("INCOMING RAID!!!", Color.magenta);

        Instance.StartCoroutine(Instance.AnnounceRaidProgress());

        Instance.ResetRaidParameters();
    }

    private IEnumerator AnnounceRaidProgress()
    {
        yield return new WaitForSeconds(3);
        Announcer.SetText("RAID PROGRESS", Color.magenta, true);
    }

    private void ResetRaidParameters()
    {
        parasiteData.raidCooldown = 3;
        parasiteData.raidDifficultyMult += 2;
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
        if (count > 4 * GameManager.Instance.playerList.Count)
        {
            Debug.Log("Dont spawn scouters, too many!!");
            return;
        }
        Debug.Log("spawned scouters");
        Vector3 _newPos = Vector3.zero;
        foreach (var player in GameManager.Instance.playerList)
        {
            _newPos = CalebUtils.RandomPositionInRadius(player.transform.position, 500, 1000);//change later
            RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });

            _newPos = CalebUtils.RandomPositionInRadius(player.transform.position, 500, 1000);
            RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });
            Debug.Log("2 scouters spawned!");
        }


        int difficulty = parasiteData.scouterDifficultyMult;
        while (difficulty > 0)//spawn a new scouter per player then decrement difficulty modifier
        {
            foreach(var player in GameManager.Instance.playerList)
            {
                _newPos = CalebUtils.RandomPositionInRadius(player.transform.position, 500, 1000);//change later
                RealMob.SpawnMob(_newPos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("Scouter") });
                Debug.Log("Additional scouter spawned!");
            }
            difficulty--;
        }
        parasiteData.scouterDifficultyMult++;
    }

    public void LoadParasiteData()
    {
        if (parasiteData.isRaidInProgress)
        {
            Instance.StartCoroutine(Instance.raidSlider.TrackRaidProgress());
            StartCoroutine(AnnounceRaidProgress());
        }
    }

    public void GoHomeScouters()
    {
        foreach (var parasite in GetAllParasites())
        {
            if (parasite.GetComponent<RealMob>().mob.mobSO.isScouter)
            {
                parasite.GetComponent<ParasiteScouterAI>().readyToGoHome = true;
            }
        }
    }

    public void SpreadParasiteBiome()
    {
        var startLocation = new Vector2Int(Mathf.RoundToInt(parasiteData.ParasiteBase.x / 25) + WorldGeneration.Instance.worldSize, Mathf.RoundToInt(parasiteData.ParasiteBase.z / 25) + WorldGeneration.Instance.worldSize);

        startLocation.y += 1;

        //var startLocation = new Vector2Int(WorldGeneration.Instance.worldSize + 25, WorldGeneration.Instance.worldSize);

        CorruptTile(startLocation);

        CheckNeighborTiles(startLocation);

        foreach (var tilePos in listToCorrupt)
        {
            CorruptTile(tilePos);
        }

        listToCorrupt.Clear();
        checkedTiles.Clear();
    }

    private void CheckNeighborTiles(Vector2Int pos)
    {
        CheckTile(new Vector2Int(pos.x+1, pos.y));
        CheckTile(new Vector2Int(pos.x, pos.y+1));
        CheckTile(new Vector2Int(pos.x-1, pos.y));
        CheckTile(new Vector2Int(pos.x, pos.y-1));
    }

    private void CheckTile(Vector2Int pos)
    {
        if (listToCorrupt.Contains(pos) || checkedTiles.Contains(pos))
        {
            return;
        }

        if (WorldGeneration.Instance.existingTileDictionary.ContainsKey(pos) && WorldGeneration.Instance.existingTileDictionary[pos].GetComponent<Cell>().biomeType != Cell.BiomeType.Parasitic)//if tile is active
        {
            listToCorrupt.Add(pos);
        }
        else if (WorldGeneration.Instance.tileDataDict.ContainsKey(pos) && WorldGeneration.Instance.tileDataDict[pos].biomeType != Cell.BiomeType.Parasitic)//if tile is deactivated or not generated yet
        {
            listToCorrupt.Add(pos);
        }
        else if (WorldGeneration.Instance.tileDataDict.ContainsKey(pos) && WorldGeneration.Instance.tileDataDict[pos].biomeType == Cell.BiomeType.Parasitic || //if is parasitic, check new tiles
            WorldGeneration.Instance.existingTileDictionary.ContainsKey(pos) && WorldGeneration.Instance.existingTileDictionary[pos].GetComponent<Cell>().biomeType == Cell.BiomeType.Parasitic) 
        {
            Debug.Log("Checking more");
            checkedTiles.Add(pos);
            CheckNeighborTiles(pos);
        }
        else//if tile has never been generated before
        {
            listToCorrupt.Add(pos);
        }
    }


    private void CorruptTile(Vector2Int pos)
    {
        if (WorldGeneration.Instance.existingTileDictionary.ContainsKey(pos))//if tile is active or deactive
        {
            if (WorldGeneration.Instance.existingTileDictionary[pos].GetComponent<Cell>().biomeType == Cell.BiomeType.Parasitic)
            {
                return;
            }

            Cell cell = WorldGeneration.Instance.existingTileDictionary[pos].GetComponent<Cell>();
            cell.BecomeParasitic();
        }
        else if (WorldGeneration.Instance.tileDataDict.ContainsKey(pos))//if tile is not generated yet
        {
            if (WorldGeneration.Instance.tileDataDict[pos].biomeType == Cell.BiomeType.Parasitic)
            {
                return;
            }

            WorldGeneration.Instance.tileDataDict[pos].biomeType = Cell.BiomeType.Parasitic;
        }
        else//if tile has never been generated before
        {
            var groundTile = Instantiate(WorldGeneration.Instance.groundTileObject, new Vector3((pos.x - WorldGeneration.Instance.worldSize) * 25, 0, (pos.y - WorldGeneration.Instance.worldSize) * 25), Quaternion.identity);
            var cell = groundTile.GetComponent<Cell>();
            groundTile.transform.rotation = Quaternion.LookRotation(Vector3.down);

            WorldGeneration.Instance.existingTileDictionary.Add(pos, groundTile);
            WorldGeneration.Instance.TileDataList.Add(cell.tileData);
            WorldGeneration.Instance.TileObjList.Add(groundTile);
            //cell.tileData = new TileData();
            cell.tileData.tileLocation = pos;
            cell.tileData.biomeType = cell.biomeType;
            cell.tileData.tileLocation = pos;
            cell.BecomeParasitic();
        }
    }

    private void SpawnWanderingParasite(Vector3 pos)
    {
        var rand = Random.Range(0, 3);

        if (rand == 0)
        {
            SpawnRandomParasite(pos);
        }
    }

    private void SpawnRandomParasite(Vector3 pos)
    {
        var rand = Random.Range(0, 5);

        if (rand == 0)
        {
            RealMob.SpawnMob(pos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("") });
        }
        else if (rand == 1)
        {
            RealMob.SpawnMob(pos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("") });
        }
        else if (rand == 2)
        {
            RealMob.SpawnMob(pos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("") });
        }
        else if (rand == 3)
        {
            RealMob.SpawnMob(pos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("") });
        }
        else if (rand == 4)
        {
            RealMob.SpawnMob(pos, new Mob { mobSO = MobObjArray.Instance.SearchMobList("") });
        }
    }

    public static List<GameObject> GetAllParasites()
    {
        var mobList = MobManager.GetAllMobs();
        List<GameObject> parasiteList = new List<GameObject>();
        foreach (var mob in mobList)
        {
            if (mob.GetComponent<RealMob>().mob.mobSO.isParasite)
            {
                parasiteList.Add(mob);
            }
        }
        return parasiteList;
    }
}
