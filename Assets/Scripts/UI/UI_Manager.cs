using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;
    GameObject selectedObj;
    [SerializeField] Camera ui_cam;
    [SerializeField] Canvas canvas;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        if (selectedObj != null)
        {
            Vector3 _pos = PlayerMain.Instance.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>();
            _pos.z = canvas.planeDistance;
            selectedObj.transform.position = ui_cam.ScreenToWorldPoint(_pos);
        }
    }

    public void OnSelectObject(GameObject obj)
    {
        if (selectedObj == null)
        {
            selectedObj = obj;
            obj.transform.GetChild(0).GetComponent<Image>().raycastTarget = false;
        }
    }

    public void OnSelectButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed && selectedObj != null)
        {
            selectedObj.transform.GetChild(0).GetComponent<Image>().raycastTarget = true;
            selectedObj = null;
        }
    }

    public void OnCancelButtonDown(InputAction.CallbackContext context)
    {
        if (context.performed && selectedObj != null)
        {
            selectedObj.transform.GetChild(0).GetComponent<Image>().raycastTarget = true;
            selectedObj = null;
        }
    }
}
