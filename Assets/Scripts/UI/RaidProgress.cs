using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaidProgress : MonoBehaviour
{
    public Slider raidProgress;
    public IEnumerator TrackRaidProgress()
    {
        raidProgress.gameObject.SetActive(true);
        raidProgress.value = CalculateRaidProgress();
        yield return null;

        if (raidProgress.value <= 0)
        {
            EndRaid();
        }
        else
        {
            StartCoroutine(TrackRaidProgress());
        }
    }

    public void SetMaxRaidHealth()
    {
        ParasiteFactionManager.parasiteData.maxRaidHealth = 0;
        foreach (var parasite in ParasiteFactionManager.GetAllParasites())
        {
            if (parasite.GetComponent<RealMob>().mob.mobSO.isRaidParasite)
            {
                ParasiteFactionManager.parasiteData.maxRaidHealth += parasite.GetComponent<HealthManager>().currentHealth;
            }
        }
    }

    private float CalculateRaidProgress()
    {
        float _currentRaidHealth = 0;
        foreach (var parasite in ParasiteFactionManager.GetAllParasites())
        {
            if (parasite.GetComponent<RealMob>().mob.mobSO.isRaidParasite)
            {
                _currentRaidHealth += parasite.GetComponent<HealthManager>().currentHealth;
            }
        }

        return _currentRaidHealth / ParasiteFactionManager.parasiteData.maxRaidHealth;
    }

    private void EndRaid()
    {
        raidProgress.gameObject.SetActive(false);
        ParasiteFactionManager.parasiteData.isRaidInProgress = false;
        Announcer.RemoveLock();
        JournalNoteController.Instance.UnlockSpecificEntry("Raid");
    }
}
