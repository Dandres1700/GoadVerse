using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(10000)]
public class FootballOSOperatorCamera : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Objetivo")]
    [SerializeField] private string ballName = "Ball";

    [Header("Vista")]
    [SerializeField] private float distance = 22f;
    [SerializeField] private float height = 12f;
    [SerializeField] private float yaw = 0f;
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float lookHeight = 1.2f;

    [Header("Control de cámara")]
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float zoomSpeed = 6f;
    [SerializeField] private float minDistance = 12f;
    [SerializeField] private float maxDistance = 35f;

    private Transform ball;

    private void LateUpdate()
    {
        if (SceneManager.GetActiveScene().name != canchaSceneName) return;

        DisableThirdPersonCamera();

        if (ball == null)
        {
            GameObject ballObject = GameObject.Find(ballName);
            if (ballObject != null)
            {
                ball = ballObject.transform;
            }
        }

        if (ball == null) return;

        HandleCameraInput();

        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, -distance);

        Vector3 targetPosition = ball.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.unscaledDeltaTime);

        Vector3 lookTarget = ball.position + Vector3.up * lookHeight;
        transform.LookAt(lookTarget);
    }

    private void HandleCameraInput()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotateSpeed * Time.unscaledDeltaTime;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    private void DisableThirdPersonCamera()
    {
        Behaviour thirdPersonCamera = GetComponent("ThirdPersonCamera") as Behaviour;

        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.enabled = false;
        }
    }
}