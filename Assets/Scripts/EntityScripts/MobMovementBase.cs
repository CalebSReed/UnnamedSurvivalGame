using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobMovementBase : MonoBehaviour
{
    public MovementOption currentMovement { get; private set; }

    public Vector3 target { get; set; }

    public enum MovementOption
    {
        DoNothing,
        MoveTowards,
        MoveAway
    }

    private void Awake()
    {
        target = transform.position;
        currentMovement = 0;
    }

    public void SwitchMovement(MovementOption _newOption)
    {
        currentMovement = _newOption;
        switch (currentMovement)
        {
            case MovementOption.DoNothing:
                break;
            case MovementOption.MoveTowards:
                break;
            case MovementOption.MoveAway:
                break;
        }
    }

    private void Update()
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
        }
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime);
    }

    private void MoveAwayFromTarget()
    {

    }
}
