using UnityEngine;

/// Camara orbital simple en tercera persona.
/// Ponla en la Main Camera y asigna el Transform del jugador en "target".

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; // El jugador

    [Header("Configuracion")]
    public Vector3 offset = new Vector3(0f, 2.5f, -4.5f);
    public float sensitivity = 3f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    private float yaw;
    private float pitch = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}