using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraZoom : MonoBehaviour
{
    public bool controlsEnabled = true;
    private float zoom;
    [SerializeField] private float zoomMult = 4f;
    [SerializeField] private float minZoom = 8f;
    [SerializeField] private float maxZoom = 16f;
    [SerializeField] private float velocity = 0f;
    [SerializeField] private float smoothTime = .25f;
    [SerializeField] private GameObject player;
    private PlayerInputActions input;

    private void Start()
    {
        zoom = Camera.main.fieldOfView;
        input = player.GetComponent<PlayerMain>().playerInput;
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() || !controlsEnabled)
        {
            return;
        }
        float scroll = input.PlayerDefault.ZoomCam.ReadValue<float>();
        zoom -= scroll * zoomMult;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, zoom, ref velocity, smoothTime);
    }
}
