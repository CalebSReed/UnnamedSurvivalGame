using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoardBehavior : MonoBehaviour
{
    public bool isRotating = true;
    public bool fourAngles;
    void LateUpdate()
    {
        if (isRotating)
        {
            transform.rotation = Camera.main.transform.rotation;

            if (fourAngles)
            {

            }
        }
    }
}
