using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GoalkeeperMovement : MonoBehaviour
{
    public float speed = 3f;
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

        if (movimiento != Vector3.zero)
        {
            transform.forward = movimiento;
        }


        // Animaciones
        float velocidad = movimiento.magnitude;

        animator.SetFloat("Speed", velocidad);


        // Detectar tipo de movimiento
        if (Mathf.Abs(x) > Mathf.Abs(z))
        {
            // Movimiento lateral
            animator.SetFloat("Direccion", 1);
        }
        else
        {
            // Movimiento frontal
            animator.SetFloat("Direccion", 0);
        }
    }
}

