using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class AnimatorEventReceiver : MonoBehaviour
{
    public event Action<AnimationEvent> eventInvoked;
    [SerializeField] private PlayerMain player;

    public void OnAnimationEvent(AnimationEvent animationEvent)
    {
        eventInvoked?.Invoke(animationEvent);
    }

    public void OnHitSwingEvent(AnimationEvent animEvent)
    {
        player.swingingState.HitObjects();
    }

    public void OnSwingEndEvent(AnimationEvent animEvent)
    {
        player.swingingState.CheckToSwingAgain();
    }
}
