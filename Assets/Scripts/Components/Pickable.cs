using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour
{
    private RealWorldObject obj;
    private RealItem realItem;
    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        if (obj != null)
        {
            obj.hasSpecialInteraction = true;
            obj.interactEvent.AddListener(OnInteract);
            obj.hoverBehavior.SpecialCase = true;
            obj.hoverBehavior.specialCaseModifier.AddListener(CheckConditions);
        }

        realItem = GetComponent<RealItem>();
        if (realItem != null)
        {
            realItem.interactEvent.AddListener(OnInteractItem);
            realItem.hoverBehavior.SpecialCase = true;
            realItem.hoverBehavior.specialCaseModifier.AddListener(CheckConditionsItem);
        }
    }

    private void OnInteractItem()
    {
        InteractArgs newArgs = new InteractArgs();
        newArgs.playerSender = GameManager.Instance.localPlayerMain;
        realItem.CollectItem(newArgs);
    }

    private void OnInteract()
    {
        obj.Break(false, GameManager.Instance.localPlayerMain);
    }

    private void CheckConditions()
    {
        obj.hoverBehavior.Prefix = "E: Pick ";
    }

    private void CheckConditionsItem()
    {
        realItem.hoverBehavior.Prefix = "E: Collect ";
    }
}
