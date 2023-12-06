using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeArrow : MonoBehaviour
{
    private Transform beaconLocation;
    private Transform sprTrans;
    [SerializeField] private Transform empty;

    private void Awake()
    {
        sprTrans = transform.Find("Image");
    }

    private void Update()
    {
        if (beaconLocation != null)
        {
            var newLook = beaconLocation.position;
            newLook.y = 0;
            empty.LookAt(newLook);
            //sprTrans.transform.eulerAngles = new Vector3(0, 0, empty.rotation.eulerAngles.y);//this is so dumb lol
            //sprTrans.eulerAngles = new Vector3(0, 0, empty.eulerAngles.y);
            empty.eulerAngles = new Vector3(0, 0, -empty.eulerAngles.y);
            sprTrans.rotation = empty.GetChild(0).rotation;
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    public void SetHome(Transform _home)
    {
        beaconLocation = _home;
    }
}
