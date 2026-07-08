using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FootballOSAnimationDemoAuto : MonoBehaviour
{
    [Header("Nombre exacto de la escena de la cancha")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Nombres de objetos")]
    [SerializeField] private string playerObjectName = "HumanM_Model";
    [SerializeField] private string ballObjectName = "Ball";

    [Header("Demo")]
    [SerializeField] private bool playDemoOnStart = true;
    [SerializeField] private bool loopDemo = false;

    [Header("Posicion del balon")]
    [SerializeField] private float ballForwardOffset = 1.2f;
    [SerializeField] private float ballHeight = 0.25f;

    [Header("Disparo")]
    [SerializeField] private float kickDelay = 0.55f;
    [SerializeField] private float shootForce = 0.5f;
    [SerializeField] private float shootLiftForce = 0.05f;

    [Header("Control del balon en la demo")]
    [SerializeField] private float stopBallAfterSeconds = 1.4f;
    [SerializeField] private float visibleBallDistance = 5f;

    private Animator playerAnimator;
    private Transform playerTransform;
    private Rigidbody ballRb;
    private Collider ballCollider;
    private CharacterController playerController;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != canchaSceneName)
        {
            return;
        }

        StartCoroutine(FindObjectsAndStartDemo());
    }

    private IEnumerator FindObjectsAndStartDemo()
    {
        while (playerAnimator == null || ballRb == null)
        {
            GameObject player = GameObject.Find(playerObjectName);
            GameObject ball = GameObject.Find(ballObjectName);

            if (player != null)
            {
                playerAnimator = player.GetComponentInChildren<Animator>();
                playerTransform = player.transform;
                playerController = player.GetComponent<CharacterController>();
            }

            if (ball != null)
            {
                ballRb = ball.GetComponent<Rigidbody>();
                ballCollider = ball.GetComponent<Collider>();
            }

            yield return null;
        }

        if (playerController != null && ballCollider != null)
        {
            Physics.IgnoreCollision(playerController, ballCollider, true);
        }

        Debug.Log("FOOTBALL OS: Jugador y balon encontrados");

        if (playDemoOnStart)
        {
            StartCoroutine(DemoRoutine());
        }
    }

    private IEnumerator DemoRoutine()
    {
        do
        {
            Debug.Log("FOOTBALL OS: Esperando primera jugada...");
            PrepareBallInFrontOfPlayer();
            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Receive");
            PrepareBallInFrontOfPlayer();
            playerAnimator.SetTrigger("Receive");
            yield return new WaitForSeconds(1.5f);

            Debug.Log("FOOTBALL OS: Shoot");
            PrepareBallInFrontOfPlayer();
            playerAnimator.SetTrigger("Shoot");

            yield return new WaitForSeconds(kickDelay);

            KickBall();

            yield return new WaitForSeconds(2.2f);

            Debug.Log("FOOTBALL OS: Celebrate");
            playerAnimator.SetTrigger("Celebrate");

            yield return new WaitForSeconds(3f);

        } while (loopDemo);
    }

    private void PrepareBallInFrontOfPlayer()
    {
        if (ballRb == null || playerTransform == null) return;

        ballRb.isKinematic = true;
        ballRb.useGravity = false;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        Vector3 ballPosition = playerTransform.position
                               + playerTransform.forward * ballForwardOffset
                               + Vector3.up * ballHeight;

        ballRb.position = ballPosition;
        ballRb.rotation = Quaternion.identity;
    }

    private void KickBall()
    {
        if (ballRb == null || playerTransform == null) return;

        ballRb.isKinematic = false;
        ballRb.useGravity = true;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        Vector3 direction = playerTransform.forward;
        direction.y += shootLiftForce;

        ballRb.AddForce(direction.normalized * shootForce, ForceMode.Impulse);

        Debug.Log("FOOTBALL OS: Balon disparado");

        StartCoroutine(StopBallForDemo());
    }

    private IEnumerator StopBallForDemo()
    {
        yield return new WaitForSeconds(stopBallAfterSeconds);

        if (ballRb == null || playerTransform == null) yield break;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        Vector3 visiblePosition = playerTransform.position
                                  + playerTransform.forward * visibleBallDistance
                                  + Vector3.up * ballHeight;

        ballRb.position = visiblePosition;
        ballRb.isKinematic = true;
        ballRb.useGravity = false;

        Debug.Log("FOOTBALL OS: Balon detenido para la demo");
    }
}
