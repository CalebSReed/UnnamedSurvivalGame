using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    public enum DoorType
    {
        Horizontal,
        HorizontalMirrored,
        Vertical,
        VerticalMirrored
    }

    public bool isOpen;
    public DoorType doorType;
    public GameObject player;//later check for doorOpeners probably
    public Sprite hDoor;
    public Sprite hDoorM;
    public Sprite vDoor;
    public Sprite vDoorM;
    public SpriteRenderer objSprite;
    public RealWorldObject realObj;

    private void Start()
    {
        hDoor = WosoArray.Instance.SearchWOSOList("wooddoorh").objSprite;
        hDoorM = WosoArray.Instance.SearchWOSOList("wooddoorhmirrored").objSprite;
        vDoor = WosoArray.Instance.SearchWOSOList("wooddoorv").objSprite;
        vDoorM = WosoArray.Instance.SearchWOSOList("wooddoorvmirrored").objSprite;
        objSprite = GetComponent<RealWorldObject>().spriteRenderer;
        player = GameObject.FindGameObjectWithTag("Player");
        realObj = GetComponent<RealWorldObject>();
        realObj.interactEvent.AddListener(CheckToOpen);
        realObj.hasSpecialInteraction = true;

        if (realObj.obj.woso.isHWall && !realObj.obj.woso.isMirrored)
        {
            doorType = DoorType.Horizontal;
        }
        else if (realObj.obj.woso.isHWall && realObj.obj.woso.isMirrored)
        {
            doorType = DoorType.HorizontalMirrored;
        }
        else if (realObj.obj.woso.isVWall && !realObj.obj.woso.isMirrored)
        {
            doorType = DoorType.Vertical;
        }
        else if (realObj.obj.woso.isVWall && realObj.obj.woso.isMirrored)
        {
            doorType = DoorType.VerticalMirrored;
        }
        /*
        switch (doorType)
        {
            case DoorType.Horizontal:
                SetToHDoor();
                break;
            case DoorType.HorizontalMirrored:
                SetToHDoorM();
                break;
            case DoorType.Vertical:
                SetToVDoor();
                break;
            case DoorType.VerticalMirrored:
                SetToVDoorM();
                break;
        }*/
    }

    private void OnMouseDown()
    {
        /*if (Vector2.Distance(player.transform.position, transform.position) < 12)
        {
            ToggleOpen();
        }*/
    }

    public void CheckToOpen()
    {
        if (Vector3.Distance(player.transform.position, transform.position) > 15)
        {
            return;
        }
        else
        {
            ToggleOpen();
        }
    }

    public void ToggleOpen()
    {
        int rand = Random.Range(1, 6);
        realObj.audio.Play($"Door{rand}", transform.position, gameObject);

        if (isOpen)
        {
            isOpen = false;
            transform.Rotate(new Vector3(0, -90, 0));
            transform.position = new Vector3(transform.position.x + 3, transform.position.y, transform.position.z+2.5f);
        }
        else
        {
            isOpen = true;
            transform.Rotate(new Vector3(0, 90, 0));
            transform.position = new Vector3(transform.position.x - 3, transform.position.y, transform.position.z-2.5f);
        }

        /*
        if (isOpen)
        {
            isOpen = false;
            switch (doorType)
            {
                case DoorType.Horizontal:
                    SetToHDoor();
                    transform.position = new Vector3(transform.position.x + 2.7f, transform.position.y + 5f, transform.position.z);
                    break;
                case DoorType.HorizontalMirrored:
                    SetToHDoorM();
                    transform.position = new Vector3(transform.position.x - 2.7f, transform.position.y + 5f, transform.position.z);
                    break;
                case DoorType.Vertical:
                    SetToVDoor();
                    transform.position = new Vector3(transform.position.x - 2.7f, transform.position.y + .75f, transform.position.z);
                    break;
                case DoorType.VerticalMirrored:
                    SetToVDoorM();
                    transform.position = new Vector3(transform.position.x + 2.7f, transform.position.y + .75f, transform.position.z);
                    break;
            }
        }
        else
        {
            isOpen = true;
            switch (doorType)
            {
                case DoorType.Horizontal:
                    SetToVDoor();
                    transform.position = new Vector3(transform.position.x - 2.7f, transform.position.y - 5f, transform.position.z);
                    break;
                case DoorType.HorizontalMirrored:
                    SetToVDoorM();
                    transform.position = new Vector3(transform.position.x + 2.7f, transform.position.y - 5f, transform.position.z);
                    break;
                case DoorType.Vertical:
                    SetToHDoor();
                    transform.position = new Vector3(transform.position.x + 2.7f, transform.position.y - .75f, transform.position.z);
                    break;
                case DoorType.VerticalMirrored:
                    SetToHDoorM();
                    transform.position = new Vector3(transform.position.x - 2.7f, transform.position.y - .75f, transform.position.z);
                    break;
            }
        }*/
    }

    private void SetToHDoor()
    {
        objSprite.sprite = hDoor;

        GetComponents<BoxCollider2D>()[1].size = new Vector2(7, 2.6f);
        GetComponents<BoxCollider2D>()[1].offset = new Vector2(.1f, .9f);

        GetComponents<BoxCollider2D>()[2].size = new Vector2(7, 8);
        GetComponents<BoxCollider2D>()[2].offset = new Vector2(0, 3.6f);
    }

    private void SetToHDoorM()
    {
        objSprite.sprite = hDoorM;

        GetComponents<BoxCollider2D>()[1].size = new Vector2(7, 2.6f);
        GetComponents<BoxCollider2D>()[1].offset = new Vector2(.1f, .9f);

        GetComponents<BoxCollider2D>()[2].size = new Vector2(7, 8);
        GetComponents<BoxCollider2D>()[2].offset = new Vector2(0, 3.6f);
    }

    private void SetToVDoor()
    {
        objSprite.sprite = vDoor;

        GetComponents<BoxCollider2D>()[1].size = new Vector2(3.7f, 7.5f);
        GetComponents<BoxCollider2D>()[1].offset = new Vector2(0, 3);

        GetComponents<BoxCollider2D>()[2].size = new Vector2(1.7f, 12.5f);
        GetComponents<BoxCollider2D>()[2].offset = new Vector2(0, 6);
    }

    private void SetToVDoorM()
    {
        objSprite.sprite = vDoorM;

        GetComponents<BoxCollider2D>()[1].size = new Vector2(3.7f, 7.5f);
        GetComponents<BoxCollider2D>()[1].offset = new Vector2(0, 3);

        GetComponents<BoxCollider2D>()[2].size = new Vector2(1.7f, 12.5f);
        GetComponents<BoxCollider2D>()[2].offset = new Vector2(0, 6);
    }
}
