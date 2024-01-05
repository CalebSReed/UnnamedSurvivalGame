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
        if (gameManager.pauseMenu.activeSelf)//For use in specific situations like pausing the game
        {
            RemoveText();
            return;
        }
        else if (!CheckIfHoveringOverUI())
        {
            Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());//this might cause bugs calling in physics update
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
            if (rayHit.gameObject.GetComponent<Hoverable>() != null && rayHit.gameObject.GetComponent<Hoverable>().Name != "")
            {
                ChangeText(rayHit.gameObject.GetComponent<Hoverable>());
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
    }
}
