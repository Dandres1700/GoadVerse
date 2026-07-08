using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FootballOSAnimationDemo : MonoBehaviour
{
    [Header("Jugador que tiene las animaciones")]
    [SerializeField] private PlayerFootballAnimationController playerAnimations;

    [Header("Nombre exacto de la escena de la cancha")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Configuracion de prueba")]
    [SerializeField] private bool playDemoOnStart = true;
    [SerializeField] private bool loopDemo = false;

    private void Start()
    {
        if (!playDemoOnStart) return;

        if (SceneManager.GetActiveScene().name != canchaSceneName)
        {
            return;
        }

        StartCoroutine(DemoRoutine());
    }

    private IEnumerator DemoRoutine()
    {
        do
        {
            Debug.Log("FOOTBALL OS: Esperando primera jugada...");
            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Jugador recibe balón");
            playerAnimations.PlayReceive();
            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Ejecutando pase");
            playerAnimations.PlayPass();
            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Oportunidad de disparo detectada");
            playerAnimations.PlayShoot();
            yield return new WaitForSeconds(2f);

            Debug.Log("FOOTBALL OS: Celebración");
            playerAnimations.PlayCelebrate();
            yield return new WaitForSeconds(3f);

        } while (loopDemo);
    }
}