using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeArrow : MonoBehaviour
{
    public static HomeArrow Instance { get; private set; }
    [SerializeField] private Camera cam;
    private bool homeIsSet;
    private Vector3 beaconLocation;
    private Transform sprTrans;

    private void Awake()
    {
        Instance = this;
        sprTrans = transform.Find("Image");
    }

    private void Update()
    {
        if (beaconLocation != null && homeIsSet)
        {
            var newLook = beaconLocation;
            newLook.y = 0;
            GameManager.Instance.localPlayerMain.homeArrowRef.LookAt(newLook);
            //sprTrans.transform.eulerAngles = new Vector3(0, 0, empty.rotation.eulerAngles.y);//this is so dumb lol
            //sprTrans.eulerAngles = new Vector3(0, 0, empty.eulerAngles.y);
            GameManager.Instance.localPlayerMain.homeArrowRef.eulerAngles = new Vector3(0, 0, -GameManager.Instance.localPlayerMain.homeArrowRef.eulerAngles.y);
            sprTrans.rotation = GameManager.Instance.localPlayerMain.homeArrowRef.GetChild(0).rotation;
            sprTrans.eulerAngles = new Vector3(sprTrans.eulerAngles.x, sprTrans.eulerAngles.y, sprTrans.eulerAngles.z + cam.transform.eulerAngles.y);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    public void RemoveHome(Vector3 homePos)
    {
        if (beaconLocation == homePos)//if for some reason multiple beacons, then removing the wrong one wont remove home arrow
        {
            homeIsSet = false;           
        }
    }

    public void SetHome(Transform _home)
    {
        homeIsSet = true;
        beaconLocation = _home.position;
    }
}
