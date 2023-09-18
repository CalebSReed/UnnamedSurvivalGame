using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    void LateUpdate()
    {
        transform.position = Input.mousePosition;
    }
}
