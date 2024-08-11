using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdrenalineManager : MonoBehaviour
{
    [SerializeField] public bool adrenalineReady;
    [SerializeField] public float adrenalineProgress;
    [SerializeField] private float maxAdrenaline = 24 * 60;
    [SerializeField] private GameObject adrenalineIcon;
    [SerializeField] private GameObject adrenalineVignette;
    [SerializeField] private GameObject tuckeredOutVignette;
    public bool inAdrenalineMode;
    public bool inSlowMode;
    private float adrenalineDuration = 15;
    public float adrenalineCountdown;
    private bool stopCoroutines;
    //save adrenaline progress, countdown, bool if currently juiced

    private void Start()
    {
        adrenalineReady = true;
        adrenalineProgress = maxAdrenaline;
    }
    private void Update()
    {
        if (!adrenalineReady)
        {
            adrenalineProgress += Time.deltaTime;
            if (adrenalineProgress >= maxAdrenaline)
            {
                adrenalineProgress = maxAdrenaline;
                adrenalineReady = true;
                //adrenalineOutline.SetActive(true);
                adrenalineIcon.SetActive(true);
                if (stopCoroutines)
                {
                    stopCoroutines = false;
                }
            }
        }
    }

    public void EndAdrenalinePrematurely()
    {
        stopCoroutines = true;
    }

    public IEnumerator StartAdrenaline()
    {
        inAdrenalineMode = true;
        ResetAdrenaline();
        adrenalineVignette.SetActive(true);
        GetComponent<PlayerMain>().speedMult += 1f;

        adrenalineCountdown = adrenalineDuration;
        while (adrenalineCountdown > 0 && stopCoroutines == false)
        {
            adrenalineCountdown -= Time.deltaTime;
            yield return null;
        }
        GetComponent<PlayerMain>().speedMult -= 1f;
        ResetAdrenaline();
        inAdrenalineMode = false;
        adrenalineVignette.SetActive(false);
        StartCoroutine(LeaveAdrenaline());
    }

    public IEnumerator LeaveAdrenaline()
    {
        tuckeredOutVignette.SetActive(true);
        inSlowMode = true;
        GetComponent<PlayerMain>().speedMult -= .5f;
        adrenalineCountdown = adrenalineDuration * 2;
        while (adrenalineCountdown > 0 && stopCoroutines == false)
        {
            adrenalineCountdown -= Time.deltaTime;
            yield return null;
        }
        GetComponent<PlayerMain>().speedMult += .5f;
        inSlowMode = false;
        tuckeredOutVignette.SetActive(false);
        if (stopCoroutines)
        {
            stopCoroutines = false;
        }
    }

    public void ResetAdrenaline()
    {
        GetComponent<HealthManager>().adrenalineInvincible = false;
        //adrenalineOutline.SetActive(false);
        adrenalineIcon.SetActive(false);
        adrenalineVignette.SetActive(false);
        tuckeredOutVignette.SetActive(false);
        adrenalineProgress = 0f;
        adrenalineReady = false;
    }
}
