using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    [SerializeField] Camera mainCam;
    [SerializeField] Canvas canvas;
    void LateUpdate()
    {
        var _pos = Input.mousePosition;
        _pos.z = canvas.planeDistance;
        transform.position = mainCam.ScreenToWorldPoint(_pos);
        
    }
}
