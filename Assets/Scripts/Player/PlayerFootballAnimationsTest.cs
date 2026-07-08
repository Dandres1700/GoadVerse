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
    }
}