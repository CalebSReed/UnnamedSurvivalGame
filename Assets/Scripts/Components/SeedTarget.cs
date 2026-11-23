using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SeedTarget : NetworkBehaviour
{
    public bool hasSeed;
    RealWorldObject obj;
    Pickable pickable;

    private void Awake()
    {
        obj = GetComponent<RealWorldObject>();
        obj.hasSpecialInteraction = true;
        obj.interactEvent.AddListener(CheckSeed);

        obj.hoverBehavior.specialCaseModifier.AddListener(CheckItems);
        obj.hoverBehavior.SpecialCase = true;

        hasSeed = true;
        pickable = GetComponent<Pickable>();

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
            obj.hoverBehavior.Prefix = $"{PlayerMain.Instance.playerInput.PlayerDefault.Interact.GetBindingDisplayString()}: Shear ";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
        else if (pickable == null)
        {
            obj.hoverBehavior.Prefix = "";
            obj.hoverBehavior.Name = obj.woso.objName;
        }
        else
        {
            obj.hoverBehavior.Prefix = "E: Pick ";
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
                Item itemToDrop = new Item { itemSO = obj.woso.seed, amount = 1 };
                itemToDrop.SaveData();

                RealItem.DropItem(itemToDrop, transform.position, true);
                int randVal = UnityEngine.Random.Range(1, 4);
                GameManager.Instance.localPlayerMain.audio.Play($"Collect{randVal}", transform.position, gameObject, true);
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
