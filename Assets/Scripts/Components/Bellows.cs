using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bellows : MonoBehaviour
{
    private int maxBellowPower;
    private int bellowPower = 0;
    private bool isOpened = false;
    private bool playerIsSearching = false;
    private GameObject player;
    private GameObject minigame;
    private bool clicked;
    private bool isTiming = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerController>().onMoved += PlayerMoved;
        minigame = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().minigame;
        minigame.SetActive(false);
    }

    private void Update()
    {
        float dist = Vector3.Distance(this.transform.position, player.transform.position);
        if ( dist > 5 && isOpened && !player.GetComponent<Collider2D>().IsTouching(this.GetComponent<Collider2D>()))//prob should have flipped this logic to be more consistent
        {
            CloseMiniGame();
        }
        if (playerIsSearching)
        {
            if (player.GetComponent<Collider2D>().IsTouching(this.GetComponent<Collider2D>()) && !isOpened)
            {
                OpenMiniGame();
                playerIsSearching = false;
                return;
            }
        }
        TrackMouse();
    }

    private void TrackMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 tempPos = mousePos;
        if (mousePos.y >= 0 && mousePos.y <= 200)
        {
            //Debug.Log("Mouse In correct spot");

        }
        else
        {
            //Debug.Log("mouse in WRONG spot");
        }
    }

    public void PlayerMoved(object sender, System.EventArgs e)
    {
        playerIsSearching = false;
    }



    public void OnClicked()
    {
        if (isOpened && !clicked)
        {
            gameObject.GetComponentInParent<Smelter>().Addtemperature(bellowPower);
            clicked = true;
            return;
        }

        if (player.GetComponent<Collider2D>().IsTouching(this.GetComponent<Collider2D>()) && !isOpened)
        {
            OpenMiniGame();
            playerIsSearching = false;
            return;
        }
        playerIsSearching = true;
        player.GetComponent<PlayerController>().target = transform.position;
    }

    private void OpenMiniGame()
    {
        isOpened = true;
        StartCoroutine(Timer());
        minigame.SetActive(true);
        Debug.Log("OPEN!");
    }
    private void CloseMiniGame()
    {
        isOpened = false;
        StopCoroutine(Timer());//THIS EXISTED THE WHOLE TIME?????? BRUH!!!
        isTiming = false;
        clicked = false;
        minigame.SetActive(false);
        Debug.Log("CLOSED");
    }

    private IEnumerator Timer()
    {   if (isOpened && !isTiming)
        {
            isTiming = true;
            bellowPower = 0;
            while (isOpened)
            {
                yield return new WaitForSeconds(.0005f);//half second
                bellowPower++;
                if (bellowPower >= 100)
                {
                    Debug.Log("Bellows are MAX");
                    while (bellowPower > 0)
                    {
                        yield return new WaitForSeconds(.0005f);
                        bellowPower--;
                    }
                    Debug.Log("Bellows are MIN");
                    clicked = false;
                    //StartCoroutine(Timer());
                }
            }
        }
    }

    public void AddTemperature(int _power)
    {
        gameObject.GetComponent<Smelter>().Addtemperature(_power);
    }

    public void SetMaxPower(int _power)
    {
        maxBellowPower = _power;
    }
}
