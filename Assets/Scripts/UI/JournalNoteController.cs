using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class JournalNoteController : MonoBehaviour
{
    public static JournalNoteController Instance { get; private set; }

    [SerializeField] private PlayerMain player;
    public GameObject pfJournal;
    public GameObject pfJournalPage;

    [SerializeField] private GameObject pfGeneralPage;
    [SerializeField] private GameObject pfDailyPage;
    [SerializeField] private GameObject pfSmithingPage;
    [SerializeField] private GameObject pfParasitePage;

    [SerializeField] private AudioManager audioManager;

    public List<JournalEntry> entryList = new List<JournalEntry>();
    public List<JournalEntry> existingEntries = new List<JournalEntry>();//just save and load this
    public Transform generalPage;
    public Transform dailyPage;
    public Transform smithingPage;
    public Transform parasitePage;
    private int generalLinesLeft = 12;
    private int generalP2LinesLeft = 12;
    public int generalPageNumber = 1;//page number is how many pages we have. New page generated when we fill up both sides of one page

    private int dailyLinesLeft = 12;
    private int dailyP2LinesLeft = 12;
    public int dailyPageNumber = 1;

    private int smithingLinesLeft = 12;
    private int smithingP2LinesLeft = 12;
    public int smithingPageNumber = 1;

    private int parasiteLinesLeft = 12;
    private int parasiteP2LinesLeft = 12;
    public int parasitePageNumber = 1;

    public void Start()
    {
        Instance = this;
        player.inventory.OnItemListChanged += OnItemCollected;
        UnlockSpecificEntry("Interactions");
        UnlockSpecificEntry("Item Actioning");
        UnlockSpecificEntry("Day1");
        StartCoroutine(CheckForMobs());
    }

    public void UnlockAllEntries()
    {
        foreach(JournalEntry entry in entryList)
        {
            UnlockNewEntry(entry);
        }
    }

    private IEnumerator CheckForMobs()
    {
        yield return new WaitForSeconds(5);
        var list = Physics2D.OverlapCircleAll(player.transform.position, 35);
        foreach(Collider2D col in list)
        {
            if (col.GetComponent<RealMob>() != null)
            {
                OnNewMobSeen(col.GetComponent<RealMob>().mob.mobSO);
            }
        }
        StartCoroutine(CheckForMobs());
    }

    public void UnlockSpecificEntry(string name)
    {
        foreach (JournalEntry entry in entryList)
        {
            if (entry.entryName == name)
            {
                UnlockNewEntry(entry);
            }
        }
    }

    private void OnItemCollected(object sender, System.EventArgs e)
    {
        //check if any entries require

        foreach (JournalEntry entry in entryList)//holy shit this is complicated
        {
            bool entryExists = existingEntries.Any(en => en.entryName == entry.entryName);
            if (entry.needsItems && !entryExists)//only do if needs items
            {
                int itemsRequired = entry.requiredItems.Count;
                int itemsFound = 0;
                for (int i = 0; i < player.inventory.GetItemList().Length; i++)//check all items in player inventory
                {
                    if (player.inventory.GetItemList()[i] != null)//null check for empty slots
                    {
                        foreach (string item in entry.requiredItems)//for each item in inventory, check each item in the list of required items of an entry
                        {
                            if (item == player.inventory.GetItemList()[i].itemSO.itemType)//if that item in entry list equals current item searched, add int. 
                            {
                                itemsFound++;
                                if (itemsFound == itemsRequired)//if we found enough items, unlock!
                                {
                                    Debug.Log("entry unlocked!");
                                    UnlockNewEntry(entry);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnNewMobSeen(MobSO mob)
    {
        //check if we've seen this mob before.
        //check if any entries require mob. Also store this mob in a list that checks that its been seen before.
        foreach (JournalEntry entry in entryList)
        {
            if (entry.needsMobs)
            {
                if (mob.mobType == entry.requiredMobs[0])
                {
                    UnlockNewEntry(entry);
                }
            }
        }
    }

    public void UnlockNewEntry(JournalEntry newEntry, bool addEntry = true)
    {
        //12 lines make up a page
        //one line is 65 pixels

        bool entryExists = existingEntries.Any(en => en.entryName == newEntry.entryName);
        if (entryExists && addEntry)
        {
            Debug.Log("exists already, DIE!");
            return;
        }

        UI_JournalBehavior.Instance.OpenNewEntryNotif();
        int rand = Random.Range(1, 4);
        audioManager.Play($"NewEntry{rand}", player.transform.position, gameObject, true);
        switch (newEntry.page)
        {
            case UI_JournalBehavior.PageType.General:
                if (generalLinesLeft > newEntry.size)//check left page lines left
                {
                    var entry = Instantiate(pfJournal, generalPage);//new textbox
                    entry.transform.parent = generalPage.parent.Find($"generalPage{generalPageNumber}");//set parent to current page number of page type
                    entry.transform.localPosition = new Vector2(-250, 375 + (65 * (generalLinesLeft - 12)));//set position to increment downwards the amount of lines we have left
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;//set text
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;  //set color here when u figure it out UGH!!
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;//set name of entry
                    generalLinesLeft -= newEntry.size;//decrement lines left
                    UI_JournalBehavior.Instance.SetNewEntryPage($"generalPage{generalPageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);//add to discovered entry list
                    }
                }
                else if (generalP2LinesLeft > newEntry.size)
                {
                    var entry = Instantiate(pfJournal, generalPage);
                    entry.transform.parent = generalPage.parent.Find($"generalPage{generalPageNumber}");
                    entry.transform.localPosition = new Vector2(200, 375 + (65 * (generalP2LinesLeft - 12)));
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    generalP2LinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"generalPage{generalPageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                else
                {
                    generalLinesLeft = 12;//reset lines left
                    generalP2LinesLeft = 12;
                    generalPageNumber++;//increment how many pages we have of this type
                    var newPage = Instantiate(pfGeneralPage, generalPage.parent);//make a new page from this page type's prefab
                    newPage.name = $"generalPage{generalPageNumber}";//set name to PAGETYPE NUMBER IE: generalPage2
                    var entry = Instantiate(pfJournal, newPage.transform);
                    newPage.transform.SetSiblingIndex(generalPage.GetSiblingIndex() + generalPageNumber - 1);//set sibling index to be in order for the journalbehavior to find correct next page
                    newPage.SetActive(false);//deactivate since its always an unopened page
                    entry.transform.localPosition = new Vector2(-250, 375);
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    generalLinesLeft -= newEntry.size;//decrement line 1 since we start anew
                    UI_JournalBehavior.Instance.SetNewEntryPage($"generalPage{generalPageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                break;


            case UI_JournalBehavior.PageType.Daily://                                                       -----DAILY PAGE-----
                if (dailyLinesLeft > newEntry.size)
                {
                    var entry = Instantiate(pfJournal, dailyPage);
                    entry.transform.parent = dailyPage.parent.Find($"dailyPage{dailyPageNumber}");
                    entry.transform.localPosition = new Vector2(-250, 375 + (65 * (dailyLinesLeft - 12)));
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    dailyLinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"dailyPage{dailyPageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                else if (dailyP2LinesLeft > newEntry.size)
                {
                    var entry = Instantiate(pfJournal, dailyPage);
                    entry.transform.parent = dailyPage.parent.Find($"dailyPage{dailyPageNumber}");
                    entry.transform.localPosition = new Vector2(200, 375 + (65 * (dailyP2LinesLeft - 12)));
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    dailyP2LinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"dailyPage{dailyPageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                else
                {
                    dailyLinesLeft = 12;
                    dailyP2LinesLeft = 12;
                    dailyPageNumber++;
                    var newPage = Instantiate(pfDailyPage, dailyPage.parent);
                    newPage.name = $"dailyPage{dailyPageNumber}";
                    var entry = Instantiate(pfJournal, newPage.transform);
                    newPage.transform.SetSiblingIndex(dailyPage.GetSiblingIndex() + dailyPageNumber - 1);
                    newPage.SetActive(false);
                    entry.transform.localPosition = new Vector2(-250, 375);
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    dailyLinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"dailyPage{dailyPageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                break;


            case UI_JournalBehavior.PageType.Smithing://                                                   -----SMITHING PAGE-----
                if (smithingLinesLeft > newEntry.size)
                {
                    var entry = Instantiate(pfJournal, smithingPage);
                    entry.transform.parent = smithingPage.parent.Find($"smithingPage{smithingPageNumber}");
                    entry.transform.localPosition = new Vector2(-250, 375 + (65 * (smithingLinesLeft - 12)));
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    smithingLinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"smithingPage{smithingPageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                else if (smithingP2LinesLeft > newEntry.size)
                {
                    var entry = Instantiate(pfJournal, smithingPage);
                    entry.transform.parent = smithingPage.parent.Find($"smithingPage{smithingPageNumber}");
                    entry.transform.localPosition = new Vector2(200, 375 + (65 * (smithingP2LinesLeft - 12)));
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    smithingP2LinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"smithingPage{smithingPageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                else
                {
                    smithingLinesLeft = 12;
                    smithingP2LinesLeft = 12;
                    smithingPageNumber++;
                    var newPage = Instantiate(pfSmithingPage, smithingPage.parent);
                    newPage.name = $"smithingPage{smithingPageNumber}";
                    newPage.transform.SetSiblingIndex(smithingPage.GetSiblingIndex() + smithingPageNumber - 1);//get the index of smithing page, add page number and subtract by one since we start at index 1
                    newPage.SetActive(false);
                    var entry = Instantiate(pfJournal, newPage.transform);
                    entry.transform.localPosition = new Vector2(-250, 375);
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    smithingLinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"smithingPage{smithingPageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                    
                }
                break;
            case UI_JournalBehavior.PageType.Parasite://                                                         -----PARASITE PAGE-----
                if (parasiteLinesLeft > newEntry.size)
                {
                    var entry = Instantiate(pfJournal, parasitePage);
                    entry.transform.parent = parasitePage.parent.Find($"parasitePage{parasitePageNumber}");
                    entry.transform.localPosition = new Vector2(-250, 375 + (65 * (parasiteLinesLeft - 12)));
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    parasiteLinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"parasitePage{parasitePageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                else if (parasiteP2LinesLeft > newEntry.size)
                {
                    var entry = Instantiate(pfJournal, parasitePage);
                    entry.transform.parent = parasitePage.parent.Find($"parasitePage{parasitePageNumber}");
                    entry.transform.localPosition = new Vector2(200, 375 + (65 * (parasiteP2LinesLeft - 12)));
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    parasiteP2LinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"parasitePage{parasitePageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                else
                {
                    parasiteLinesLeft = 12;
                    parasiteP2LinesLeft = 12;
                    parasitePageNumber++;
                    var newPage = Instantiate(pfParasitePage, parasitePage.parent);
                    newPage.name = $"parasitePage{parasitePageNumber}";
                    newPage.transform.SetSiblingIndex(parasitePage.GetSiblingIndex() + parasitePageNumber - 1);
                    newPage.SetActive(false);
                    var entry = Instantiate(pfJournal, newPage.transform);
                    entry.transform.localPosition = new Vector2(-250, 375);
                    entry.GetComponent<TextMeshProUGUI>().text = newEntry.contents;
                    entry.GetComponent<TextMeshProUGUI>().color = Color.red;
                    UI_JournalBehavior.Instance.SetNewEntry(entry.GetComponent<TextMeshProUGUI>());
                    entry.name = newEntry.entryName;
                    parasiteLinesLeft -= newEntry.size;
                    UI_JournalBehavior.Instance.SetNewEntryPage($"parasitePage{parasitePageNumber}");
                    if (addEntry)
                    {
                        existingEntries.Add(newEntry);
                    }
                }
                break;
        }
    }

    public void LoadEntries(List<JournalEntry> newList)
    {
        foreach(JournalEntry entry in newList)
        {
            UnlockNewEntry(entry);
        }
    }
}
