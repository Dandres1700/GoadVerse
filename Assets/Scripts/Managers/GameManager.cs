using UnityEngine;
using System.Collections.Generic;

/// Guarda el estado global del juego (puntajes, minijuego actual, etc).
/// Es un Singleton que persiste entre escenas.
/// Debe existir UNA sola vez, idealmente en una escena "Bootstrap" que carga primero.

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estado del juego")]
    public string currentMinigameId;

    private Dictionary<string, int> scores = new Dictionary<string, int>();

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

    public void RegisterScore(string minigameId, int score)
    {
        scores[minigameId] = score;
        Debug.Log($"Puntaje registrado - {minigameId}: {score}");
    }

    public int GetScore(string minigameId)
    {
        return scores.TryGetValue(minigameId, out int score) ? score : 0;
    }
}