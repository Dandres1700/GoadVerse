using UnityEngine;
using UnityEngine.SceneManagement;

/// Controla los botones del menú principal.
public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre exacto de la escena del Lobby (debe estar en Build Settings)")]
    public string lobbySceneName = "Lobby";

    public void OnJugarPressed()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadLobby();
        }
        else
        {
            // Fallback por si el Bootstrap aún no cargó
            SceneManager.LoadScene(lobbySceneName);
        }
    }

    public void OnOpcionesPressed()
    {
        // Por ahora solo un placeholder, luego conectamos el panel de opciones
        Debug.Log("Abrir panel de opciones");
    }

    public void OnSalirPressed()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}