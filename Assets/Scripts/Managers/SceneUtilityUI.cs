using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneUtilityUI : MonoBehaviour
{
    private const string LobbySceneName = "Lobby";
    private const string MinigameSceneName = "minijuego1";

    private static SceneUtilityUI instance;

    private GameObject sceneRoot;
    private GameObject settingsPanel;
    private TMP_Text pauseButtonText;
    private bool isPaused;
    private bool settingsPausedGame;
    private float timeScaleBeforePause = 1f;
    private CursorLockMode cursorLockBeforePanel = CursorLockMode.None;
    private bool cursorVisibleBeforePanel = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;

        instance.BuildForScene(SceneManager.GetActiveScene());
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        instance = FindAnyObjectByType<SceneUtilityUI>();

        if (instance != null)
        {
            DontDestroyOnLoad(instance.gameObject);
            return;
        }

        GameObject utilityObject = new GameObject("SceneUtilityUI");
        instance = utilityObject.AddComponent<SceneUtilityUI>();
        DontDestroyOnLoad(utilityObject);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInstance();
        instance.BuildForScene(scene);
    }

    private void BuildForScene(Scene scene)
    {
        ClearSceneUI();
        Time.timeScale = 1f;
        isPaused = false;
        settingsPausedGame = false;
        timeScaleBeforePause = 1f;

        if (scene.name == MinigameSceneName)
        {
            BuildMinigameUI();
        }
        else if (scene.name == LobbySceneName)
        {
            BuildLobbyUI();
        }
    }

    private void ClearSceneUI()
    {
        if (sceneRoot != null)
        {
            Destroy(sceneRoot);
            sceneRoot = null;
        }

        settingsPanel = null;
        pauseButtonText = null;
    }

    private void BuildMinigameUI()
    {
        SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
        Canvas canvas = GetOrCreateCanvas("MinigameRuntimeCanvas", settings.runtimeCanvasSortingOrder);

        sceneRoot = new GameObject("MinigameRuntimeUI");
        sceneRoot.transform.SetParent(canvas.transform, false);
        sceneRoot.transform.SetAsLastSibling();
        StretchToParent(sceneRoot);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CreateTopRightButton(sceneRoot.transform, settings.pauseButtonLabel, settings.minigamePauseButtonPosition, () =>
        {
            SetPaused(!isPaused);
        }, out pauseButtonText);

        CreateTopRightButton(sceneRoot.transform, settings.settingsButtonLabel, settings.minigameSettingsButtonPosition, () =>
        {
            ShowSettingsPanel();
        }, out _);

        BuildSettingsPanel(sceneRoot.transform, true);
    }

    private void BuildLobbyUI()
    {
        SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
        Canvas canvas = GetOrCreateCanvas("LobbyRuntimeCanvas", settings.runtimeCanvasSortingOrder);

        sceneRoot = new GameObject("LobbyRuntimeUI");
        sceneRoot.transform.SetParent(canvas.transform, false);
        sceneRoot.transform.SetAsLastSibling();
        StretchToParent(sceneRoot);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CreateTopRightButton(sceneRoot.transform, settings.lobbyExitButtonLabel, settings.lobbyExitButtonPosition, QuitGame, out _);
    }

    private Canvas GetOrCreateCanvas(string fallbackName, int sortingOrder)
    {
        GameObject canvasObject = GameObject.Find(fallbackName);

        if (canvasObject == null)
        {
            canvasObject = new GameObject(fallbackName);
        }

        Canvas canvas = canvasObject.GetComponent<Canvas>();

        if (canvas == null)
        {
            canvas = canvasObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();

        if (scaler == null)
        {
            scaler = canvasObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        return canvas;
    }

    private void StretchToParent(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();

        if (rect == null)
        {
            rect = obj.AddComponent<RectTransform>();
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void BuildSettingsPanel(Transform canvasRoot, bool includeLobbyExit)
    {
        SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
        settingsPanel = new GameObject("MinigameSettingsPanel");
        settingsPanel.transform.SetParent(canvasRoot, false);

        RectTransform rootRect = settingsPanel.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image rootImage = settingsPanel.AddComponent<Image>();
        rootImage.color = settings.overlayColor;

        GameObject box = new GameObject("SettingsBox");
        box.transform.SetParent(settingsPanel.transform, false);

        RectTransform boxRect = box.AddComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.anchoredPosition = settings.minigamePanelPosition;
        boxRect.sizeDelta = settings.minigamePanelSize;

        Image boxImage = box.AddComponent<Image>();
        boxImage.color = settings.panelColor;

        TMP_Text body = CreateText(box.transform, "ControlsText", settings.minigameTextPosition, settings.minigameTextSize, settings.panelTextFontSize);
        body.text = settings.minigameControlsText;
        body.alignment = TextAlignmentOptions.Center;

        if (includeLobbyExit)
        {
            CreateCenteredButton(box.transform, settings.exitToLobbyButtonLabel, settings.minigameExitLobbyButtonPosition, settings.minigameExitLobbyButtonSize, ReturnToLobby, out _);
            CreateCenteredButton(box.transform, settings.closeButtonLabel, settings.minigameCloseButtonPosition, settings.minigameCloseButtonSize, HideSettingsPanel, out _);
        }
        else
        {
            CreateCenteredButton(box.transform, settings.closeButtonLabel, settings.minigameCloseButtonPosition, settings.minigameCloseButtonSize, HideSettingsPanel, out _);
        }

        settingsPanel.SetActive(false);
    }

    private void ShowSettingsPanel()
    {
        if (settingsPanel == null)
        {
            return;
        }

        UnlockCursorForPanel();

        settingsPausedGame = !isPaused && SceneManager.GetActiveScene().name == MinigameSceneName;

        if (settingsPausedGame)
        {
            SetPaused(true);
        }

        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
    }

    private void HideSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (settingsPausedGame)
        {
            settingsPausedGame = false;
            SetPaused(false);
        }

        RestoreCursorAfterPanel();
    }

    private void SetPaused(bool paused)
    {
        if (paused)
        {
            if (!Mathf.Approximately(Time.timeScale, 0f))
            {
                timeScaleBeforePause = Time.timeScale;
            }

            Time.timeScale = 0f;
            isPaused = true;
            UnlockCursorForPanel();
        }
        else
        {
            Time.timeScale = timeScaleBeforePause > 0f ? timeScaleBeforePause : 1f;
            isPaused = false;

            if (settingsPanel == null || !settingsPanel.activeSelf)
            {
                RestoreCursorAfterPanel();
            }
        }

        if (pauseButtonText != null)
        {
            SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
            pauseButtonText.text = isPaused ? settings.resumeButtonLabel : settings.pauseButtonLabel;
        }
    }

    private void ReturnToLobby()
    {
        Time.timeScale = 1f;
        isPaused = false;
        settingsPausedGame = false;
        RestoreCursorAfterPanel();

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadLobby();
        }
        else
        {
            SceneManager.LoadScene(LobbySceneName);
        }
    }

    private void UnlockCursorForPanel()
    {
        if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
        {
            cursorLockBeforePanel = Cursor.lockState;
            cursorVisibleBeforePanel = Cursor.visible;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursorAfterPanel()
    {
        if (SceneManager.GetActiveScene().name != MinigameSceneName)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = cursorLockBeforePanel;
        Cursor.visible = cursorVisibleBeforePanel;
    }

    private void QuitGame()
    {
        Debug.Log("Saliendo del juego desde Lobby...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void CreateTopRightButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action, out TMP_Text buttonText)
    {
        GameObject buttonObject = CreateButtonObject(parent, label, action, out buttonText);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = SceneUtilityUISettings.Get().topRightButtonSize;
    }

    private void CreateCenteredButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action, out TMP_Text buttonText)
    {
        GameObject buttonObject = CreateButtonObject(parent, label, action, out buttonText);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private GameObject CreateButtonObject(Transform parent, string label, UnityEngine.Events.UnityAction action, out TMP_Text buttonText)
    {
        GameObject buttonObject = new GameObject(label + "Button");
        buttonObject.transform.SetParent(parent, false);

        buttonObject.AddComponent<RectTransform>();

        SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
        Image image = buttonObject.AddComponent<Image>();
        image.color = settings.buttonColor;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        buttonObject.AddComponent<ButtonHoverEffect>();

        buttonText = CreateText(buttonObject.transform, "Text", Vector2.zero, Vector2.zero, settings.buttonTextFontSize);
        buttonText.text = label;
        buttonText.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return buttonObject;
    }

    private TMP_Text CreateText(Transform parent, string objectName, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = settings.minFontSize;
        text.fontSizeMax = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.color = settings.textColor;
        text.raycastTarget = false;

        return text;
    }
}
