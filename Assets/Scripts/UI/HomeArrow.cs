using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeArrow : MonoBehaviour
{
    private Vector3 beaconLocation;
    private Transform sprTrans;
    private Transform empty;

    private void Awake()
    {
        sprTrans = transform.Find("Image");
        empty = transform.Find("Empty");
    }

    private void Update()
    {
        if (beaconLocation != null)
        {
            Vector3 thisPos = Camera.main.ScreenToWorldPoint(transform.position);
            empty.position = thisPos;
            Vector3 _look = empty.InverseTransformPoint(beaconLocation);//this is dumb that im using an empty transform idfk how else to do this bruh
            float _angle = Mathf.Atan2(_look.y, _look.x) * Mathf.Rad2Deg;
            sprTrans.rotation = Quaternion.Euler(new Vector3(0, 0, _angle));
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    public void SetHome(Vector3 _home)
    {
        beaconLocation = _home;
    }
}
