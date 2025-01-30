using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DoorBehavior : NetworkBehaviour
{
    public bool isOpen;
    public RealWorldObject realObj;

    private void Start()
    {
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
        if (Vector3.Distance(GameManager.Instance.localPlayer.transform.position, transform.position) > 15)
        {
            return;
        }
        else
        {
            if (GameManager.Instance.isServer)
            {
                ToggleOpen();
            }
            else
            {
                AskToToggleRPC();
            }
        }
    }

    public void ToggleOpen()
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
    }

    [Rpc(SendTo.Server)]
    private void AskToToggleRPC()
    {
        ToggleOpen();
    }
}
