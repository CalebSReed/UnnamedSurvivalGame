using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    [SerializeField] Camera mainCam;
    [SerializeField] Canvas canvas;
    [SerializeField] private GameObject player;
    private PlayerInputActions input;

    private void Start()
    {
        input = player.GetComponent<PlayerMain>().playerInput;
    }
    void LateUpdate()
    {
        Vector3 _pos = input.PlayerDefault.MousePosition.ReadValue<Vector2>();
        _pos.z = canvas.planeDistance;
        transform.position = mainCam.ScreenToWorldPoint(_pos);
        
    }
}
