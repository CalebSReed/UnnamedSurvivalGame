using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_JournalBehavior : MonoBehaviour
{
    public GameObject generalPage;
    public GameObject dailyPage;
    public GameObject smithingPage;
    public GameObject parasitePage;

    public PlayerMain player;
    public JournalNoteController controller;

    private int pageIndex;
    public Transform notesController;

    public enum PageType
    {
        General,
        Daily,
        Smithing,
        Parasite
    }

    private PageType page;

    private void Update()
    {
        if (!GameManager.Instance.journal.activeSelf)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PageLeft();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            PageRight();
        }
    }

    public void DisplayPage()
    {
        switch (page)
        {
            case PageType.General:
                generalPage.SetActive(true);
                dailyPage.SetActive(false);
                smithingPage.SetActive(false);
                parasitePage.SetActive(false);
                break;
            case PageType.Daily:
                generalPage.SetActive(false);
                dailyPage.SetActive(true);
                smithingPage.SetActive(false);
                parasitePage.SetActive(false);
                break;
            case PageType.Smithing:
                generalPage.SetActive(false);
                dailyPage.SetActive(false);
                smithingPage.SetActive(true);
                parasitePage.SetActive(false);
                break;
            case PageType.Parasite:
                generalPage.SetActive(false);
                dailyPage.SetActive(false);
                smithingPage.SetActive(false);
                parasitePage.SetActive(true);
                break;
        }
    }

    public void PageRight()
    {
        if (pageIndex < notesController.childCount - 1)//max pages
        {
            pageIndex++;
            foreach (Transform child in notesController)
            {
                child.gameObject.SetActive(false);
            }
            notesController.GetChild(pageIndex).gameObject.SetActive(true);
        }       
    }

    public void PageLeft()
    {
        if (pageIndex > 0)
        {
            pageIndex--;
            foreach (Transform child in notesController)
            {
                child.gameObject.SetActive(false);
            }
            notesController.GetChild(pageIndex).gameObject.SetActive(true);
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
