using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderBehavior : MonoBehaviour
{
    private bool wanderCooldown = false;
    //private bool isWalking = false;
    private bool isWaiting = false;
    public int visionDistance = 7;
    public int fleeVisionDistance = 20;
    public Vector3 target;
    private GameObject transformTarget;
    private int speed = 25;

    private void Update()//add go home function to go back into rabbit hole when close enough or when sunset/night, or after certain amount of time
    {
        DecideMovement();
        MoveToTarget();
    }

    private void DecideMovement()
    {
        if (!wanderCooldown)
        {
            Wander();
        }
        else if (target == transform.position && wanderCooldown && !isWaiting)
        {
            //Debug.Log("waiting");
            StartCoroutine(WaitForCoolDown());
        }
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void Wander()
    {
        //isWalking = true;
        //Debug.Log("WANDER");
        float _tX = (Random.Range(5, 11));//change to walking range value
        float _tY = (Random.Range(5, 11));
        int _rand_num = Random.Range(0, 2);
        if (_rand_num == 1)
        {
            _tX *= -1;
        }
        _rand_num = Random.Range(0, 2);
        if (_rand_num == 1)
        {
            _tY *= -1;
        }
        target.x += _tX;
        target.y += _tY;
        wanderCooldown = true;
    }
    private IEnumerator WaitForCoolDown()
    {
        isWaiting = true;
        yield return new WaitForSeconds(1f);
        wanderCooldown = false;
        isWaiting = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
}
