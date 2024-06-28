using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MouseHoverBehavior : MonoBehaviour
{
    //Here we go!
    [SerializeField] TextMeshProUGUI hoverText;
    [SerializeField] TextMeshProUGUI heldItemText;
    PlayerMain player;
    [SerializeField] GameManager gameManager;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();
    }

    private void Update()
    {
        if (gameManager.pauseMenu.activeSelf)
        {
            RemoveText();
            return;
        }
        else if (!CheckIfHoveringOverUI())
        {
            if (player.StateMachine.currentPlayerState == player.deployState)
            {
                ChangeText($"LMB: Deploy {player.deployState.deployItem.itemSO.deployObject.objName}");
                return;
            }
            else if (player.StateMachine.currentPlayerState == player.tillingState)
            {
                ChangeText($"RMB: Till Ground");
                return;
            }
            /*else if (player.StateMachine.currentPlayerState == player.aimingState)
            {
                if (player.aimingState.chargingPower < player.aimingState.maxCharge)
                {
                    ChangeText($"RMB: Charge {player.equippedHandItem.itemSO.itemName}");
                }
                else
                {
                    ChangeText($"LMB: {player.equippedHandItem.itemSO.doActionType} {player.equippedHandItem.itemSO.itemName}");
                }
                return;
            }*/
            Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());
            RaycastHit[] rayHitList = Physics.RaycastAll(ray);
            foreach (RaycastHit rayHit in rayHitList)
            {
                if (rayHit.collider.isTrigger && rayHit.collider.GetComponentInParent<Hoverable>() != null)
                {
                    Hoverable hoverable = rayHit.collider.GetComponentInParent<Hoverable>();
                    if (hoverable.SpecialCase)
                    {
                        hoverable.DoSpecialCase();
                        ChangeText(rayHit.collider.GetComponentInParent<Hoverable>());
                        return;
                    }
                    else
                    {
                        ChangeText(rayHit.collider.GetComponentInParent<Hoverable>());
                        return;
                    }

                }
            }
        }
        else
        {
            return;
        }
        RemoveText();
    }

    private bool CheckIfHoveringOverUI()//UI should take priority
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>();

        List<RaycastResult> rayHitList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, rayHitList);
        foreach (RaycastResult rayHit in rayHitList)
        {
            var hoverable = rayHit.gameObject.GetComponent<Hoverable>();
            if (hoverable != null)
            {
                if (hoverable.SpecialCase)
                {
                    hoverable.DoSpecialCase();
                }
                if (hoverable.Name != "" || hoverable.Prefix != "")
                {
                    ChangeText(hoverable);
                }
                return true;
            }
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            RemoveText();
            return true;
        }
        return false;
    }

    public void RemoveText()
    {
        hoverText.text = "";
    }

    public void ChangeText(string val)
    {
        if (val.Contains("LMB"))
        {
            val = val.Remove(0, 3);
            val = val.Insert(0, "<sprite name=\"LMB\">");
        }
        if (val.Contains("RMB"))
        {
            val = val.Remove(0, 3);
            val = val.Insert(0, "<sprite name=\"RMB\">");
        }
        if (val.Contains("MMB"))
        {
            val = val.Remove(0, 3);
            val = val.Insert(0, "<sprite name=\"MMB\">");
        }
        hoverText.text = val;
    }

    public void ChangeText(Hoverable hover)
    {
        if (hover.Prefix != "")
        {
            hoverText.text = $"{hover.Prefix}{hover.Name}";
        }
        else if (hover.Name != "")
        {
            hoverText.text = hover.Name;
        }

        if (hover.ShiftCase && player.playerInput.PlayerDefault.SpecialModifier.ReadValue<float>() == 1f)
        {
            hoverText.text = $"{hover.ShiftPrefix}{hover.Name}";
        }
        else if (hover.ControlCase && player.playerInput.PlayerDefault.SecondSpecialModifier.ReadValue<float>() == 1f)
        {
            hoverText.text = $"{hover.ControlPrefix}{hover.Name}";
        }

        if (hover.Prefix != null && hoverText.text.Contains("LMB"))
        {
            hoverText.text = hoverText.text.Remove(0, 3);
            hoverText.text = hoverText.text.Insert(0, "<sprite name=\"LMB\">");
        }
        if (hover.Prefix != null && hoverText.text.Contains("RMB"))
        {
            hoverText.text = hoverText.text.Remove(0, 3);
            hoverText.text = hoverText.text.Insert(0, "<sprite name=\"RMB\">");
        }
        if (hover.Prefix != null && hoverText.text.Contains("MMB"))
        {
            hoverText.text = hoverText.text.Remove(0, 3);
            hoverText.text = hoverText.text.Insert(0, "<sprite name=\"MMB\">");
        }
    }
}
