using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoutAI : MonoBehaviour
{
    public int speed { get; set; }

    private bool goToPlayer = false;

    private Vector3 target;

    private RealWorldObject researchTarget;

    private Vector3 lastPosition;

    private PlayerMain player;

    public float playerVisionDistance { get; set; }

    public float fleeVisionDistance { get; set; }

    public float objectVisionDistance { get; set; }

    private Coroutine researchCoroutine;

    private int currentBasePoints = 0;

    private int minimumBasePointsRequired = 100;

    private bool currentlyAttacking = false;

    private bool attackLanded = false;

    private bool readyToGoHome = false;

    private bool waiting = false;

    public enum State
    {
        Idle,
        Scouting,
        Fleeing,
        Attacking,
        ReturningHome,
        GoingToResearch,
        Researching
    }

    private State currentState = State.Idle;

    //scouter should start to roam with a bias towards the player's direction.    or maybe only biased if theyre close enough to player? otherwise just have them go random places??
    //scouter should flee when close to player but attack if hurt
    //how do we make melee weapons viable tho? 
    //scouter should immediately ignore player and investigate ANY nearby manmade objects when it finds them
    //scouter should count points of playermade objects nearby. Maybe it has to touch them research them? And will have a very delayed reaction on the player catching it.
    //once the points exceed the minimum required or exceed previous base's points the colony will assume this is the new base to raid. 
    //im guessing the location would be the center of all manmade object in an area? scan like 100 units around each object and if new object found scan that until no unscanned objects remain? 
    //then average all their locations or sumn idk or maybe that doesnt matter. I imagine a large base could confuse parasites?
    //or we could always have them be biased towards player location once a wave starts (unless player is reasonably far away??)
    //anyways... once new base is found, stop sending out scouters and have all existing ones return to parasite base.
    //then send waves MWAHAHAH!


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();
        target = transform.position;
        StartCoroutine(Idle());
        StartCoroutine(CheckIfMoving());
    }

    private void Update()
    {
        DecideNextAction();

        if (currentlyAttacking)
        {
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }


    private void DecideNextAction()
    {
        if (currentState != State.Researching && researchCoroutine != null)
        {
            StopCoroutine(researchCoroutine);
        }

        if (currentState != State.Attacking && readyToGoHome)
        {
            currentState = State.ReturningHome;
        }

        if (currentState != State.Attacking)
        {
            currentlyAttacking = false;
        }

        switch (currentState)
        {
            case State.Idle:
                CheckToFlee();
                break;
            case State.Scouting:
                FindManMadeObjects();
                CheckToFlee();
                GoToTarget();
                CheckIfTargetReached();
                break;
            case State.Fleeing:
                CheckToStopFleeing();
                transform.position = MoveAway(transform.position, player.transform.position, speed * Time.deltaTime);
                break;
            case State.Attacking:

                break;
            case State.GoingToResearch:
                GoToTarget();
                TryToResearch();
                break;
            case State.Researching:

                break;
            case State.ReturningHome:
                transform.position = MoveAway(transform.position, player.transform.position, speed * Time.deltaTime);
                //GoToTarget();
                CheckToDespawn();
                break;
        }
    }

    private void CheckToDespawn()
    {
        if (Vector3.Distance(transform.position, player.transform.position) > 200f)
        {
            GetComponent<RealMob>().Die(false);
        }
    }

    private void TryToResearch()
    {
        if (Vector3.Distance(transform.position, target) <= 5)
        {
            currentState = State.Researching;
            researchCoroutine = StartCoroutine(GainResearchPoints());
        }
    }

    private IEnumerator GainResearchPoints()
    {
        int _prog = 0;
        while (_prog <= 5)
        {
            yield return new WaitForSeconds(1);
            if (Vector3.Distance(transform.position, target) > 5)
            {
                currentState = State.GoingToResearch;
                yield break;
            }
            _prog++;
        }
        print("research done :3");
        currentBasePoints += researchTarget.obj.woso.basePoints;
        ParasiteFactionManager.Instance.researchedObjectList.Add(researchTarget.gameObject);
        if (minimumBasePointsRequired > currentBasePoints)
        {
            currentState = State.Idle;
            StartCoroutine(Idle());
        }
        else
        {
            readyToGoHome = true;
            CalculateBasePosition();
            currentState = State.ReturningHome;
        }
    }

    private void CalculateBasePosition()
    {
        Vector3 _tempPos = Vector3.zero;
        foreach (GameObject _object in ParasiteFactionManager.Instance.researchedObjectList)
        {
            _tempPos += _object.transform.position;
        }
        _tempPos /= ParasiteFactionManager.Instance.researchedObjectList.Count;
        ParasiteFactionManager.parasiteData.PlayerBase = _tempPos;
    }

    private void CheckToStopFleeing()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.position, fleeVisionDistance);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.CompareTag("Player"))
            {
                currentState = State.Fleeing;
                return;
            }
        }
        StartCoroutine(Idle());
        currentState = State.Idle;
    }

    private void CheckToFlee()//maybe call every second? So player has a chance to use melee weapons to 
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.position, playerVisionDistance);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.CompareTag("Player"))
            {
                currentState = State.Fleeing;
                return;
            }
        }
    }

    private void FindManMadeObjects()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.position, objectVisionDistance);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.GetComponent<RealWorldObject>() != null)
            {
                if (_target.GetComponent<RealWorldObject>().obj.woso.isPlayerMade && !IsAlreadyResearched(_target.gameObject))
                {
                    if (Vector3.Distance(_target.transform.position, ParasiteFactionManager.parasiteData.PlayerBase) > 500f || ParasiteFactionManager.parasiteData.PlayerBase == Vector3.zero)
                    {
                        target = _target.transform.position;
                        researchTarget = _target.GetComponent<RealWorldObject>();
                        currentState = State.GoingToResearch;
                        return;
                    }
                }
            }
        }
    }

    private bool IsAlreadyResearched(GameObject _target)
    {
        foreach (GameObject _object in ParasiteFactionManager.Instance.researchedObjectList)
        {
            if (_target == _object)
            {
                return true;
            }
        }
        return false;
    }

    public void OnHit()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.position, fleeVisionDistance);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.gameObject.GetComponent<ScoutAI>() != null)
            {
                if (_target.gameObject.GetComponent<ScoutAI>().currentState != State.Attacking && _target.gameObject.GetComponent<ScoutAI>() != this)
                {
                    //Debug.Log("GO GET EM BOYS!");
                    _target.gameObject.GetComponent<ScoutAI>().currentState = State.Attacking;
                    StartCoroutine(_target.gameObject.GetComponent<ScoutAI>().Chase());
                }
            }
        }

        if (currentState != State.Attacking)
        {
            currentState = State.Attacking;
            StartCoroutine(BackOff());
        }
    }

    private bool Attack()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.position, 2);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.CompareTag("Player"))
            {
                _target.GetComponent<HealthManager>().TakeDamage(25, gameObject.tag, gameObject);
                return true;
            }
        }
        return false;
    }

    private IEnumerator BackOff()
    {
        target = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
        int i = 0;
        while (i < 60)
        {
            transform.position = MoveAway(transform.position, target, speed * Time.deltaTime);
            yield return null;
            i++;
        }
        StartCoroutine(LaunchAttack());
    }


    private void CheckIfTargetReached()
    {
        if (target == transform.position)
        {
            currentState = State.Idle;
            StartCoroutine(Idle());
        }
    }

    private IEnumerator Idle()
    {
        GetComponent<RealMob>().mobAnim.SetBool("isMoving", false);
        yield return new WaitForSeconds(3f);
        if (currentState == State.Idle)
        {
            WanderTowardsPlayer();
        }
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

    private void GoToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void WanderTowardsPlayer()
    {
        print("wanderin now");
        if (!goToPlayer || Vector3.Distance(transform.position, player.transform.position) < 100)
        {
            float _tX = (Random.Range(5, 21));//change to walking range value
            float _tY = (Random.Range(5, 21));
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
            target.x += _tX;//target starts as current pos, then we add these new values
            target.y += _tY;
            goToPlayer = true;
            GetComponent<RealMob>().mobAnim.SetBool("isMoving", true);
            currentState = State.Scouting;
        }
        else
        {
            target = player.transform.position;//you are bad at math you are bad at math you are bad at math
            currentState = State.Scouting;
            goToPlayer = false;
            GetComponent<RealMob>().mobAnim.SetBool("isMoving", true);
            StartCoroutine(CountDown());
        }
    }

    private IEnumerator CountDown()
    {
        yield return new WaitForSeconds(5);
        if (currentState == State.Scouting)
        {
            currentState = State.Idle;
        }
    }

    private IEnumerator CheckIfMoving()
    {
        lastPosition = transform.position;

        yield return new WaitForSeconds(1f);//wait a second b4 checking

        if (Vector2.Distance(lastPosition, transform.position) <= 0.1f && currentState != State.Idle && currentState != State.Attacking && currentState != State.Researching)
        {
            Debug.Log("STUCK! MOVING TO NEW SPOT!");
            WanderTowardsPlayer();
        }

        if (Vector2.Distance(lastPosition, transform.position) > 0.1f && currentState != State.Idle)
        {
            GetComponent<RealMob>().mobAnim.SetBool("isMoving", true);
        }     
        StartCoroutine(CheckIfMoving());
    }

    private IEnumerator LaunchAttack()
    {
        if (currentState != State.Attacking)
        {
            yield break;
        }
        yield return new WaitForSeconds(1);
        GetComponent<Rigidbody2D>().mass = .25f;
        currentlyAttacking = true;
        Debug.LogError("ATTTTTTTTTTTACKKKKKKKKK!");
        target = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
        Vector3 dir = target - transform.position;
        dir += dir;
        GetComponent<Rigidbody2D>().AddForce(dir, ForceMode2D.Impulse);
        yield return new WaitForSeconds(1.5f);
        GetComponent<Rigidbody2D>().mass = 3;
        currentlyAttacking = false;
        StartCoroutine(Chase());
    }

    private IEnumerator BiteAttack()
    {
        if (currentState != State.Attacking)
        {
            yield break;
        }
        int i = 0;
        target = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
        Vector3 dir = target - transform.position;
        while (i < 3)
        {
            yield return new WaitForSeconds(.5f);
            dir += dir;
            attackLanded = false;
            GetComponent<Rigidbody2D>().mass = .75f;
            currentlyAttacking = true;
            Debug.LogError("BITE!");
            attackLanded = Attack();
            GetComponent<Rigidbody2D>().AddForce(dir, ForceMode2D.Impulse);
            i++;
        }
        yield return new WaitForSeconds(.5f);
        GetComponent<Rigidbody2D>().mass = 3;
        attackLanded = false;
        currentlyAttacking = false;
        StartCoroutine(Chase());
    }

    private void DecideNextAttack()
    {
        int _randVal = Random.Range(0, 2);
        target = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
        if (_randVal == 0)
        {
            StartCoroutine(LaunchAttack());
        }
        else
        {
            StartCoroutine(BiteAttack());
        }
    }

    private IEnumerator Chase()
    {
        target = player.transform.position;
        GoToTarget();
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.position, playerVisionDistance);

        foreach (Collider2D _target in _targetList)
        {
            if (_target.CompareTag("Player"))
            {
                print("START THE ATTACK");
                DecideNextAttack();
                yield break;
            }
        }

        Collider2D[] _targetList2 = Physics2D.OverlapCircleAll(transform.position, fleeVisionDistance * 1.5f);

        bool _playerFound = false;
        foreach (Collider2D _target in _targetList2)
        {
            if (_target.CompareTag("Player"))
            {
                _playerFound = true;
            }
        } 
        
        if (!_playerFound)
        {
            print("eh, bored");
            currentState = State.Idle;
            StartCoroutine(Idle());
            yield break;
        }

        yield return null;
        StartCoroutine(Chase());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && currentlyAttacking && !attackLanded)
        {
            player.GetComponent<HealthManager>().TakeDamage(25, gameObject.tag, gameObject);//be diff on attack type?
            currentlyAttacking = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, playerVisionDistance);
        Gizmos.DrawWireSphere(transform.position, objectVisionDistance);
        Gizmos.DrawWireSphere(transform.position, fleeVisionDistance);
    }
}
