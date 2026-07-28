using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoardBehavior : MonoBehaviour
{
    public bool isRotating = true;
    public bool fourAngles;
    public bool trueFaceCamera;
    void LateUpdate()
    {
        if (isRotating)
        {
            if (trueFaceCamera)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, Camera.main.transform.rotation.eulerAngles.z);  //Use this if u want everything to look like "paper cutouts", otherwise above is best for faking 3d
            }

            if (fourAngles)
            {

            }
        }
    }
}
