using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(10000)]
public class FootballOSCameraOverride : MonoBehaviour
{
    [Header("Escena de cancha")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Camara fija para presentacion")]
    [SerializeField] private Vector3 cameraPosition = new Vector3(-8f, 7f, -7f);
    [SerializeField] private Vector3 lookAtPoint = new Vector3(-4f, 1.2f, 6f);

    [Header("Bloquear camara")]
    [SerializeField] private bool lockCamera = true;

    private void LateUpdate()
    {
        if (!lockCamera) return;

        if (SceneManager.GetActiveScene().name != canchaSceneName) return;

        DisableThirdPersonCamera();

        transform.position = cameraPosition;
        transform.LookAt(lookAtPoint);
    }

    private void DisableThirdPersonCamera()
    {
        ThirdPersonCamera thirdPersonCamera = GetComponent<ThirdPersonCamera>();

        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.enabled = false;
        }
    }
}