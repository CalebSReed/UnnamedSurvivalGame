using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeArrow : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private Transform beaconLocation;
    private Transform sprTrans;

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

    public void SetHome(Transform _home)
    {
        beaconLocation = _home;
    }
}
