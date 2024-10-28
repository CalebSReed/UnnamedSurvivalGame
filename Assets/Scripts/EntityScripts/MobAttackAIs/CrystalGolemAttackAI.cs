using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalGolemAttackAI : MonoBehaviour
{

    public MobMovementBase mobMovement { get; set; }
    public Animator anim { get; set; }
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
        CloseAttack();
    }

    private void CloseAttack()
    {
        attacking = true;
        int _randVal = Random.Range(0, 2);
        if (_randVal == 0)
        {
            anim.Play("Smash");
        }
        else
        {
            anim.Play("Punch_Side");
        }
    }

    private void FarAttack()
    {
        attacking = true;
        int _randVal = Random.Range(0, 2);
        if (_randVal == 0)
        {
            anim.Play("Smash");
        }
        else
        {
            anim.Play("Jump_Side");
        }
    }

    private void SetComboCount(object sender, ComboArgs e)
    {
        comboHitsLeft = e.comboCount;
        losingCombo = true;
        comboDepletionProgress = 0;
        realMob.animEvent.beingComboed = true;
    }

    private void TakeDamage(object sender, System.EventArgs e)
    {
        if (comboHitsLeft > 0)
        {
            if (mirroredHurt)
            {
                anim.Play("Hurt");
            }
            else
            {
                anim.Play("HurtM");
            }
            mirroredHurt = !mirroredHurt;
        }
        comboHitsLeft--;
        Debug.Log(comboHitsLeft);
        if (comboHitsLeft <= 0 && realMob.animEvent.beingComboed)
        {
            realMob.GetKnockedBack();
            realMob.animEvent.beingComboed = false;
        }
    }

    private void StartAttackTimer(object sender, System.EventArgs e)//on aggro
    {
        target = mobMovement.target;
        attackTimer = Random.Range(2f, 5f);
        timerRoutine = StartCoroutine(WaitToAttack());
        Debug.Log("starting timer");
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
        ResetAttackTimer();
    }

    private void ResetAttackTimer()
    {
        StopCoroutine(timerRoutine);
        attackTimer = Random.Range(2f, 5f);
        timerRoutine = StartCoroutine(WaitToAttack());
    }

    private void BeginStun(object sender, System.EventArgs e)
    {
        stunned = true;
    }

    private void EndStun(object sender, System.EventArgs e)
    {
        stunned = false;
    }

    void Start()
    {
        mobMovement = GetComponent<MobMovementBase>();
        realMob = GetComponent<RealMob>();
        anim = realMob.mobAnim;
        atkRadius = GetComponent<RealMob>().mob.mobSO.combatRadius;
        //mobMovement.SwitchMovement(MobMovementBase.MovementOption.Special);

        GetComponent<MobAggroAI>().StartCombat += StartCombat;
        GetComponent<MobAggroAI>().onAggro += StartAttackTimer;
        realMob.animEvent.SetCombo += SetComboCount;
        realMob.hpManager.OnDamageTaken += TakeDamage;
        realMob.animEvent.onAttackEnded += EndAttack;
        realMob.animEvent.beginHitStun += BeginStun;
        realMob.animEvent.endHitStun += EndStun;
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
                    realMob.animEvent.ResumeChasing();
                }
            }
        }
        else
        {
            comboDepletionProgress = 0;
        }
    }
}
