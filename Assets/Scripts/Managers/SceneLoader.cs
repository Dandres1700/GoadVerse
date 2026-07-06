using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Configuración")]
    public string mainMenuSceneName = "Menu_Principal";
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

    void Start()
    {
        // Solo el Bootstrap debe disparar esto (ver Paso 4)
        StartCoroutine(LoadSceneAsync(mainMenuSceneName));
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

    public void LoadMainMenu()
    {
        StartCoroutine(LoadSceneAsync(mainMenuSceneName));
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