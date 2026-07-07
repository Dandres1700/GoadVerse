using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    [Header("Posición")]
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public float smoothSpeed = 10f;

    [Header("Rotación")]
    public float sensitivity = 3f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("Mando")]
    [SerializeField] private bool enableGamepadLook = true;
    [SerializeField] private float gamepadLookSensitivity = 120f;
    [SerializeField] private float gamepadLookDeadZone = 0.15f;

    [Header("Zoom")]
    public float minZoomDistance = 2.5f;
    public float maxZoomDistance = 10f;
    public float zoomSpeed = 6f;

    private float yaw;
    private float pitch = 20f;

    private void Start()
    {
        ClampZoomDistance();
        LockCursor(true);
    }

    private void LateUpdate()
    {
        if (target == null) return;
        if (target == transform) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(false);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            LockCursor(true);
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;

            Vector2 gamepadLook = GetGamepadLookInput();
            yaw += gamepadLook.x * gamepadLookSensitivity * Time.deltaTime;
            pitch -= gamepadLook.y * gamepadLookSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        HandleZoom();

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    private Vector2 GetGamepadLookInput()
    {
        if (!enableGamepadLook) return Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)
        {
            Vector2 look = Gamepad.current.rightStick.ReadValue();

            if (look.sqrMagnitude > gamepadLookDeadZone * gamepadLookDeadZone)
            {
                return look;
            }
        }
#endif

        return Vector2.zero;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) <= 0.001f) return;

        float zoomDirection = offset.z < 0f ? -1f : 1f;
        float zoomDistance = Mathf.Abs(offset.z);
        zoomDistance = Mathf.Clamp(
            zoomDistance - scroll * zoomSpeed,
            minZoomDistance,
            maxZoomDistance
        );

        offset.z = zoomDistance * zoomDirection;
    }

    private void ClampZoomDistance()
    {
        float zoomDirection = offset.z < 0f ? -1f : 1f;
        float zoomDistance = Mathf.Clamp(Mathf.Abs(offset.z), minZoomDistance, maxZoomDistance);
        offset.z = zoomDistance * zoomDirection;
    }

    private static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
