using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLoader : MonoBehaviour//loads objects if close to player, unload when we get too far
{
    public Transform player;
    public CircleCollider2D circleCollider;
    // Start is called before the first frame update
    void Start()
    {
        Physics2D.IgnoreLayerCollision(0, 11);
        Collider[] hitColliders;
        hitColliders = Physics.OverlapSphere(transform.position, circleCollider.radius); // Should probably add layermask and a triggerquery

        for (int i = hitColliders.Length - 1; i > -1; i--)
        {
            Debug.Log("loading");
            ObjectComponentState(hitColliders[i], true);
        }
    }

    private void Update()
    {
        transform.position = player.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        ObjectComponentState(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        ObjectComponentState(other, false);
    }

    private void ObjectComponentState(Collider other, bool state)
    {
        Debug.Log("loading " + state);

        other.GetComponentInChildren<CircleCollider2D>(true).enabled = state;
        other.GetComponent<CircleCollider2D>().enabled = state;

        other.GetComponentInChildren<MonoBehaviour>(true).enabled = state;
        other.GetComponent<MonoBehaviour>().enabled = state;
    }
}
