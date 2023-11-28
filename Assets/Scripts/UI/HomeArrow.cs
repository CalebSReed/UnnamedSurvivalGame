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
            /*Vector3 thisPos = transform.position;
            thisPos.z = 10;
            Camera.main.ScreenToWorldPoint(thisPos);
            empty.position = thisPos;
            Vector3 _look = empty.InverseTransformPoint(beaconLocation);//this is dumb that im using an empty transform idfk how else to do this bruh
            float _angle = Mathf.Atan2(_look.y, _look.x) * Mathf.Rad2Deg;
            sprTrans.rotation = Quaternion.Euler(new Vector3(0, 0, _angle));*/

            Ray ray = Camera.main.ScreenPointToRay(empty.transform.position);
            RaycastHit rayHit;
            Physics.Raycast(ray, out rayHit);

            var newPos = rayHit.point;
            newPos.y = 0;
            empty.position = newPos;
            empty.LookAt(beaconLocation);
            //sprTrans.transform.eulerAngles = new Vector3(0, 0, empty.rotation.eulerAngles.y);//this is so dumb lol
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
