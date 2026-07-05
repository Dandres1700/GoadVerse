using UnityEngine;

/// Portal que envia al jugador desde el Lobby hacia un minijuego.
/// Requiere un Collider marcado como Trigger en el mismo GameObject.
[RequireComponent(typeof(Collider))]
public class MinigamePortal : MonoBehaviour
{
    [Header("Configuracion")]
    [Tooltip("Nombre exacto de la escena del minijuego (debe estar agregada en Build Settings).")]
    public string minigameSceneName;

    [Tooltip("Tag del objeto que puede activar el portal. Dejalo vacio para aceptar cualquier objeto.")]
    public string playerTag = "Player";

    [Tooltip("Evita cargar varias veces si el jugador toca el portal mas de una vez.")]
    public bool disableAfterUse = true;

    private bool isLoading;

    void Reset()
    {
        Collider portalCollider = GetComponent<Collider>();
        portalCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isLoading)
            return;

        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            return;

        if (string.IsNullOrEmpty(minigameSceneName))
        {
            Debug.LogWarning("MinigamePortal: asigna el nombre de la escena del minijuego.", this);
            return;
        }

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("MinigamePortal: no hay un SceneLoader en la escena.", this);
            return;
        }

        if (disableAfterUse)
            isLoading = true;

        SceneLoader.Instance.LoadMinigame(minigameSceneName);
    }
}
