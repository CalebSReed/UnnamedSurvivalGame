using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bellows : MonoBehaviour
{
    private int maxBellowPower;
    private int bellowPower;
    private bool isOpened = false;
    private bool playerIsSearching = false;
    private GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerController>().onMoved += PlayerMoved;
    }

    private void Update()
    {
        float dist = Vector3.Distance(this.transform.position, player.transform.position);
        if ( dist > 25 && isOpened)
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
    }

    public void PlayerMoved(object sender, System.EventArgs e)
    {
        playerIsSearching = false;
    }

    public void OnClicked()
    {
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
        Debug.Log("OPEN!");
    }
    private void CloseMiniGame()
    {
        isOpened = false;
        Debug.Log("CLOSED");
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
