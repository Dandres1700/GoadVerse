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

    [Header("Fuerza del balón")]
    [SerializeField] private float passForce = 6f;
    [SerializeField] private float shootForce = 14f;
    [SerializeField] private float liftForce = 2f;

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

        Debug.Log("FOOTBALL OS: Jugador y balón encontrados");

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
            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Receive");
            playerAnimator.SetTrigger("Receive");
            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Pass");
            playerAnimator.SetTrigger("Pass");
            yield return new WaitForSeconds(0.6f);
            KickBall(passForce, 0.4f);

            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Shoot");
            playerAnimator.SetTrigger("Shoot");
            yield return new WaitForSeconds(0.7f);
            KickBall(shootForce, liftForce);

            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Celebrate");
            playerAnimator.SetTrigger("Celebrate");

            yield return new WaitForSeconds(3f);

        } while (loopDemo);
    }

    private void KickBall(float force, float lift)
    {
        if (ballRb == null || playerTransform == null) return;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        Vector3 direction = playerTransform.forward;
        direction.y += lift;

        ballRb.AddForce(direction.normalized * force, ForceMode.Impulse);

        Debug.Log("FOOTBALL OS: Balón impulsado");
    }
}