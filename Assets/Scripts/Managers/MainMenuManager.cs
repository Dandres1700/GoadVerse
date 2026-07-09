using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// Controla los botones del menu principal.
public class MainMenuManager : MonoBehaviour
{
    [Header("Configuracion")]
    [Tooltip("Nombre exacto de la escena del Lobby (debe estar en Build Settings)")]
    public string lobbySceneName = "Lobby";

    private GameObject settingsPanel;
    private GameObject historiaPanel;
    private RawImage historiaDisplay;
    private TMP_Text historiaStatusText;
    private RenderTexture historiaRenderTexture;
    private AudioSource historiaAudioSource;
    private VideoPlayer historiaVideoPlayer;
    private bool historiaSequenceRunning;

    public void OnJugarPressed()
    {
        if (historiaSequenceRunning)
        {
            return;
        }

        StartCoroutine(PlayHistoriaSequenceThenLobby());
    }

    private IEnumerator PlayHistoriaSequenceThenLobby()
    {
        historiaSequenceRunning = true;

        if (settingsPanel != null)
        {
            Destroy(settingsPanel);
            settingsPanel = null;
        }

        SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
        Canvas canvas = GetOrCreateRuntimeCanvas(settings);
        BuildHistoriaPanel(canvas, settings);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        string videoPath = FindHistoriaFile(settings.historiaFolderName, settings.instructionsVideoName, ".mp4", ".mov", ".webm", ".avi");
        yield return PlayInstructionsVideo(videoPath);

        string historiaVideoPath = FindHistoriaFile(settings.historiaFolderName, settings.historiaVideoName, ".mp4", ".mov", ".webm", ".avi");
        string audioPath = FindHistoriaFile(settings.historiaFolderName, settings.historiaAudioName, ".mp3", ".wav", ".ogg");
        yield return PlayHistoriaVideoWithAudio(historiaVideoPath, audioPath, settings.historiaFallbackSeconds);

        DestroyHistoriaPanel();
        historiaSequenceRunning = false;
        LoadLobby();
    }

    private void LoadLobby()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadLobby();
        }
        else
        {
            SceneManager.LoadScene(lobbySceneName);
        }
    }

    private void BuildHistoriaPanel(Canvas canvas, SceneUtilityUISettings settings)
    {
        DestroyHistoriaPanel();

        historiaPanel = new GameObject("HistoriaIntroPanel");
        historiaPanel.transform.SetParent(canvas.transform, false);
        historiaPanel.transform.SetAsLastSibling();

        RectTransform panelRect = historiaPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image background = historiaPanel.AddComponent<Image>();
        background.color = settings.historiaBackgroundColor;

        GameObject displayObject = new GameObject("HistoriaDisplay");
        displayObject.transform.SetParent(historiaPanel.transform, false);

        RectTransform displayRect = displayObject.AddComponent<RectTransform>();
        displayRect.anchorMin = Vector2.zero;
        displayRect.anchorMax = Vector2.one;
        displayRect.offsetMin = Vector2.zero;
        displayRect.offsetMax = Vector2.zero;

        historiaDisplay = displayObject.AddComponent<RawImage>();
        historiaDisplay.color = Color.white;

        AspectRatioFitter fitter = displayObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 16f / 9f;

        historiaStatusText = CreateText(historiaPanel.transform, "HistoriaStatus", new Vector2(0f, -430f), new Vector2(900f, 90f), 28f);
        historiaStatusText.text = "Cargando historia...";
        historiaStatusText.alignment = TextAlignmentOptions.Center;
    }

    private IEnumerator PlayInstructionsVideo(string videoPath)
    {
        if (!IsUsableFile(videoPath))
        {
            yield return ShowHistoriaWarningAndWait(
                "No se encontro un video de instrucciones valido. Entrando a la historia...",
                1.5f
            );
            yield break;
        }

        if (historiaStatusText != null)
        {
            historiaStatusText.text = "";
        }

        historiaRenderTexture = new RenderTexture(1920, 1080, 0);
        historiaRenderTexture.Create();

        historiaDisplay.texture = historiaRenderTexture;

        historiaVideoPlayer = historiaPanel.AddComponent<VideoPlayer>();
        historiaVideoPlayer.playOnAwake = false;
        historiaVideoPlayer.isLooping = false;
        historiaVideoPlayer.source = VideoSource.Url;
        historiaVideoPlayer.url = ToFileUrl(videoPath);
        historiaVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        historiaVideoPlayer.targetTexture = historiaRenderTexture;
        historiaVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        bool finished = false;
        bool failed = false;
        historiaVideoPlayer.loopPointReached += _ => finished = true;
        historiaVideoPlayer.errorReceived += (_, message) =>
        {
            failed = true;
            Debug.LogWarning("No se pudo reproducir Instrucciones.mp4: " + message);
        };

        historiaVideoPlayer.Prepare();

        float prepareTimer = 0f;
        while (!historiaVideoPlayer.isPrepared && !failed && prepareTimer < 10f)
        {
            prepareTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!historiaVideoPlayer.isPrepared || failed)
        {
            yield return ShowHistoriaWarningAndWait(
                "No se pudo preparar el video de instrucciones. Continuando...",
                1.5f
            );
            yield break;
        }

        historiaVideoPlayer.Play();

        float playbackTimer = 0f;
        float maxPlaybackSeconds = GetVideoTimeoutSeconds(historiaVideoPlayer, 20f);

        while (!finished && !failed && playbackTimer < maxPlaybackSeconds)
        {
            playbackTimer += Time.unscaledDeltaTime;

            if (!historiaVideoPlayer.isPlaying && historiaVideoPlayer.time > 0.1f)
            {
                break;
            }

            yield return null;
        }

        historiaVideoPlayer.Stop();
        Destroy(historiaVideoPlayer);
        historiaVideoPlayer = null;
    }

    private IEnumerator PlayHistoriaVideoWithAudio(string videoPath, string audioPath, float fallbackSeconds)
    {
        if (historiaStatusText != null)
        {
            historiaStatusText.text = "";
        }

        if (!IsUsableFile(videoPath))
        {
            yield return ShowHistoriaWarningAndWait(
                "No se encontro el video de historia. Entrando al lobby...",
                fallbackSeconds
            );
            yield break;
        }

        if (!IsUsableFile(audioPath))
        {
            yield return ShowHistoriaWarningAndWait(
                "No se encontro el audio de historia. Entrando al lobby...",
                fallbackSeconds
            );
            yield break;
        }

        historiaRenderTexture = RecreateRenderTexture(historiaRenderTexture);
        historiaDisplay.texture = historiaRenderTexture;

        historiaVideoPlayer = historiaPanel.AddComponent<VideoPlayer>();
        historiaVideoPlayer.playOnAwake = false;
        historiaVideoPlayer.isLooping = false;
        historiaVideoPlayer.source = VideoSource.Url;
        historiaVideoPlayer.url = ToFileUrl(videoPath);
        historiaVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        historiaVideoPlayer.targetTexture = historiaRenderTexture;
        historiaVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        StopOtherSceneAudio();

        historiaAudioSource = historiaPanel.AddComponent<AudioSource>();
        historiaAudioSource.playOnAwake = false;

        bool videoFinished = false;
        bool videoFailed = false;
        historiaVideoPlayer.loopPointReached += _ => videoFinished = true;
        historiaVideoPlayer.errorReceived += (_, message) =>
        {
            videoFailed = true;
            Debug.LogWarning("No se pudo reproducir Historia.mp4: " + message);
        };

        historiaVideoPlayer.Prepare();

        float prepareTimer = 0f;
        while (!historiaVideoPlayer.isPrepared && !videoFailed && prepareTimer < 10f)
        {
            prepareTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!historiaVideoPlayer.isPrepared || videoFailed)
        {
            yield return ShowHistoriaWarningAndWait(
                "No se pudo preparar el video de historia. Entrando al lobby...",
                fallbackSeconds
            );
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(ToFileUrl(audioPath), GetAudioType(audioPath)))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("No se pudo cargar Historia_narrada: " + request.error);
                yield return ShowHistoriaWarningAndWait(
                    "No se pudo cargar la narracion. Entrando al lobby...",
                    fallbackSeconds
                );
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

            if (clip == null)
            {
                yield return ShowHistoriaWarningAndWait(
                    "La narracion no esta disponible. Entrando al lobby...",
                    fallbackSeconds
                );
                yield break;
            }

            historiaAudioSource.clip = clip;
            historiaVideoPlayer.Play();
            historiaAudioSource.Play();

            float playbackTimer = 0f;
            float maxPlaybackSeconds = Mathf.Max(
                GetVideoTimeoutSeconds(historiaVideoPlayer, fallbackSeconds),
                clip.length + 5f
            );

            while (((!videoFinished && !videoFailed) || (historiaAudioSource != null && historiaAudioSource.isPlaying))
                && playbackTimer < maxPlaybackSeconds)
            {
                playbackTimer += Time.unscaledDeltaTime;

                bool videoStopped = !historiaVideoPlayer.isPlaying && historiaVideoPlayer.time > 0.1f;
                bool audioStopped = historiaAudioSource == null || !historiaAudioSource.isPlaying;

                if (videoStopped && audioStopped)
                {
                    break;
                }

                yield return null;
            }
        }

        if (historiaVideoPlayer != null)
        {
            historiaVideoPlayer.Stop();
            Destroy(historiaVideoPlayer);
            historiaVideoPlayer = null;
        }

        if (historiaAudioSource != null)
        {
            historiaAudioSource.Stop();
            Destroy(historiaAudioSource);
            historiaAudioSource = null;
        }
    }

    private void DestroyHistoriaPanel()
    {
        if (historiaVideoPlayer != null)
        {
            historiaVideoPlayer.Stop();
            Destroy(historiaVideoPlayer);
            historiaVideoPlayer = null;
        }

        if (historiaAudioSource != null)
        {
            historiaAudioSource.Stop();
            Destroy(historiaAudioSource);
            historiaAudioSource = null;
        }

        if (historiaRenderTexture != null)
        {
            historiaRenderTexture.Release();
            Destroy(historiaRenderTexture);
            historiaRenderTexture = null;
        }

        if (historiaPanel != null)
        {
            Destroy(historiaPanel);
            historiaPanel = null;
        }

        historiaDisplay = null;
        historiaStatusText = null;
    }

    private IEnumerator ShowHistoriaWarningAndWait(string message, float seconds)
    {
        Debug.LogWarning(message);

        if (historiaStatusText != null)
        {
            historiaStatusText.text = message;
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, seconds));
    }

    private void StopOtherSceneAudio()
    {
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource == null || audioSource == historiaAudioSource)
            {
                continue;
            }

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private static string FindHistoriaFile(string folderName, string fileNameWithoutExtension, params string[] extensions)
    {
        string streamingFolderPath = Path.Combine(Application.streamingAssetsPath, folderName);
        string filePath = FindHistoriaFileInFolder(streamingFolderPath, fileNameWithoutExtension, extensions);

        if (!string.IsNullOrEmpty(filePath))
        {
            return filePath;
        }

        string editorFolderPath = Path.Combine(Application.dataPath, folderName);
        return FindHistoriaFileInFolder(editorFolderPath, fileNameWithoutExtension, extensions);
    }

    private static string FindHistoriaFileInFolder(string folderPath, string fileNameWithoutExtension, string[] extensions)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            return string.Empty;
        }

        foreach (string filePath in Directory.GetFiles(folderPath))
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            if (!string.Equals(name, fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (string expectedExtension in extensions)
            {
                if (string.Equals(extension, expectedExtension, StringComparison.OrdinalIgnoreCase))
                {
                    return filePath;
                }
            }
        }

        return string.Empty;
    }

    private static bool IsUsableFile(string filePath)
    {
        return !string.IsNullOrWhiteSpace(filePath)
            && File.Exists(filePath)
            && new FileInfo(filePath).Length > 0;
    }

    private static string ToFileUrl(string filePath)
    {
        return new Uri(filePath).AbsoluteUri;
    }

    private static RenderTexture RecreateRenderTexture(RenderTexture current)
    {
        if (current != null)
        {
            current.Release();
            UnityEngine.Object.Destroy(current);
        }

        RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.Create();
        return renderTexture;
    }

    private static float GetVideoTimeoutSeconds(VideoPlayer videoPlayer, float fallbackSeconds)
    {
        if (videoPlayer == null)
        {
            return Mathf.Max(5f, fallbackSeconds);
        }

        if (videoPlayer.length > 0.1)
        {
            return (float)videoPlayer.length + 5f;
        }

        if (videoPlayer.frameCount > 0 && videoPlayer.frameRate > 0.1)
        {
            return (float)(videoPlayer.frameCount / videoPlayer.frameRate) + 5f;
        }

        return Mathf.Max(5f, fallbackSeconds);
    }

    private static AudioType GetAudioType(string audioPath)
    {
        string extension = Path.GetExtension(audioPath).ToLowerInvariant();

        if (extension == ".mp3") return AudioType.MPEG;
        if (extension == ".ogg") return AudioType.OGGVORBIS;
        if (extension == ".wav") return AudioType.WAV;

        return AudioType.UNKNOWN;
    }

    public void OnOpcionesPressed()
    {
        if (settingsPanel != null)
        {
            Destroy(settingsPanel);
            settingsPanel = null;
        }

        EnsureSettingsPanel();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
        }
    }

    public void OnSalirPressed()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void EnsureSettingsPanel()
    {
        if (settingsPanel != null)
        {
            return;
        }

        SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
        Canvas canvas = GetOrCreateRuntimeCanvas(settings);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);
        settingsPanel.transform.SetAsLastSibling();

        RectTransform rootRect = settingsPanel.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image rootImage = settingsPanel.AddComponent<Image>();
        rootImage.color = settings.overlayColor;

        GameObject panelBox = new GameObject("SettingsBox");
        panelBox.transform.SetParent(settingsPanel.transform, false);

        RectTransform boxRect = panelBox.AddComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.pivot = new Vector2(0.5f, 0.5f);
        boxRect.anchoredPosition = settings.mainMenuPanelPosition;
        boxRect.sizeDelta = settings.mainMenuPanelSize;

        Image boxImage = panelBox.AddComponent<Image>();
        boxImage.color = settings.panelColor;

        TMP_Text bodyText = CreateText(panelBox.transform, "SettingsText", settings.mainMenuTextPosition, settings.mainMenuTextSize, settings.panelTextFontSize);
        bodyText.text = settings.mainMenuSettingsText;
        bodyText.alignment = TextAlignmentOptions.Center;

        CreateButton(panelBox.transform, settings.closeButtonLabel, settings.mainMenuCloseButtonPosition, settings.mainMenuCloseButtonSize, () =>
        {
            settingsPanel.SetActive(false);
        });

        settingsPanel.SetActive(false);
    }

    private Canvas GetOrCreateRuntimeCanvas(SceneUtilityUISettings settings)
    {
        GameObject canvasObject = GameObject.Find("MainMenuRuntimeCanvas");

        if (canvasObject == null)
        {
            canvasObject = new GameObject("MainMenuRuntimeCanvas");
        }

        Canvas canvas = canvasObject.GetComponent<Canvas>();

        if (canvas == null)
        {
            canvas = canvasObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = settings.runtimeCanvasSortingOrder;

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

    private void CreateButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label + "Button");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        SceneUtilityUISettings settings = SceneUtilityUISettings.Get();
        Image image = buttonObject.AddComponent<Image>();
        image.color = settings.buttonColor;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        buttonObject.AddComponent<ButtonHoverEffect>();

        TMP_Text buttonText = CreateText(buttonObject.transform, "Text", Vector2.zero, size, settings.buttonTextFontSize);
        buttonText.text = label.ToUpper();
        buttonText.alignment = TextAlignmentOptions.Center;
    }
}
