using UnityEngine;

[RequireComponent(typeof(GoalkeeperMovement))]
public class GoalkeeperPlayerInput : MonoBehaviour
{
    [Header("Control")]
    public bool controladoPorElJugador = false;

    private GoalkeeperMovement movement;

    private void Awake()
    {
        movement = GetComponent<GoalkeeperMovement>();
    }

    private void Update()
    {
        // Si la IA tiene el control, no leer el teclado
        if (!controladoPorElJugador)
            return;

        float z = Input.GetAxisRaw("Horizontal");
        float x = Input.GetAxisRaw("Vertical");

        movement.MoveDirection = new Vector3(x, 0f, z);
    }
}