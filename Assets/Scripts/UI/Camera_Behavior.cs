using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Behavior : MonoBehaviour
{
    public float rotSpeed;
    private Vector3 offset = new Vector3(0f, 0f, 0f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private Transform camPivot;
    public Transform rotRef;

    [SerializeField] private Transform target;

    private void Awake()
    {

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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            rotRef.Rotate(Vector3.up, 45);
        }

        if (Input.GetKeyDown(KeyCode.E))
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
