using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouseInWorld : MonoBehaviour
{
    public bool snapGrid;

    private PlayerMain player;

    private void Start()
    {
        //GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        //player = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
    }

    private void OnEnable()
    {
        player = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
    }

    private void Update()
    {
        Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());//this might cause bugs calling in physics update
        RaycastHit[] rayHits = Physics.RaycastAll(ray, Mathf.Infinity, GameManager.Instance.tileMask);

        foreach (var rayHit in rayHits)
        {
            if (rayHit.collider.CompareTag("Tile"))
            {
                RaycastHit correctRayHit = rayHit;

                Vector3 currentPos = correctRayHit.point;
                currentPos.y = .1f;
                transform.position = currentPos;

                if (snapGrid)//is wall or not holdin ctrl
                {
                    transform.localPosition = Vector3.forward;
                    transform.position = new Vector3(Mathf.Round(currentPos.x / 6.25f) * 6.25f, .1f, Mathf.Round(currentPos.z / 6.25f) * 6.25f);//these dont actually place where they SHOULD!!!
                }
                return;
            }
        }
    }
}
