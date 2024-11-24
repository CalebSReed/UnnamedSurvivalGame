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

    public float surroundDistance = 20;

    public enum MovementOption
    {
        DoNothing,
        MoveTowards,
        MoveAway,
        Chase,
        Wait,
        Surround,
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
        speed = realMob.mob.mobSO.walkSpeed;
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
            case MovementOption.Surround:
                realMob.mobAnim.SetBool("isMoving", false);
                realMob.rb.velocity = Vector3.zero;
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
            case MovementOption.Surround:
                var targetHealth = target.GetComponent<HealthManager>();
                if (targetHealth != null && targetHealth.currentHealth < targetHealth.maxHealth / 4)
                {
                    //Debug.LogError("WOOHOO!");
                    SwitchMovement(MovementOption.Chase);
                    break;
                }

                realMob.mobAnim.SetBool("isMoving", true);
                break;
            case MovementOption.Wait:
                if (aggroOverride)
                {
                    SwitchMovement(realMob.mob.mobSO.aggroStrategy);
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
            case MovementOption.Surround:
                SurroundTarget();
                break;
            case MovementOption.Special:
                break;
        }
        CheckToFlip();
    }

    private void SurroundTarget()
    {
        if (Vector3.Distance(target.transform.position, transform.position) < surroundDistance)
        {
            Vector3 dir = target.transform.position - transform.position;
            Vector3 left = Vector3.Cross(dir, Vector3.up).normalized;
            transform.LookAt(left, Vector3.up);
            realMob.rb.velocity = left * speed;
        }
        if (Vector3.Distance(target.transform.position, transform.position) < surroundDistance - 2)
        {
            Vector3 dir = target.transform.position - transform.position;
            transform.LookAt(dir, Vector3.up);
            realMob.rb.velocity -= dir.normalized * speed / 2;
        }
        else
        {
            Vector3 targetPos = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            transform.LookAt(targetPos, Vector3.up);
            transform.position = targetPos;
        }
    }

    private void MoveTowardsTarget()
    {
        if (currentMovement == MovementOption.Chase && target != null || currentMovement == MovementOption.Surround && target != null)//true target is assigned on prey found in aggro AI
        {
            Vector3 targetPos = Vector3.MoveTowards(transform.position, target.transform.position, realMob.mob.mobSO.runSpeed * Time.deltaTime);
            transform.LookAt(targetPos, Vector3.up);
            transform.position = targetPos;
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
            Vector3 targetPos = CalebUtils.MoveAway(transform.position, target.transform.position, realMob.mob.mobSO.runSpeed * 2 * Time.deltaTime);//run fast bro
            transform.LookAt(targetPos, Vector3.up);
            transform.position = targetPos;
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
            wanderTarget = new Vector3(wanderTarget.x, transform.position.y, wanderTarget.z);
            transform.LookAt(wanderTarget, Vector3.up);
        }

        SwitchMovement(MovementOption.MoveTowards);
        GetComponent<RealMob>().mobAnim.SetBool("isMoving", true);
    }

    private void CheckToFlip()
    {
        float angle = Vector3.SignedAngle(transform.forward, realMob.player.mainCam.transform.parent.forward, Vector3.up);
        angle = Mathf.Sign(angle);

        if (angle > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
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
        var player = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
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
