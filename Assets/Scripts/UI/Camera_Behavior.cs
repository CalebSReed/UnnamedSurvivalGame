using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class Camera_Behavior : MonoBehaviour
{
    public static Camera_Behavior Instance { get; private set; }

    public bool controlsEnabled = true;

    public float rotSpeed;
    [SerializeField] private Vector3 offset = new Vector3(0f, 25f, 0f);
    [SerializeField] private float smoothTime = 0.1f;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private Transform camPivot;
    public Transform rotRef;

    private Transform target;
    private GameObject player;
    private PlayerInputActions input;

    [SerializeField] float mouseXSens;
    float yRot;
    public Transform enemyTarget;
    public bool targetLocked;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnLocalPlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(object sender, System.EventArgs e)
    {
        player = GameManager.Instance.localPlayer;
        input = player.GetComponent<PlayerMain>().playerInput;
        target = player.transform;
        LockCursor(true);
    }

    public void LockCursor(bool lockOn)
    {
        if (lockOn)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            controlsEnabled = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            controlsEnabled = false;
        } 
    }

    private void Update()
    {
        if (target != null)
        {
            yRot += input.PlayerDefault.MouseDelta.ReadValue<Vector2>().x * Time.deltaTime * mouseXSens;
        }
    }

    void LateUpdate()//which to choose.....
    {
        if (target != null)
        {
            if (targetLocked && enemyTarget != null)
            {
                var rot = Quaternion.LookRotation(enemyTarget.position - player.transform.position, Vector3.up);
                rotRef.rotation = rot;
            }
            else if (targetLocked && enemyTarget == null)
            {
                targetLocked = false;
            }
            else if (controlsEnabled)
            {
                rotRef.rotation = Quaternion.Euler(0, yRot, 0);
            }

            Vector3 targetPosition = target.position + offset;
            camPivot.position = Vector3.Lerp(camPivot.position, new Vector3(targetPosition.x, targetPosition.y, targetPosition.z), smoothTime);
        }
        camPivot.transform.rotation = Quaternion.Lerp(camPivot.rotation, rotRef.rotation, rotSpeed * Time.deltaTime);

    }
    
    public void LockOnTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!targetLocked && GameManager.Instance.localPlayerMain.enemyList.Count > 0)
            {
                List<float> distances = new();
                foreach (var enemy in GameManager.Instance.localPlayerMain.enemyList)
                {
                    distances.Add(Vector3.Distance(enemy.transform.position, player.transform.position));
                }

                var min = distances.Min();

                foreach (var enemy in GameManager.Instance.localPlayerMain.enemyList)
                {
                    if (Vector3.Distance(enemy.transform.position, player.transform.position) == min)
                    {
                        enemyTarget = enemy.transform;
                        targetLocked = !targetLocked;
                        break;
                    }
                }
            }
            else
            {
                enemyTarget = null;
                targetLocked = false;
            }
        }
    }

    public void RotateCamLeft(InputAction.CallbackContext context)
    {
        if (context.performed && controlsEnabled)
        {
            //rotRef.Rotate(Vector3.up, 45);
        }
    }

    public void RotateCamRight(InputAction.CallbackContext context)
    {
        if (context.performed && controlsEnabled)
        {
            //rotRef.Rotate(Vector3.up, -45);
        }
    }

    /*private IEnumerator SmoothRotation()
    {
        while (camPivot.rotation != newRot)
        camPivot.rotation = Vector3.SmoothDamp();
        yield return null;
    }*/
}
