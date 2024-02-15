using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedTarget : MonoBehaviour
{
    private bool hasSeed;
    RealWorldObject obj;

    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.hasSpecialInteraction = true;
        obj.interactEvent.AddListener(CheckSeed);

        obj.hoverBehavior.specialCaseModifier.AddListener(CheckItems);
        obj.hoverBehavior.SpecialCase = true;

        hasSeed = true;

        obj.onSaved += OnSave;
        obj.onLoaded += OnLoad;
    }

    private void CheckItems()
    {
        if (obj.playerMain.doAction == Action.ActionType.Shear && hasSeed)
        {
            obj.hoverBehavior.Prefix = "RMB: Shear ";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
        else
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
    }

    private void CheckSeed()
    {
        if (obj.playerMain.doAction == Action.ActionType.Shear && hasSeed)
        {
            hasSeed = false;
            RealItem.DropItem(new Item { itemSO = obj.woso.seed, amount = 1 }, transform.position, true);
        }
    }

    private void OnSave(object sender, System.EventArgs e)
    {
        obj.saveData.hasSeed = hasSeed;
    }

    private void OnLoad(object sender, System.EventArgs e)
    {
        hasSeed = obj.saveData.hasSeed;
    }
}
