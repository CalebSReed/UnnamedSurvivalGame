using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerMain localPlayer;

    private void Start()
    {
        GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        localPlayer = GameManager.Instance.localPlayer.GetComponent<PlayerMain>();
    }

    public void OnInteractButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed && localPlayer != null)
        {
            localPlayer.OnInteractButtonDown(context);
        }
    }

    public void OnSpecialInteractButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed && localPlayer != null)
        {
            localPlayer.OnSpecialInteractButtonDown(context);
        }
    }

    public void OnCancelButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed && localPlayer != null)
        {
            localPlayer.OnCancelButtonDown(context);
        }
    }

    public void OnDodgeRoll(InputAction.CallbackContext context)
    {
        if (context.performed && localPlayer != null)
        {
            localPlayer.DodgeRoll(context);
        }
    }
}
