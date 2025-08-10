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

            //transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);  Use this if u want everything to look like "paper cutouts", otherwise above is best for faking 3d

            if (fourAngles)
            {

            }
        }
    }
}
