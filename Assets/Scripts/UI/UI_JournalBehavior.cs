using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_JournalBehavior : MonoBehaviour
{
    public static UI_JournalBehavior Instance { get; private set; }

    public GameObject generalPage;
    public GameObject dailyPage;
    public GameObject smithingPage;
    public GameObject parasitePage;
    [SerializeField] private Image doodleImage;

    public PlayerMain player;
    public JournalNoteController controller;

    private int pageIndex;
    public Transform notesController;
    public GameObject NewEntryNotif;
    public TextMeshProUGUI newEntry;

    private string newEntryPage;
    public bool entrySeen;

    public enum PageType
    {
        General,
        Daily,
        Smithing,
        Parasite
    }

    private PageType page;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!GameManager.Instance.journal.activeSelf)
        {
            return;
        }

        /*if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PageLeft();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            PageRight();
        }*/
    }

    public void SetNewEntryPage(string n)//so that journal will open jump to new entry if we click the notif.
    {
        newEntryPage = n;
    }

    public void OpenNewEntryNotif()
    {
        NewEntryNotif.GetComponent<Animator>().SetBool("isOpen", true);
    }

    public void CloseNewEntryNotif()
    {
        if (!GameManager.Instance.journal.activeSelf)
        {
            ClearNotification();
            DisplaySpecificPage(newEntryPage);
            GameManager.Instance.ToggleJournal();
        }
        else
        {
            ClearNotification();
            DisplaySpecificPage(newEntryPage);
        }
    }

    public void ClearNotification()
    {
        NewEntryNotif.GetComponent<Animator>().SetBool("isOpen", false);
    }

    public void DisplaySpecificPage(string pageName)
    {
        Resetpages();
        pageIndex = notesController.Find(pageName).GetSiblingIndex();
        notesController.Find(pageName).gameObject.SetActive(true);
    }

    private void Resetpages()
    {
        foreach (Transform child in notesController)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void CheckIfNewEntrySeen()
    {
        if (notesController.GetChild(pageIndex).name == newEntryPage)
        {
            entrySeen = true;
            ClearNotification();
        }
    }

    public void PageRight()
    {
        if (pageIndex < notesController.childCount - 1)//max pages
        {
            pageIndex++;
            Resetpages();
            notesController.GetChild(pageIndex).gameObject.SetActive(true);
            CheckIfNewEntrySeen();
            ChangeDoodles();
        }       
    }

    public void PageLeft()
    {
        if (pageIndex > 0)
        {
            pageIndex--;
            Resetpages();
            notesController.GetChild(pageIndex).gameObject.SetActive(true);
            CheckIfNewEntrySeen();
            ChangeDoodles();
        }
    }

    public void SetNewEntry(TextMeshProUGUI newEntry)
    {
        if (this.newEntry != null)
        {
            this.newEntry.color = Color.black;
        }
        
        this.newEntry = newEntry;
    }

    public void ChangeDoodles()
    {
        int rand = Random.Range(0, 2);

        if (rand == 0)
        {
            doodleImage.sprite = SceneReferences.Instance.journalDoodles1;
        }
        else
        {
            doodleImage.sprite = SceneReferences.Instance.journalDoodles2;
        }
    }

    //list of item actions
    //at awake we make a list of all items that do an action other than default. Scroll thru all items list
    //then we make a new list of all items that can get actioned with stuff other than default
    //then we check each list if they match eachother. 
    //next we instantiate that list, with pics of the actioner, the actionee, and the reward.
    //actionee and reward only shown if this recipe has been performed before.
    //when enough are generated, go to next page and start at top
    //if that is filled, generate a new page start at top. pretty easy morty...
}
