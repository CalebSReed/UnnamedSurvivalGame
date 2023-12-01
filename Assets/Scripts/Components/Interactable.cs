using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public InteractUnityEvent OnInteractEvent = new InteractUnityEvent();

    public void OnInteract(InteractArgs args)
    {
        OnInteractEvent?.Invoke(args);
    }
}
