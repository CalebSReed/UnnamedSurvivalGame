using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CrystalGolemAttackAI : NetworkBehaviour
{

    public MobMovementBase mobMovement { get; set; }
    public Animator anim { get; set; }
    public Animator shadowAnim { get; set; }
    public float atkRadius { get; set; }
    public GameObject target { get; set; }
    public bool attacking { get; set; }
    public RealMob realMob { get; set; }

    bool mirroredHurt;

    public int comboHitsLeft;

    private float comboDepletionProgress;

    private bool losingCombo;

    private float attackTimer;

    private Coroutine timerRoutine;

    private bool stunned;

    public void StartCombat(object sender, CombatArgs e)
    {
        if (comboHitsLeft > 0)
        {
            return;
        }

        target = e.combatTarget;

        if (!GameManager.Instance.isServer)
        {
            return;
        }

        CloseAttack();
    }

    private int StringToIntAttack(string val)
    {
        if (val == "Smash")
        {
            return 0;
        }
        else if (val == "SwipeL_Side")
        {
            return 1;
        }
        else if (val == "SwipeR_Side")
        {
            return 2;
        }
        else if (val == "Punch_Side")
        {
            return 3;
        }
        else if (val == "Shoot_Side")
        {
            return 4;
        }
        else if (val == "Jump_Side")
        {
            return 5;
        }
        else if (val == "Hurt")
        {
            return 6;
        }
        else if (val == "HurtM")
        {
            return 7;
        }
        else if (val == "EndCombo_Side")
        {
            return 8;
        }
        else
        {
            Debug.LogError("Wrong Attack Name");
            return -1;
        }
    }

    private string IntToStringAttack(int val)
    {
        if (val == 0)
        {
            return "Smash";
        }
        else if (val == 1)
        {
            return "SwipeL_Side";
        }
        else if (val == 2)
        {
            return "SwipeR_Side";
        }
        else if (val == 3)
        {
            return "Punch_Side";
        }
        else if (val == 4)
        {
            return "Shoot_Side";
        }
        else if (val == 5)
        {
            return "Jump_Side";
        }
        else if (val == 6)
        {
            return "Hurt";
        }
        else if (val == 7)
        {
            return "HurtM";
        }
        else if (val == 8)
        {
            return "EndCombo_Side";
        }
        else
        {
            Debug.LogError("Wrong Attack Name");
            return "Null";
        }
    }



    [Rpc(SendTo.NotServer)]
    private void SyncAttackRPC(int val)
    {
        //anim.Play(IntToStringAttack(val));
    }

    private void CloseAttack()
    {
        attacking = true;
        int _randVal = Random.Range(0, 4);
        if (_randVal == 0)
        {
            anim.Play("Smash");
            //shadowAnim.Play("Smash");
            SyncAttackRPC(StringToIntAttack("Smash"));
        }
        else if (_randVal == 1)
        {
            anim.Play("SwipeL_Side");
            //shadowAnim.Play("SwipeL_Side");
            SyncAttackRPC(StringToIntAttack("SwipeL_Side"));
        }
        else if (_randVal == 2)
        {
            anim.Play("SwipeR_Side");
            //shadowAnim.Play("SwipeR_Side");
            SyncAttackRPC(StringToIntAttack("SwipeR_Side"));
        }
        else
        {
            anim.Play("Punch_Side");
            //shadowAnim.Play("Punch_Side");
            SyncAttackRPC(StringToIntAttack("Punch_Side"));
        }
    }

    private void FarAttack()
    {
        attacking = true;
        int _randVal = Random.Range(0, 3);
        if (_randVal == 0)
        {
            anim.Play("Smash");
            //shadowAnim.Play("Smash");
            SyncAttackRPC(StringToIntAttack("Smash"));
        }
        else if (_randVal == 1)
        {
            anim.Play("Shoot_Side");
            //shadowAnim.Play("Shoot_Side");
            SyncAttackRPC(StringToIntAttack("Shoot_Side"));
        }
        else
        {
            anim.Play("Jump_Side");
            //shadowAnim.Play("Jump_Side");
            SyncAttackRPC(StringToIntAttack("Jump_Side"));
        }
    }

    private void SetComboCount(object sender, ComboArgs e)
    {
        comboHitsLeft = e.comboCount;
        losingCombo = true;
        comboDepletionProgress = 0;
        realMob.animEvent.beingComboed = true;
    }

    private void TakeDamage(object sender, DamageArgs e)
    {
        if (comboHitsLeft > 0)
        {
            if (mirroredHurt)
            {
                anim.Play("Hurt");
                //shadowAnim.Play("Hurt");
                SyncAttackRPC(StringToIntAttack("Hurt"));
            }
            else
            {
                anim.Play("HurtM");
                //shadowAnim.Play("HurtM");
                SyncAttackRPC(StringToIntAttack("HurtM"));
            }
            mirroredHurt = !mirroredHurt;
        }
        comboHitsLeft--;
        Debug.Log(comboHitsLeft);

        if (comboHitsLeft == 1)
        {
            realMob.player.swingAnimator.SetBool("ForceStopCombo", true);
        }

        if (comboHitsLeft <= 0 && realMob.animEvent.beingComboed)
        {
            realMob.GetKnockedBack(e.senderObject.GetComponent<PlayerMain>().swingingState.dir.normalized);
            realMob.animEvent.beingComboed = false;
        }
    }

    private void StartAttackTimer(object sender, System.EventArgs e)//on aggro
    {
        if (!GameManager.Instance.isServer)
        {
            return;
        }
        target = mobMovement.target;
        attackTimer = Random.Range(2f, 5f);
        timerRoutine = StartCoroutine(WaitToAttack());
        Debug.Log("starting timer");
    }

    private void OnHurtAggro(object sender, CombatArgs e)
    {
        if (timerRoutine == null)
        {
            StartAttackTimer(this, System.EventArgs.Empty);
        }
    }


    private IEnumerator WaitToAttack()
    {
        yield return new WaitForSeconds(attackTimer);

        if (target != null && mobMovement.currentMovement == realMob.mob.mobSO.aggroStrategy && comboHitsLeft <= 0)
        {
            if (Vector3.Distance(transform.position, target.transform.position) > atkRadius * 2)
            {
                FarAttack();
            }
            else
            {
                CloseAttack();
            }
        }
    }

    private void EndAttack(object sender, System.EventArgs e)
    {
        attacking = false;

        if (!GameManager.Instance.isServer)
        {
            return;
        }

        ResetAttackTimer();
    }

    private void ResetAttackTimer()
    {
        StopCoroutine(timerRoutine);
        attackTimer = Random.Range(2f, 5f);
        timerRoutine = StartCoroutine(WaitToAttack());
    }

    private void CheckCombo(object sender, AttackEventArgs e)
    {
        if (e.checkType == "swipe" && target != null && Vector3.Distance(realMob.transform.position, target.transform.position) < atkRadius * 2)
        {
            anim.Play("EndCombo_Side");
            //shadowAnim.Play("EndCombo_Side");
            SyncAttackRPC(StringToIntAttack("EndCombo_Side"));
        }
        else
        {
            realMob.animEvent.ResumeChasing();
        }
    }

    private void BeginStun(object sender, System.EventArgs e)
    {
        stunned = true;
    }

    private void EndStun(object sender, System.EventArgs e)
    {
        stunned = false;
    }

    private void StopTimer(object sender, System.EventArgs e)
    {
        timerRoutine = null;
        StopAllCoroutines();
    }

    void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        shadowAnim = realMob.shadowAnim;
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        //mobMovement.SwitchMovement(MobMovementBase.MovementOption.Special);

        GetComponent<MobAggroAI>().StartCombat += StartCombat;
        GetComponent<MobAggroAI>().onAggro += StartAttackTimer;
        GetComponent<MobNeutralAI>().OnAggroed += OnHurtAggro;
        GetComponent<MobAggroAI>().onDeaggro += StopTimer;
        realMob.animEvent.SetCombo += SetComboCount;
        realMob.hpManager.OnDamageTaken += TakeDamage;
        realMob.animEvent.onAttackEnded += EndAttack;
        realMob.animEvent.beginHitStun += BeginStun;
        realMob.animEvent.endHitStun += EndStun;
        realMob.animEvent.checkAttackConditions += CheckCombo;
    }

    private void Update()
    {
        if (losingCombo && !stunned && !attacking)//count timer when we have combo to lose, are not stunned, and not attacking
        {
            comboDepletionProgress += Time.deltaTime;
            if (comboDepletionProgress > .5f)//lose hits when timer hits
            {
                comboHitsLeft--;
                comboDepletionProgress -= .5f;
                if (comboHitsLeft <= 0)//reset timer at 0
                {
                    comboDepletionProgress = 0;
                    losingCombo = false;
                    realMob.animEvent.beingComboed = false;
                    realMob.player.swingAnimator.SetBool("ForceStopCombo", false);
                    realMob.animEvent.ResumeChasing();
                }
                else if (comboHitsLeft == 1)
                {
                    realMob.player.swingAnimator.SetBool("ForceStopCombo", true);
                }
            }
        }
        else
        {
            comboDepletionProgress = 0;
        }
    }
}
