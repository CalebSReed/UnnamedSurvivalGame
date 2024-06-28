using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoardBehavior : MonoBehaviour
{
    public bool isRotating = true;
    void LateUpdate()
    {
        if (isRotating)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
