﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class AnimatorEventReceiver : MonoBehaviour
{
    public event Action<AnimationEvent> eventInvoked;

    public void OnAnimationEvent(AnimationEvent animationEvent)
    {
        eventInvoked?.Invoke(animationEvent);
    }
}