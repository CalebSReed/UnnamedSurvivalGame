using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderBehavior : MonoBehaviour
{
    private bool wanderCooldown = false;
    private bool isWalking = false;
    private bool isWaiting = false;
    public int visionDistance = 7;
    public int fleeVisionDistance = 20;
    public Vector3 target;
    private GameObject transformTarget;
    private int speed = 15;
    private Vector3 lastPosition;

    private void Start()
    {
        StartCoroutine(CheckIfMoving());
    }

    private void FixedUpdate()//add go home function to go back into rabbit hole when close enough or when sunset/night, or after certain amount of time
    {
        DecideMovement();
        MoveToTarget();
    }

    private void DecideMovement()
    {
        if (!isWalking)
        {
            target = transform.position;
        }
        if (!wanderCooldown)
        {
            Wander();
        }
        else if (Vector2.Distance(transform.position, target) < 0.01f && wanderCooldown && !isWaiting )
        {
            //Debug.Log("waiting");
            StartCoroutine(WaitForCoolDown());
        }
    }

    private void MoveToTarget()
    {
        
        if (!isWaiting)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

    private void Wander()
    {
        isWalking = true;
        //bool isObstructed = false;
        //Debug.Log("WANDER");
        /*while (isObstructed)
        {
            isObstructed = false;



            var checkList = Physics2D.OverlapCircleAll(target, 2.5f);
            foreach (Collider2D check in checkList)
            {
                if (!check.isTrigger && check.CompareTag("WorldObject"))
                {
                    isObstructed = true;
                    break;
                }
            }
        }*/
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
        yield return new WaitForSeconds(2f);
        wanderCooldown = false;
        isWaiting = false;
    }

    private IEnumerator CheckIfMoving()
    {
        lastPosition = transform.position;

        yield return new WaitForSeconds(1f);//wait a second b4 checking

        if (Vector2.Distance(lastPosition, transform.position) <= 0.1f && !isWaiting)
        {
            //Debug.Log("STUCK! MOVING TO NEW SPOT!");
            Wander();
        }
        StartCoroutine(CheckIfMoving());
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
}
