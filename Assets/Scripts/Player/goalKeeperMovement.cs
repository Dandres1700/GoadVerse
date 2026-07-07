using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GoalkeeperMovement : MonoBehaviour
{
    public float speed = 3f;
    public float rotationSpeed = 10f;
    public Animator animator;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 movimiento = new Vector3(x, 0, z).normalized;

        controller.Move(movimiento * speed * Time.deltaTime);

        // Si el movimiento es principalmente frontal, rotar al personaje
        if (movimiento.magnitude > 0.1f && Mathf.Abs(z) >= Mathf.Abs(x))
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(movimiento);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                rotationSpeed * Time.deltaTime
            );
        }

        // Animaciones
        animator.SetFloat("Speed", movimiento.magnitude);

        if (Mathf.Abs(x) > Mathf.Abs(z))
            animator.SetFloat("Direccion", 1); // Lateral
        else
            animator.SetFloat("Direccion", 0); // Frontal
    }
}

