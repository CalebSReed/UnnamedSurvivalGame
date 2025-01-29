using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobManager : MonoBehaviour
{
    [SerializeField] Transform player;

    public static MobManager Instance;
    private WaitForSeconds oneSecond = new WaitForSeconds(1);

    private void Awake()
    {
        Instance = this;
        StartCoroutine(CheckMobs());
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        Transform playerToUse = null;
        foreach (var player in GameManager.Instance.playerList)
        {
            if (player.IsServer)
            {
                playerToUse = player.transform;
            }
        }
        player = playerToUse;
    }

    private IEnumerator CheckMobs()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            bool closeEnough = false;

            foreach (var player in GameManager.Instance.playerList)
            {
                if (!child.gameObject.activeSelf && Vector3.Distance(player.transform.position, child.position) <= 100)//might be faster if we store inactive ones in another object? but tbf there should only be like 5+ mobs at once active at a time anyways
                {
                    closeEnough = true;
                }
            }

            if (closeEnough)
            {
                child.gameObject.SetActive(true);
            }
            yield return null;
        }
        yield return null;
        StartCoroutine(CheckMobs());
    }

    public static List<GameObject> GetAllMobs()
    {
        List<GameObject> mobs = new List<GameObject>();
        for (int i = 0; i < Instance.transform.childCount; i++)
        {
            var child = Instance.transform.GetChild(i);
            mobs.Add(child.gameObject);
        }
        return mobs;
    }
}
