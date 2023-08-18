using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoardBehavior : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
