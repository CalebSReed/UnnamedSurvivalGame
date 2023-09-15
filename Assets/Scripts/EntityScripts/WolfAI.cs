using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WolfAI : MonoBehaviour
{
    //public Collider2D hitBox;
    private Vector3 target;
    public int speed = 15;
    private GameObject player;
    private bool playerDetected;
    private bool isMoving;
    private bool isAttacking = false;
    //public LayerMask entityLayer;
    private int atkDmg = 25;
    private float atkRange = 5;
    private float walkCooldown = 3;
    private float atkCooldown = .75f;
    //internal HealthManager hpManager;
    //public int health;
    private bool isAlive = true;
    private bool waiting;
    //public Transform visionBox;
    //public GameObject tempAtkSpr;
    //public SpriteRenderer spriteRenderer;

    public int visionDistance = 25;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        //spriteRenderer = GetComponent<SpriteRenderer>();
        //hpManager = GetComponent<HealthManager>();//set field/ new instance of healthmanager class to the healthmanager reference we have installed in this gameobject
        //hpManager.OnDamageTaken += TakeDamage;
        target = transform.position;
        //hpManager.SetHealth(health);
        //StartCoroutine("Wander");
    }

    // Update is called once per frame
    void Update()
    {
        FindTargets();
        DecideMovement();
        CheckAttack();
    }

    private void FindTargets()
    {
        Collider2D[] _targetList = Physics2D.OverlapCircleAll(transform.position, visionDistance);
        foreach (Collider2D _enemy in _targetList)
        {
            if (_enemy.CompareTag("Player"))
            {
                playerDetected = true;             
                //Debug.Log("hit player");
                return;
            }
        }
        //Debug.Log("NONE FOUND");
        playerDetected = false;
    }

    private void TakeDamage(object sender, System.EventArgs e)
    {
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        //spriteRenderer.color = new Color(255, 0, 0);
        yield return new WaitForSeconds(.1f);
        //spriteRenderer.color = new Color(255, 255, 255);
    }

    private void DecideMovement()
    {
        if (!playerDetected && target == transform.position && !isAttacking && !isMoving)//player not detected and not attacking and not moving
        {
            Wander();
        }
        else if (target == transform.position && isMoving && !isAttacking && !waiting)//stopped moving, induce cooldown before moving again
        {
            StartCoroutine(Cooldown(walkCooldown));
        }
        else if (playerDetected && !isAttacking)//if not actively attacking but sees player, chase
        {
            target = player.transform.position;
        }
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);//move towards target
    }

    private IEnumerator Cooldown(float _timer)
    {
        waiting = true;
        yield return new WaitForSeconds(_timer);
        isMoving = false;
        waiting = false;
    }

    private Collider2D[] hitEnemies()
    {
        Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(transform.position, atkRange);
        return _hitEnemies;
    }

    private void CheckAttack()
    {
        if (!isAttacking)
        {
            Collider2D[] _hitEnemies = hitEnemies();
            foreach (Collider2D _enemy in _hitEnemies)
            {
                if (_enemy.CompareTag("Player"))
                {
                    StartCoroutine(Attack());
                }
            }
        }

        //target = transform.position;
    }

    private IEnumerator Attack()//make it so only one entity is attacked UNLESS AOE ATTACK, so make script to find nearest target
    {
        Debug.Log("attack!!");
        isAttacking = true;
        target = transform.position;
        isMoving = false;
        yield return new WaitForSeconds(atkCooldown);//windup

        Collider2D[] _hitEnemies = hitEnemies();

        //tempAtkSpr.SetActive(true);
        foreach (Collider2D _enemy in _hitEnemies)
        {
            if (_enemy.CompareTag("Player"))
            {
                _enemy.GetComponent<PlayerMain>().TakeDamage(atkDmg);
                Debug.Log("hit player");
                if (_enemy.GetComponent<PlayerMain>().godMode)
                {
                    GetComponent<HealthManager>().TakeDamage(999999);
                }
            }
        }
        yield return new WaitForSeconds(.5f);//attackLag
        //tempAtkSpr.SetActive(false);
        isAttacking = false;
    }

    private void Wander()
    {
        if (target == transform.position && !isMoving)
        {
            float _tX = (Random.Range(5, 11));
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
        }
        isMoving = true;
    }

    //make wandering ai, then chasing ai, and attacking ai

    private void OnCollisionEnter2D(Collision2D collision)
    {       
        /*target = transform.position;
        if (!isAttacking)
        {
            Attack();
        }*/
    }

    public void OnMouseDown()
    {
        //StartCoroutine(player.GetComponent<PlayerMain>().OnEnemySelected());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
           //playerDetected = true;          
        }
        if (collision.CompareTag("Projectile"))
        {
            //hpManager.TakeDamage(collision.GetComponent<ProjectileManager>().item.GetDamage());
            //Destroy(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //playerDetected = false;
            //Debug.Log("no triggr");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
}
