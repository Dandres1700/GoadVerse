using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(10000)]
public class MundialSystemCameraController : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] private string matchSceneName = "minijuego1";

    [Header("Referencias")]
    [SerializeField] private string ballName = "Ball";
    [SerializeField] private string manualPlayerName = "HumanM_Model";

    [Header("Intro al estadio")]
    [SerializeField] private bool playIntroOnStart = true;

    [SerializeField] private Vector3 introStartPosition = new Vector3(-4f, 18f, -45f);
    [SerializeField] private Vector3 introStartLookAt = new Vector3(-4f, 1f, 0f);

    [SerializeField] private Vector3 stadiumPosition = new Vector3(-4f, 24f, -35f);
    [SerializeField] private Vector3 stadiumLookAt = new Vector3(-4f, 0f, 5f);

    [SerializeField] private Vector3 teamsPosition = new Vector3(-15f, 10f, -14f);
    [SerializeField] private Vector3 teamsLookAt = new Vector3(-4f, 1f, 5f);

    [SerializeField] private Vector3 sideScanPosition = new Vector3(-28f, 14f, -18f);
[SerializeField] private Vector3 sideScanLookAt = new Vector3(-4f, 1f, 0f);

[SerializeField] private Vector3 goalkeeperPosition = new Vector3(-4f, 6f, -30f);
[SerializeField] private Vector3 goalkeeperLookAt = new Vector3(-4f, 1f, -18f);

[SerializeField] private Vector3 ballPosition = new Vector3(-8f, 5f, -8f);
[SerializeField] private Vector3 ballLookAt = new Vector3(-4f, 1f, 0f);

    [Header("Tiempos de cámara")]
    [SerializeField] private float firstShotTime = 0.4f;
[SerializeField] private float stadiumShotTime = 0.7f;
[SerializeField] private float teamsShotTime = 0.8f;
[SerializeField] private float transitionTime = 0.55f;
    

    [Header("Cámara táctica")]
    [SerializeField] private float tacticalDistance = 24f;
    [SerializeField] private float tacticalHeight = 13f;
    [SerializeField] private float tacticalYaw = 0f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float lookHeight = 1.2f;

    [Header("Control manual de cámara")]
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float zoomSpeed = 8f;
    [SerializeField] private float minDistance = 12f;
    [SerializeField] private float maxDistance = 40f;

    private Transform ball;
    private bool introRunning;
    private bool tacticalCameraActive;
    private bool introComplete;

    public bool IsIntroRunning => introRunning;
    public bool IsIntroComplete => !playIntroOnStart || introComplete;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != matchSceneName)
        {
            return;
        }

        DisableManualPlayer();
        DisableOldCameraScripts();

        FindBall();

        if (playIntroOnStart)
        {
            introComplete = false;
            StartCoroutine(IntroRoutine());
        }
        else
        {
            introComplete = true;
            tacticalCameraActive = true;
        }
    }

    private void LateUpdate()
    {
        if (SceneManager.GetActiveScene().name != matchSceneName)
        {
            return;
        }

        DisableManualPlayer();
        DisableOldCameraScripts();

        if (ball == null)
        {
            FindBall();
        }

        if (introRunning || !tacticalCameraActive || ball == null)
        {
            return;
        }

        HandleCameraInput();
        FollowBallTactical();
    }

    private void FindBall()
    {
        GameObject ballObject = GameObject.Find(ballName);

        if (ballObject != null)
        {
            ball = ballObject.transform;
        }
    }

    private IEnumerator IntroRoutine()
{
    introRunning = true;
    tacticalCameraActive = false;

    // Toma 1: entrada rápida al estadio
    SetCameraInstant(introStartPosition, introStartLookAt);
    yield return new WaitForSecondsRealtime(firstShotTime);

    // Toma 2: estadio completo
    yield return MoveCameraTo(stadiumPosition, stadiumLookAt, transitionTime);
    yield return new WaitForSecondsRealtime(stadiumShotTime);

    // Toma 3: barrido lateral del escenario
    yield return MoveCameraTo(sideScanPosition, sideScanLookAt, transitionTime);
    yield return new WaitForSecondsRealtime(0.6f);

    // Toma 4: jugadores/equipo
    yield return MoveCameraTo(teamsPosition, teamsLookAt, transitionTime);
    yield return new WaitForSecondsRealtime(teamsShotTime);

    // Toma 5: arquero
    yield return MoveCameraTo(goalkeeperPosition, goalkeeperLookAt, transitionTime);
    yield return new WaitForSecondsRealtime(0.55f);

    // Toma 6: balón / zona de inicio
    yield return MoveCameraTo(ballPosition, ballLookAt, transitionTime);
    yield return new WaitForSecondsRealtime(0.5f);

    // Activa cámara táctica final
    tacticalCameraActive = true;
    introRunning = false;
    introComplete = true;
}

    private void FollowBallTactical()
    {
        Quaternion rotation = Quaternion.Euler(0f, tacticalYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, tacticalHeight, -tacticalDistance);

        Vector3 targetPosition = ball.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.unscaledDeltaTime
        );

        Vector3 lookTarget = ball.position + Vector3.up * lookHeight;
        transform.LookAt(lookTarget);
    }

    private void HandleCameraInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButton(1))
        {
            tacticalYaw += Input.GetAxis("Mouse X") * rotateSpeed * Time.unscaledDeltaTime;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            tacticalDistance -= scroll * zoomSpeed;
            tacticalDistance = Mathf.Clamp(tacticalDistance, minDistance, maxDistance);
        }
    }

    private IEnumerator MoveCameraTo(Vector3 targetPosition, Vector3 lookAt, float duration)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Quaternion targetRotation = GetLookRotation(targetPosition, lookAt);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    private void SetCameraInstant(Vector3 position, Vector3 lookAt)
    {
        transform.position = position;
        transform.rotation = GetLookRotation(position, lookAt);
    }

    private Quaternion GetLookRotation(Vector3 position, Vector3 lookAt)
    {
        Vector3 direction = lookAt - position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return transform.rotation;
        }

        return Quaternion.LookRotation(direction);
    }

    private void DisableManualPlayer()
    {
        GameObject manualPlayer = GameObject.Find(manualPlayerName);

        if (manualPlayer == null)
        {
            return;
        }

        Behaviour movement = manualPlayer.GetComponent("PlayerMovement3D") as Behaviour;

        if (movement != null)
        {
            movement.enabled = false;
        }

        CharacterController controller = manualPlayer.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        Renderer[] renderers = manualPlayer.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        manualPlayer.transform.position = new Vector3(0f, -100f, 0f);
    }

    private void DisableOldCameraScripts()
    {
        Behaviour thirdPersonCamera = GetComponent("ThirdPersonCamera") as Behaviour;

        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.enabled = false;
        }

        Behaviour cinematicCamera = GetComponent("FootballOSCinematicCamera") as Behaviour;

        if (cinematicCamera != null)
        {
            cinematicCamera.enabled = false;
        }

        Behaviour cameraOverride = GetComponent("FootballOSCameraOverride") as Behaviour;

        if (cameraOverride != null)
        {
            cameraOverride.enabled = false;
        }
    }
}
