using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// Encargado de cargar el Lobby o los minijuegos.
/// Es un Singleton que persiste entre escenas.
/// Debe existir UNA sola vez, idealmente junto al GameManager en la escena "Bootstrap".
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Configuracion")]
    [Tooltip("Nombre exacto de la escena del Lobby (debe estar agregada en Build Settings)")]
    public string lobbySceneName = "Lobby";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMinigame(string sceneName)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.currentMinigameId = sceneName;

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void LoadLobby()
    {
        StartCoroutine(LoadSceneAsync(lobbySceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (operation != null && !operation.isDone)
        {
            yield return null;
        }
    }
}