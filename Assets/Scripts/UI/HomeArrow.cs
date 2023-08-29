using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeArrow : MonoBehaviour
{
    private Vector3 beaconLocation;

    private void Update()
    {
        if (beaconLocation != null)
        {
            Vector3 _look = transform.InverseTransformPoint(beaconLocation);
            float _angle = Mathf.Atan2(_look.y, _look.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, _angle));
        }

    }

    public void SetHome(Vector3 _home)
    {
        beaconLocation = _home;
    }
}
