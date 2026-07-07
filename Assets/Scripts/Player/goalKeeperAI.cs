using UnityEngine;

[RequireComponent(typeof(GoalkeeperMovement))]
public class GoalkeeperAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform ball;

    [Header("Alineación")]
    [SerializeField] private float tolerancia = 0.2f;

    [Header("Salida del portero")]
    [SerializeField] private float distanciaSalida = 30f;

    private GoalkeeperMovement movement;
    private GoalkeeperPlayerInput playerInput;

    private void Awake()
    {
        movement = GetComponent<GoalkeeperMovement>();
        playerInput = GetComponent<GoalkeeperPlayerInput>();
    }

    private void Update()
    {
        // Si el jugador controla al portero, la IA no actúa.
        if (playerInput != null && playerInput.controladoPorElJugador)
            return;

        if (ball == null)
        {
            movement.MoveDirection = Vector3.zero;
            movement.AnimationDirection = 0f;
            return;
        }

        float diferenciaX = Mathf.Abs(ball.position.x - transform.position.x);

        // ============================================================
        // BALÓN CERCA -> SALIR A INTERCEPTAR
        // ============================================================
        if (diferenciaX <= distanciaSalida)
        {
            Vector3 direccion = ball.position - transform.position;
            direccion.y = 0f;

            movement.MoveDirection = direccion.normalized;

            // Elegir la animación correcta
            if (Mathf.Abs(direccion.z) > Mathf.Abs(direccion.x))
                movement.AnimationDirection = 1f; // Lateral
            else
                movement.AnimationDirection = 0f; // Frontal
        }
        // ============================================================
        // BALÓN LEJOS -> SOLO ALINEARSE EN Z
        // ============================================================
        else
        {
            float diferenciaZ = ball.position.z - transform.position.z;

            if (Mathf.Abs(diferenciaZ) <= tolerancia)
            {
                movement.MoveDirection = Vector3.zero;
            }
            else
            {
                movement.MoveDirection = new Vector3(
                    0f,
                    0f,
                    Mathf.Sign(diferenciaZ)
                );
            }

            // Siempre es un movimiento lateral
            movement.AnimationDirection = 1f;
        }
    }
}