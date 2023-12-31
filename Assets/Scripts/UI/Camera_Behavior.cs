using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Camera_Behavior : MonoBehaviour
{
    public bool controlsEnabled = true;

    public float rotSpeed;
    private Vector3 offset = new Vector3(0f, 0f, 0f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private Transform camPivot;
    public Transform rotRef;

    [SerializeField] private Transform target;
    [SerializeField] private GameObject player;
    private PlayerInputActions input;

    private void Start()
    {
        input = player.GetComponent<PlayerMain>().playerInput;
    }

    void FixedUpdate()//which to choose.....
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            camPivot.position = Vector3.SmoothDamp(camPivot.position, new Vector3(targetPosition.x, 25, targetPosition.z), ref velocity, smoothTime);
        }
        camPivot.transform.rotation = Quaternion.Lerp(camPivot.rotation, rotRef.rotation, rotSpeed * Time.deltaTime);

    }
    
    public void RotateCamLeft(InputAction.CallbackContext context)
    {
        if (context.performed && controlsEnabled)
        {
            rotRef.Rotate(Vector3.up, 45);
        }
    }

    public void RotateCamRight(InputAction.CallbackContext context)
    {
        if (context.performed && controlsEnabled)
        {
            rotRef.Rotate(Vector3.up, -45);
        }
    }

    /*private IEnumerator SmoothRotation()
    {
        while (camPivot.rotation != newRot)
        camPivot.rotation = Vector3.SmoothDamp();
        yield return null;
    }*/
}
