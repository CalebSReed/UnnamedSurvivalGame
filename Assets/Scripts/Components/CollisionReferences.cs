using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionReferences : MonoBehaviour
{
    public GameObject rootObj;
    public HealthManager hp;
    public RealMob mob;
    public RealWorldObject obj;
    public PlayerMain player;

    private void OnTriggerEnter(Collider other)
    {
        if (obj != null)
        {
            if (obj.woso.doesDamage && other.transform.root.gameObject == GameManager.Instance.localPlayer && obj.doDmgCoroutine == null)
            {
                other.GetComponentInParent<HealthManager>().TakeDamage(obj.woso.damage, "ground", gameObject);
                obj.doDmgCoroutine = obj.StartCoroutine(obj.CheckToDamageAgain());
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (obj != null)
        {
            if (obj.woso.doesDamage && other.transform.root.gameObject == GameManager.Instance.localPlayer && obj.doDmgCoroutine != null)
            {
                obj.StopCoroutine(obj.doDmgCoroutine);
                obj.doDmgCoroutine = null;
            }
        }
    }
}
