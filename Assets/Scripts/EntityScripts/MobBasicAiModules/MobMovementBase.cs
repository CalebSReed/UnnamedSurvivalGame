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

    public bool ignoreFleeingOverride { get; set; }

    public Vector3 lastPosition { get; set; }

    private Vector3 lastFlipPos;

    private bool checkFlip;

    public event System.EventHandler OnWander;

    private AudioManager audio;

    private AnimatorEventReceiver animEvent;

    public enum MovementOption
    {
        DoNothing,
        MoveTowards,
        MoveAway,
        Chase,
        Wait,
        Special
    }

    private void Awake()
    {
        audio = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        animEvent = GetComponentInChildren<AnimatorEventReceiver>();
        animEvent.eventInvoked += PlayFootStep;
        //audio = gameObject.GetComponent<RealMob>().audio;
        target = gameObject;
        lastPosition = transform.position;
        currentMovement = 0;
        realMob = GetComponent<RealMob>();
        speed = realMob.mob.mobSO.speed;
        wanderTarget = transform.position;
        Wander();
    }

    private void OnEnable()
    {
        StartCoroutine(CheckIfMoving());
        if (currentMovement != MovementOption.Special)
        {
            Wander();
        }
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
                realMob.mobAnim.SetBool("isMoving", false);
                break;
            case MovementOption.Chase:
                realMob.mobAnim.SetBool("isMoving", false);
                break;
            case MovementOption.Wait:
                break;
            case MovementOption.Special:
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
                realMob.mobAnim.SetBool("isMoving", true);
                if (IsInEnemyList())
                {
                    var player = GameObject.Find("Player").GetComponent<PlayerMain>();
                    foreach (GameObject obj in player.enemyList)
                    {
                        if (obj == gameObject)
                        {
                            player.enemyList.Remove(obj);
                            return;
                        }
                    }
                }
                break;
            case MovementOption.Chase:
                realMob.mobAnim.SetBool("isMoving", true);
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
                    wanderTarget = realMob.home.transform.position;
                    SwitchMovement(MovementOption.MoveTowards);
                    return;
                }
                wanderTarget = transform.position;                
                StartCoroutine(WaitToWander());
                break;
            case MovementOption.Special:
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
            case MovementOption.Special:
                break;
        }
        CheckToFlip();
    }

    private void MoveTowardsTarget()
    {
        if (currentMovement == MovementOption.Chase && target != null)//true target is assigned on prey found in aggro AI
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);//stop using move towards, generate a vector and send the RB that way instead
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
            transform.position = CalebUtils.MoveAway(transform.position, target.transform.position, speed * 2 * Time.deltaTime);//run fast bro
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
            OnWander?.Invoke(this, System.EventArgs.Empty);
        }
    }

    private void Wander()
    {
        if (realMob.mob.mobSO.isRaidParasite)
        {
            wanderTarget = ParasiteFactionManager.parasiteData.PlayerBase;
        }
        else
        {
            wanderTarget = CalebUtils.RandomPositionInRadius(wanderTarget, 5, 25);
            wanderTarget = new Vector3(wanderTarget.x, 0, wanderTarget.z);
        }

        SwitchMovement(MovementOption.MoveTowards);
        GetComponent<RealMob>().mobAnim.SetBool("isMoving", true);
    }

    private void CheckToFlip()
    {
        if (checkFlip)
        {
            checkFlip = false;
            if (lastFlipPos.x < transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if  (lastFlipPos.x > transform.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else
        {
            checkFlip = true;
        }
        lastFlipPos = transform.position;
    }

    private IEnumerator CheckIfMoving()
    {
        if (currentMovement == MovementOption.Special)
        {
            realMob.mobAnim.SetBool("isMoving", false);
            yield return new WaitForSeconds(1f);
            StartCoroutine(CheckIfMoving());
            yield break;
        }
        if (currentMovement != MovementOption.Wait)
        {
            GetComponent<RealMob>().mobAnim.SetBool("isMoving", true);
        }

        lastPosition = transform.position;

        //wait a second b4 checking

        if (Vector3.Distance(lastPosition, transform.position) <= 3f && currentMovement != MovementOption.DoNothing && currentMovement != MovementOption.Special)
        {
            //Debug.Log("STUCK! MOVING TO NEW SPOT!");//here we should override chase behavior until next wait period, that way they get smart and move away instead of chase thru wall
            wanderTarget = transform.position;//reset target so we can add new Dir from origin point. This is our temp solution to getting stuck instead of using a navMesh i guess??
            Wander();
        }

        
    }

    public void PlayFootStep(AnimationEvent Event)
    {
        int i = Random.Range(1, 7);
        audio.Play($"Step{i}", transform.position, gameObject);
    }

    private bool IsInEnemyList()
    {
        var player = GameObject.Find("Player").GetComponent<PlayerMain>();
        foreach (GameObject obj in player.enemyList)
        {
            if (obj == gameObject)
            {
                return true;
            }
        }
        return false;
    }
}
