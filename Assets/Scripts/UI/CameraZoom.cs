using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraZoom : MonoBehaviour
{
    private float zoom;
    [SerializeField] private float zoomMult = 4f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 8f;
    [SerializeField] private float velocity = 0f;
    [SerializeField] private float smoothTime = .25f;

    private void Start()
    {
        zoom = Camera.main.orthographicSize;
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoom -= scroll * zoomMult;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        Camera.main.orthographicSize = Mathf.SmoothDamp(Camera.main.orthographicSize, zoom, ref velocity, smoothTime);
    }
}
