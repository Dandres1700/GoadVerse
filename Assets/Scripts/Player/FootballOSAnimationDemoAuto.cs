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
    [SerializeField] private float ballForwardOffset = 1.1f;
    [SerializeField] private float ballHeight = 0.35f;

    [Header("Fuerza del balon")]
    [SerializeField] private float passForce = 3.5f;
    [SerializeField] private float shootForce = 8f;
    [SerializeField] private float passLiftForce = 0.2f;
    [SerializeField] private float shootLiftForce = 0.8f;

    private Animator playerAnimator;
    private Transform playerTransform;
    private Rigidbody ballRb;

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
            }

            if (ball != null)
            {
                ballRb = ball.GetComponent<Rigidbody>();
            }

            yield return null;
        }

        Debug.Log("FOOTBALL OS: Jugador y balon encontrados");

        PrepareBallInFrontOfPlayer();

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
            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Pass");
            PrepareBallInFrontOfPlayer();
            playerAnimator.SetTrigger("Pass");

            yield return new WaitForSeconds(0.6f);
            KickBall(passForce, passLiftForce);

            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Shoot");
            PrepareBallInFrontOfPlayer();
            playerAnimator.SetTrigger("Shoot");

            yield return new WaitForSeconds(0.7f);
            KickBall(shootForce, shootLiftForce);

            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Celebrate");
            playerAnimator.SetTrigger("Celebrate");

            yield return new WaitForSeconds(3f);

        } while (loopDemo);
    }

    private void PrepareBallInFrontOfPlayer()
    {
        if (ballRb == null || playerTransform == null) return;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        Vector3 ballPosition = playerTransform.position
                               + playerTransform.forward * ballForwardOffset
                               + Vector3.up * ballHeight;

        ballRb.position = ballPosition;
        ballRb.rotation = Quaternion.identity;
    }

    private void KickBall(float force, float lift)
    {
        if (ballRb == null || playerTransform == null) return;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        Vector3 direction = playerTransform.forward;
        direction.y += lift;

        ballRb.AddForce(direction.normalized * force, ForceMode.Impulse);

        Debug.Log("FOOTBALL OS: Balon impulsado");
    }
}