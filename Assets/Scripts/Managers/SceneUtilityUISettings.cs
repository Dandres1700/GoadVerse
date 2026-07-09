using UnityEngine;

[CreateAssetMenu(fileName = "SceneUtilityUISettings", menuName = "GoadVerse/Scene Utility UI Settings")]
public class SceneUtilityUISettings : ScriptableObject
{
    private const string ResourceName = "SceneUtilityUISettings";

    [Header("Canvas")]
    public int runtimeCanvasSortingOrder = 1000;

    [Header("Botones superiores")]
    public Vector2 topRightButtonSize = new Vector2(170f, 58f);
    public Vector2 minigamePauseButtonPosition = new Vector2(-210f, -24f);
    public Vector2 minigameSettingsButtonPosition = new Vector2(-24f, -24f);
    public Vector2 lobbyExitButtonPosition = new Vector2(-24f, -24f);

    [Header("Panel menu principal")]
    public Vector2 mainMenuPanelPosition = Vector2.zero;
    public Vector2 mainMenuPanelSize = new Vector2(760f, 660f);
    public Vector2 mainMenuTextPosition = new Vector2(0f, 65f);
    public Vector2 mainMenuTextSize = new Vector2(660f, 470f);
    public Vector2 mainMenuCloseButtonPosition = new Vector2(0f, -265f);
    public Vector2 mainMenuCloseButtonSize = new Vector2(300f, 76f);

    [Header("Panel minijuego")]
    public Vector2 minigamePanelPosition = Vector2.zero;
    public Vector2 minigamePanelSize = new Vector2(760f, 680f);
    public Vector2 minigameTextPosition = new Vector2(0f, 80f);
    public Vector2 minigameTextSize = new Vector2(660f, 470f);
    public Vector2 minigameExitLobbyButtonPosition = new Vector2(-175f, -270f);
    public Vector2 minigameExitLobbyButtonSize = new Vector2(320f, 72f);
    public Vector2 minigameCloseButtonPosition = new Vector2(175f, -270f);
    public Vector2 minigameCloseButtonSize = new Vector2(250f, 72f);

    [Header("Textos")]
    [TextArea(8, 18)]
    public string mainMenuSettingsText =
        "MENU DE CONFIGURACIONES\n\n" +
        "Movimiento del personaje\n" +
        "WASD / Flechas: mover\n" +
        "Shift: correr\n" +
        "Mouse: mover camara\n\n" +
        "Portal del lobby\n" +
        "Acercate al portal y presiona E para entrar.\n\n" +
        "Minijuego de futbol\n" +
        "WASD / Flechas: mover jugador\n" +
        "E / Tab: cambiar jugador\n" +
        "O: patear o pasar el balon\n" +
        "Boton PAUSA: detener o reanudar el juego";

    [TextArea(8, 18)]
    public string minigameControlsText =
        "CONTROLES\n\n" +
        "Lobby\n" +
        "WASD / Flechas: mover personaje\n" +
        "Shift: correr\n" +
        "Mouse: mover camara\n" +
        "Portal: acercate y presiona E\n\n" +
        "Minijuego\n" +
        "WASD / Flechas: mover jugador\n" +
        "E / Tab: cambiar jugador\n" +
        "O: patear o pasar el balon\n" +
        "PAUSA: detener o reanudar el partido";

    public string pauseButtonLabel = "PAUSA";
    public string resumeButtonLabel = "RESUMIR";
    public string settingsButtonLabel = "SETTINGS";
    public string closeButtonLabel = "CERRAR";
    public string lobbyExitButtonLabel = "SALIR";
    public string exitToLobbyButtonLabel = "SALIR AL LOBBY";

    [Header("Estilo")]
    public Color overlayColor = new Color32(0, 6, 14, 188);
    public Color panelColor = new Color32(6, 16, 28, 240);
    public Color buttonColor = new Color32(7, 42, 58, 245);
    public Color textColor = Color.white;
    public float panelTextFontSize = 30f;
    public float buttonTextFontSize = 26f;
    public float minFontSize = 16f;

    [Header("Historia")]
    public string historiaFolderName = "Historia";
    public string instructionsVideoName = "Instrucciones";
    public string historiaVideoName = "Historia";
    public string historiaAudioName = "Historia_narrada";
    public float historiaFallbackSeconds = 4f;
    public Color historiaBackgroundColor = Color.black;

    private static SceneUtilityUISettings cachedSettings;

    public static SceneUtilityUISettings Get()
    {
        if (cachedSettings != null)
        {
            return cachedSettings;
        }

        cachedSettings = Resources.Load<SceneUtilityUISettings>(ResourceName);

        if (cachedSettings == null)
        {
            cachedSettings = CreateInstance<SceneUtilityUISettings>();
            cachedSettings.name = "Runtime SceneUtilityUISettings";
        }

        return cachedSettings;
    }
}
