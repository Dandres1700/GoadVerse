using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFootballAnimationsTest : MonoBehaviour
{
    private Animator animator;

    [Header("Nombre exacto de la escena de la cancha")]
    [SerializeField] private string canchaSceneName = "minijuego1";

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
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("PASS ACTIVADO");
            animator.SetTrigger("Pass");
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
}