using System;
using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    [SerializeField]
    private bool esEquipoAzul;

    public static event Action<bool> GoalScored;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entró: " + other.name);

        if (!other.CompareTag("Ball"))
            return;

        Debug.Log("¡Gol!");

        GoalScored?.Invoke(esEquipoAzul);
    }
}