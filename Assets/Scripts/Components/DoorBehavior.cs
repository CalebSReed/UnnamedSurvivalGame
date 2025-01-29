using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DoorBehavior : NetworkBehaviour
{
    public bool isOpen;
    public GameObject player;//later check for doorOpeners probably
    public RealWorldObject realObj;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        realObj = GetComponent<RealWorldObject>();
        realObj.interactEvent.AddListener(CheckToOpen);
        realObj.hasSpecialInteraction = true;

        realObj.hoverBehavior.SpecialCase = true;
        realObj.hoverBehavior.specialCaseModifier.AddListener(CheckState);
        realObj.hoverBehavior.Name = realObj.woso.objName;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //AskIfOpenRPC();
    }

    private void CheckState()
    {
        if (isOpen)
        {
            realObj.hoverBehavior.Prefix = "RMB: Close ";
        }
        else
        {
            realObj.hoverBehavior.Prefix = "RMB: Open ";
        }
    }

    public void CheckToOpen()
    {
        if (Vector3.Distance(player.transform.position, transform.position) > 15)
        {
            return;
        }
        else
        {
            if (GameManager.Instance.isServer)
            {
                ToggleOpen(false);
            }
            else
            {
                AskToToggleRPC();
            }
        }
    }

    public void ToggleOpen(bool askServer)
    {
        int rand = Random.Range(1, 6);
        realObj.audio.Play($"Door{rand}", transform.position, gameObject);

        if (isOpen)
        {
            isOpen = false;
            if (realObj.obj.woso.isMirrored)
            {
                transform.Rotate(new Vector3(0, 90, 0));
                transform.position = new Vector3(transform.position.x - 3, transform.position.y, transform.position.z - 2.5f);
            }
            else
            {
                transform.Rotate(new Vector3(0, -90, 0));
                transform.position = new Vector3(transform.position.x + 3, transform.position.y, transform.position.z + 2.5f);
            }
        }
        else
        {
            isOpen = true;
            if (realObj.obj.woso.isMirrored)
            {
                transform.Rotate(new Vector3(0, -90, 0));
                transform.position = new Vector3(transform.position.x + 3, transform.position.y, transform.position.z + 2.5f);
            }
            else
            {
                transform.Rotate(new Vector3(0, 90, 0));
                transform.position = new Vector3(transform.position.x - 3, transform.position.y, transform.position.z - 2.5f);
            }
        }

        if (GameManager.Instance.isServer)
        {
            ToggleDoorRPC();
        }
        if (askServer)
        {
            AskToToggleRPC();
        }
    }

    [Rpc(SendTo.Server)]
    private void AskToToggleRPC()
    {
        ToggleOpen(false);
    }

    [Rpc(SendTo.NotServer)]
    private void ToggleDoorRPC()
    {
        ToggleOpen(false);
    }
}
