using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFootballAnimationsTest : MonoBehaviour
{
    private Animator animator;
    private Rigidbody cachedBallRigidbody;

    [Header("Nombre exacto de la escena de la cancha")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Balon")]
    [SerializeField] private string ballObjectName = "Ball";
    [SerializeField] private float kickForce = 0.5f;
    [SerializeField] private float passKickDelay = 0.6f;
    [SerializeField] private float shootKickDelay = 0.7f;
    [SerializeField] private float passLiftForce = 0.2f;
    [SerializeField] private float shootLiftForce = 0.8f;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (animator == null) return;

        if (SceneManager.GetActiveScene().name != canchaSceneName)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("SHOOT ACTIVADO");
            animator.SetTrigger("Shoot");
            StartCoroutine(KickBallAfterDelay(shootKickDelay, shootLiftForce));
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("PASS ACTIVADO");
            animator.SetTrigger("Pass");
            StartCoroutine(KickBallAfterDelay(passKickDelay, passLiftForce));
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("RECEIVE ACTIVADO");
            animator.SetTrigger("Receive");
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("TACKLE ACTIVADO");
            animator.SetTrigger("Tackle");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("CELEBRATE ACTIVADO");
            animator.SetTrigger("Celebrate");
        }
    }

    private IEnumerator KickBallAfterDelay(float delay, float lift)
    {
        yield return new WaitForSeconds(delay);
        KickBall(lift);
    }

    private void KickBall(float lift)
    {
        Rigidbody ballRigidbody = GetBallRigidbody();

        if (ballRigidbody == null) return;

        ballRigidbody.isKinematic = false;
        ballRigidbody.useGravity = true;
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        Vector3 direction = transform.forward;
        direction.y += lift;

        ballRigidbody.AddForce(direction.normalized * kickForce, ForceMode.Impulse);
    }

    private Rigidbody GetBallRigidbody()
    {
        if (cachedBallRigidbody != null)
        {
            return cachedBallRigidbody;
        }

        GameObject ball = GameObject.Find(ballObjectName);

        if (ball == null) return null;

        cachedBallRigidbody = ball.GetComponent<Rigidbody>();
        return cachedBallRigidbody;
    }

    private void OnValidate()
    {
        kickForce = Mathf.Max(0f, kickForce);
        passKickDelay = Mathf.Max(0f, passKickDelay);
        shootKickDelay = Mathf.Max(0f, shootKickDelay);
    }
}
