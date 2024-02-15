using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public BiomeType biomeType;

    public TileData tileData = new TileData();

    private Transform player;

    public bool isCellLoaded = false;

    [SerializeField] private bool isParasitic;

    public enum BiomeType
    {
        Forest,
        Savannah,
        Desert,
        Snowy,
        Rocky,
        Grasslands,
        MagicalForest,
        Swamp,
        Deciduous,
        Parasitic
    }

    private void OnEnable()
    {
        StartCoroutine(CheckPlayerDistance());
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        isCellLoaded = true;

        if (!isParasitic && biomeType == BiomeType.Parasitic)
        {
            Debug.Log("Bruh");
            BecomeParasitic();
        }
    }

    private IEnumerator CheckPlayerDistance()
    {
        yield return new WaitForSeconds(1f);
        if (Vector3.Distance(transform.position, player.position) > 200)
        {
            UnloadCell();
        }
        else
        {
            StartCoroutine(CheckPlayerDistance());
        }
    }

    public void BecomeParasitic()
    {
        if (isParasitic)
        {
            return;
        }

        isParasitic = true;
        biomeType = BiomeType.Parasitic;
        tileData.biomeType = BiomeType.Parasitic;
        WorldGeneration.Instance.SetTileSprite(GetComponent<SpriteRenderer>(), biomeType);

        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).CompareTag("Tile"))
            {
                if (transform.GetChild(i).GetComponent<RealWorldObject>() != null)
                {
                    transform.GetChild(i).GetComponent<RealWorldObject>().Break(true);
                }
                else if (transform.GetChild(i).GetComponent<RealItem>() != null)
                {
                    transform.GetChild(i).GetComponent<RealItem>().DestroySelf();
                }
            }   
        }

        Debug.Log("Parasite biome!!!");
    }

    private void UnloadCell()
    {
        gameObject.SetActive(false);
    }

}
