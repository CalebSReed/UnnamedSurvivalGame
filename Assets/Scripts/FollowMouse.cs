using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.position = Input.mousePosition;
    }
}
