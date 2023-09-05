using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public BiomeType biomeType;

    private Transform player;

    public bool isCellLoaded = false;

    public enum BiomeType
    {
        Forest,
        Savannah,
        Desert,
        Snowy,
        Rocky,
        Grasslands,
        MagicalForest,
        Swamp
    }

    private void OnEnable()
    {
        StartCoroutine(CheckPlayerDistance());
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        isCellLoaded = true;
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

    private void UnloadCell()
    {
        gameObject.SetActive(false);
    }

}
