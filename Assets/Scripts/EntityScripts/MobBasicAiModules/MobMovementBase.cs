using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobMovementBase : MonoBehaviour
{
    public RealMob realMob;
    public MovementOption currentMovement { get; private set; }

    public GameObject target;

    public Vector3 wanderTarget;

    public Vector3 trueTarget;

    public bool aggroOverride { get; set; }

    public bool goHome;

    public int speed;

    public Vector3 lastPosition { get; set; }

    public enum MovementOption
    {
        DoNothing,
        MoveTowards,
        MoveAway,
        Chase,
        Wait,
    }

    private void Awake()
    {
        target = gameObject;
        lastPosition = transform.position;
        currentMovement = 0;
        realMob = GetComponent<RealMob>();
        speed = realMob.mob.mobSO.speed;
        wanderTarget = transform.position;
        Wander();
    }

    public void SwitchMovement(MovementOption _newOption)
    {
        switch (currentMovement)//leave state
        {
            case MovementOption.DoNothing:
                break;
            case MovementOption.MoveTowards:
                break;
            case MovementOption.MoveAway:
                break;
            case MovementOption.Chase:
                break;
            case MovementOption.Wait:
                break;
        }

        currentMovement = _newOption;

        switch (currentMovement)//enter state
        {
            case MovementOption.DoNothing:
                break;
            case MovementOption.MoveTowards:
                break;
            case MovementOption.MoveAway:
                break;
            case MovementOption.Chase:
                break;
            case MovementOption.Wait:
                if (aggroOverride)
                {
                    SwitchMovement(MovementOption.Chase);
                    return;
                }
                else if (goHome && realMob.home != null)
                {
                    target = realMob.home.gameObject;
                    SwitchMovement(MovementOption.MoveTowards);
                }
                wanderTarget = transform.position;
                StartCoroutine(WaitToWander());
                break;
        }
    }

    private void Update()//wander towards vector, or flee / chase an objects position
    {
        switch (currentMovement)
        {
            case MovementOption.DoNothing:
                
                break;
            case MovementOption.MoveTowards:
                MoveTowardsTarget();
                break;
            case MovementOption.MoveAway:
                MoveAwayFromTarget();
                break;
            case MovementOption.Chase:
                MoveTowardsTarget();
                break;
            case MovementOption.Wait:
                break;
        }
    }

    private void MoveTowardsTarget()
    {
        if (currentMovement == MovementOption.Chase && target != null)//true target is assigned on prey found in aggro AI
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        }
        else//normal
        {
            transform.position = Vector3.MoveTowards(transform.position, wanderTarget, speed * Time.deltaTime);

            if (transform.position == wanderTarget)
            {
                SwitchMovement(MovementOption.Wait);
            }
        }
    }

    private void MoveAwayFromTarget()
    {
        if (target != null)
        {
            transform.position = MoveAway(transform.position, target.transform.position, speed * 2 * Time.deltaTime);//run fast bro
        }
        else
        {
            SwitchMovement(MovementOption.Wait);
        }
    }

    private IEnumerator WaitToWander()
    {
        GetComponent<RealMob>().mobAnim.SetBool("isMoving", false);
        int _randVal = Random.Range(1, 6);
        yield return new WaitForSeconds(_randVal);
        if (currentMovement == MovementOption.Wait)
        {
            Wander();
        }
    }

    private void Wander()
    {
        float _tX = (Random.Range(5, 26));//change to walking range value
        float _tY = (Random.Range(5, 26));
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
        wanderTarget.x += _tX;//target starts as current pos, then we add these new values
        wanderTarget.y += _tY;
        SwitchMovement(MovementOption.MoveTowards);
        GetComponent<RealMob>().mobAnim.SetBool("isMoving", true);
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

    private IEnumerator CheckIfMoving()
    {
        if (currentMovement != MovementOption.Wait)
        {
            GetComponent<RealMob>().mobAnim.SetBool("isMoving", true);
        }

        lastPosition = transform.position;

        yield return new WaitForSeconds(1f);//wait a second b4 checking

        if (Vector2.Distance(lastPosition, transform.position) <= 1f)
        {
            Debug.Log("STUCK! MOVING TO NEW SPOT!");//here we should override chase behavior until next wait period, that way they get smart and move away instead of chase thru wall
            wanderTarget = transform.position;//reset target so we can add new Dir from origin point. This is our temp solution to getting stuck instead of using a navMesh i guess??
            Wander();
        }

        StartCoroutine(CheckIfMoving());
    }
}
