using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobState
{
    protected RealMob mob;
    protected MobStateMachine mobStateMachine;

    public MobState(RealMob _mob, MobStateMachine _mobStateMachine)
    {
        mob = _mob;
        mobStateMachine = _mobStateMachine;
    }

    public virtual void EnterState()
    {
        
    }

    public virtual void ExitState()
    {

    }

    public virtual void FrameUpdate()
    {
        
    }

    public virtual void PhysicsUpdate()
    {

    }

    public virtual void AnimationTriggerEvent()
    {

    }
}
