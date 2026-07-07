using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GoalkeeperMovement : MonoBehaviour
{
    public float speed = 3f;
    public float rotationSpeed = 10f;
    public Animator animator;

    private CharacterController controller;

    public Vector3 MoveDirection { get; set; }

    // 0 = Frontal, 1 = Lateral
    public float AnimationDirection { get; set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 movimiento = MoveDirection.normalized;

        controller.Move(movimiento * speed * Time.deltaTime);

        if (movimiento.magnitude > 0.1f &&
            Mathf.Abs(movimiento.x) >= Mathf.Abs(movimiento.z))
        {
            Quaternion objetivo = Quaternion.LookRotation(movimiento);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                objetivo,
                rotationSpeed * Time.deltaTime
            );
        }

        animator.SetFloat("Speed", movimiento.magnitude);
        animator.SetFloat("Direccion", AnimationDirection);
    }

}

