using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyAI : MonoBehaviour
{
    private bool isFleeing = false;
    private bool wanderCooldown = false;
    private bool isWalking = false;
    private bool isFollowing = false;
    private bool isWaiting = false;
    public int visionDistance = 7;
    public int fleeVisionDistance = 20;
    public Vector3 target;
    private GameObject transformTarget;
    private int speed = 25;

    private void Update()//add go home function to go back into rabbit hole when close enough or when sunset/night, or after certain amount of time
    {
        FindTargets();
        DecideMovement();
        MoveToTarget();
    }

    private void DecideMovement()
    {

        if (!isFleeing && !isWalking && !isFollowing)
        {
            //Debug.LogError("resettingggg");
            target = transform.position;
        }

        if (isFleeing)
        {
            Flee();
        }
        else if (isFollowing)
        {
            Follow();
        }
        else if (!wanderCooldown && !isFleeing)
        {
            Wander();
        }
        else if (target == transform.position && wanderCooldown && !isWaiting)
        {
            //Debug.Log("waiting");
            StartCoroutine(WaitForCoolDown());
        }
        else if (isWaiting)
        {
            //Debug.Log("waiting.......");
        }
        else if (wanderCooldown)
        {
            //Debug.Log("walking.....");
        }
        else
        {
            //Debug.LogError("resettingggg");
            target = transform.position;
        }
        
    }

    private void MoveToTarget()
    {
        if (isFleeing)
        {
            transform.position = MoveAway(transform.position, target, speed * Time.deltaTime);
        }
        else if (!isFleeing)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
        //transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void Wander()
    {
        isWalking = true;
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

    private Vector2 MoveAway(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        Vector3 a = target - current;
        float magnitude = a.magnitude;
        if (magnitude <= maxDistanceDelta || magnitude == 0f)
        {
            return target;
        }
        return current - a / magnitude * maxDistanceDelta;
    }

    private void Flee()
    {
        //float angle = Vector2.Angle(transform.position, transformTarget.transform.position);

        //target = 
        //wanderCooldown = true;
        isWalking = false;

        //Vector2 newTarget = transform.position - -transformTarget.transform.position*2;

        target = transformTarget.transform.position;
        //target = newTarget;
    }

    private void Follow()
    {
        target = transformTarget.transform.position;
    }

    private void FindTargets()
    {
        if (isFleeing)
        {
            Collider2D[] _fleeTargetList = Physics2D.OverlapCircleAll(transform.position, fleeVisionDistance);
            CycleList(_fleeTargetList);
        }
        else
        {
            Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.position, visionDistance);
            CycleList(_targetList);
        }
    }

    private void CycleList(Collider2D[] _targetList)
    {
        foreach (Collider2D _target in _targetList)
        {
            if (_target.CompareTag("Item"))
            {
                Item _item = _target.GetComponent<RealItem>().GetItem();
                if (_item.itemType == Item.ItemType.WildCarrot || _item.itemType == Item.ItemType.WildParsnip)//change to use bait list maybe
                {
                    isFollowing = true;
                    isFleeing = false;
                    wanderCooldown = true;
                    transformTarget = _target.gameObject;
                    //Debug.Log("FOLLOWING");
                    return;
                }
            }
            else if (_target.CompareTag("Player") || _target.CompareTag("Enemy"))
            {
                isFleeing = true;
                isFollowing = false;
                wanderCooldown = true;
                transformTarget = _target.gameObject;
                //Debug.Log("FLEEING");
                return;
            }
        }
        isFleeing = false;
        isFollowing = false;
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
