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
    private PlayerMain player;
    private PlayerInputActions input;
    private Camera camera;

    private void Start()
    {
        camera = GetComponent<Camera>();
        zoom = camera.fieldOfView;
        GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        player = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
        input = player.playerInput;
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() || !controlsEnabled || player == null)
        {
            return;
        }
        float scroll = input.PlayerDefault.ZoomCam.ReadValue<float>();
        zoom -= scroll * zoomMult;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        camera.fieldOfView = Mathf.SmoothDamp(camera.fieldOfView, zoom, ref velocity, smoothTime);
    }
}
