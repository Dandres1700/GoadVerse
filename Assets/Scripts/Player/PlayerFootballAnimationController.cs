using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFootballAnimationController : MonoBehaviour
{
    private Animator animator;

    private static readonly int ShootHash = Animator.StringToHash("Shoot");
    private static readonly int PassHash = Animator.StringToHash("Pass");
    private static readonly int ReceiveHash = Animator.StringToHash("Receive");
    private static readonly int TackleHash = Animator.StringToHash("Tackle");
    private static readonly int CelebrateHash = Animator.StringToHash("Celebrate");

    [Header("Nombre exacto de la escena de la cancha")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Solo para pruebas temporales")]
    [SerializeField] private bool enableKeyboardTest = true;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Esto es SOLO para probar en Unity.
        // En el juego final, esto se puede apagar desde el Inspector.
        if (!enableKeyboardTest) return;

        if (!IsInFootballScene()) return;

        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayShoot();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayPass();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayReceive();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayTackle();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayCelebrate();
        }
    }

    public void PlayShoot()
    {
        if (!CanPlayFootballAnimation()) return;

        Debug.Log("FOOTBALL OS -> SHOOT");
        animator.SetTrigger(ShootHash);
    }

    public void PlayPass()
    {
        if (!CanPlayFootballAnimation()) return;

        Debug.Log("FOOTBALL OS -> PASS");
        animator.SetTrigger(PassHash);
    }

    public void PlayReceive()
    {
        if (!CanPlayFootballAnimation()) return;

        Debug.Log("FOOTBALL OS -> RECEIVE");
        animator.SetTrigger(ReceiveHash);
    }

    public void PlayTackle()
    {
        if (!CanPlayFootballAnimation()) return;

        Debug.Log("FOOTBALL OS -> TACKLE");
        animator.SetTrigger(TackleHash);
    }

    public void PlayCelebrate()
    {
        if (!CanPlayFootballAnimation()) return;

        Debug.Log("FOOTBALL OS -> CELEBRATE");
        animator.SetTrigger(CelebrateHash);
    }

    private bool CanPlayFootballAnimation()
    {
        return animator != null && IsInFootballScene();
    }

    private bool IsInFootballScene()
    {
        return SceneManager.GetActiveScene().name == canchaSceneName;
    }
}