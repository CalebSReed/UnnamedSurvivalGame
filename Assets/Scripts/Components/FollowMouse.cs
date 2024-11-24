using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    [SerializeField] Camera mainCam;
    [SerializeField] Canvas canvas;
    private GameObject player;
    private PlayerInputActions input;

    private void Start()
    {
        GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;   
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        player = GameManager.Instance.localPlayer;
        input = player.GetComponent<PlayerMain>().playerInput;
    }

    void LateUpdate()
    {
        if (player == null)
        {
            return;
        }
        Vector3 _pos = input.PlayerDefault.MousePosition.ReadValue<Vector2>();
        _pos.z = canvas.planeDistance;
        transform.position = mainCam.ScreenToWorldPoint(_pos);
        
    }
}
