using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SeedTarget : NetworkBehaviour
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AskIfHasSeedRPC();
    }

    private void CheckItems()
    {
        if (GameManager.Instance.localPlayerMain.doAction == Action.ActionType.Shear && hasSeed)
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
        if (GameManager.Instance.localPlayerMain.doAction == Action.ActionType.Shear && hasSeed)
        {
            hasSeed = false;
            if (GameManager.Instance.isServer)
            {
                RealItem.DropItem(new Item { itemSO = obj.woso.seed, amount = 1 }, transform.position, true);
            }
            else
            {
                ClientHelper.Instance.AskToSpawnItemBasicRPC(transform.position, obj.woso.seed.itemType, true);
            }
            SeedWasTakenRPC();
            GameManager.Instance.localPlayerMain.UseEquippedItemDurability();
        }
    }

    [Rpc(SendTo.Server)]
    private void AskIfHasSeedRPC()
    {
        if (!hasSeed)
        {
            SeedWasTakenRPC();
        }
    }

    [Rpc(SendTo.NotMe)]
    private void SeedWasTakenRPC()
    {
        hasSeed = false;
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
