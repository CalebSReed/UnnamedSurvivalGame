using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulOrbBehavior : MonoBehaviour
{
    GameObject target;
    float multiplier = 0f;
    public int healthVal;
    void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        multiplier += .2f;
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * multiplier * 2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target)
        {
            target.GetComponent<HealthManager>().RestoreHealth(healthVal);
            Destroy(gameObject);
        }
    }
}
