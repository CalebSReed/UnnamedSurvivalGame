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
        if (target != null && enemyTarget == null)
        {
            yRot += input.PlayerDefault.MouseDelta.ReadValue<Vector2>().x * mouseXSens;//whoops do not multiply mouse movement by deltatime! It's already frame independent! Unless we were using mouseX and mouseY axis movement those are frame dependent for some reason?!?!
        }
    }
    float ClampAngle(float angle, float from, float to)//thank u unity forums
    {
        // accepts e.g. -80, 80
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
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
                yRot = rotRef.localEulerAngles.y;
            }
            else if (controlsEnabled)
            {
                rotRef.rotation = Quaternion.Euler(0, yRot, 0);
            }

            Vector3 targetPosition = target.position + offset;
            camPivot.position = Vector3.Lerp(camPivot.position, new Vector3(targetPosition.x, targetPosition.y, targetPosition.z), smoothTime);
        }
        rotRef.eulerAngles = new Vector3(ClampAngle(rotRef.eulerAngles.x, -45, 9999f), rotRef.eulerAngles.y, rotRef.eulerAngles.z);//do not go lower than -45f on X rot or else we clip underneath the ground
        camPivot.transform.rotation = rotRef.rotation;//Quaternion.Lerp(camPivot.rotation, rotRef.rotation, rotSpeed * Time.deltaTime);

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
                yRot = rotRef.localEulerAngles.y;
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
