using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Tooltip("Distancia horizontal para activar el portal aunque el trigger no dispare. Usa 0 para desactivarlo.")]
    public float activationRadius = 2.5f;

    [Header("Interaccion")]
    [Tooltip("Si esta activo, el jugador debe estar cerca y presionar la tecla de interaccion.")]
    public bool requireInteractionKey = true;

    [Tooltip("Tecla que activa el portal cuando el jugador esta cerca.")]
    public KeyCode interactionKey = KeyCode.E;

    [Tooltip("Evita cargar varias veces si el jugador toca el portal mas de una vez.")]
    public bool disableAfterUse = true;

    private bool isLoading;
    private Transform cachedPlayer;
    private bool playerInTrigger;

    void Reset()
    {
        Collider portalCollider = GetComponent<Collider>();
        portalCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isLoading)
            return;

        if (!IsAllowedActivator(other))
            return;

        cachedPlayer = other.transform;
        playerInTrigger = true;

        TryLoadTargetScene();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsAllowedActivator(other))
            return;

        if (cachedPlayer == other.transform)
        {
            cachedPlayer = null;
        }

        playerInTrigger = false;
    }

    void Update()
    {
        if (isLoading)
            return;

        if (!playerInTrigger && !IsPlayerInsideActivationRadius())
            return;

        TryLoadTargetScene();
    }

    private bool IsPlayerInsideActivationRadius()
    {
        if (activationRadius <= 0f)
            return false;

        Transform player = GetPlayerTransform();
        if (player == null)
            return false;

        Vector2 portalPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerPosition = new Vector2(player.position.x, player.position.z);

        return Vector2.Distance(portalPosition, playerPosition) <= activationRadius;
    }

    private Transform GetPlayerTransform()
    {
        if (cachedPlayer != null)
            return cachedPlayer;

        if (!string.IsNullOrEmpty(playerTag))
        {
            try
            {
                GameObject taggedPlayer = GameObject.FindGameObjectWithTag(playerTag);
                if (taggedPlayer != null)
                {
                    cachedPlayer = taggedPlayer.transform;
                    return cachedPlayer;
                }
            }
            catch (UnityException)
            {
                // Si el tag no existe, buscamos el controlador directo abajo.
            }
        }

        PlayerMovement3D playerMovement = FindAnyObjectByType<PlayerMovement3D>();
        if (playerMovement != null)
        {
            cachedPlayer = playerMovement.transform;
        }

        return cachedPlayer;
    }

    private bool IsAllowedActivator(Collider other)
    {
        if (string.IsNullOrEmpty(playerTag))
            return true;

        try
        {
            return other.CompareTag(playerTag);
        }
        catch (UnityException)
        {
            return other.GetComponent<PlayerMovement3D>() != null
                || other.GetComponentInParent<PlayerMovement3D>() != null;
        }
    }

    private void TryLoadTargetScene()
    {
        if (requireInteractionKey && !Input.GetKeyDown(interactionKey))
            return;

        LoadTargetScene();
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(minigameSceneName))
        {
            Debug.LogWarning("MinigamePortal: asigna el nombre de la escena del minijuego.", this);
            return;
        }

        if (disableAfterUse)
            isLoading = true;

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadMinigame(minigameSceneName);
        }
        else
        {
            SceneManager.LoadScene(minigameSceneName);
        }
    }
}
