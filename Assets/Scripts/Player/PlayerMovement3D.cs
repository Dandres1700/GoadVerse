using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement3D : MonoBehaviour
{
    public enum PlayerControlType
    {
        Player1_WASD,
        Player2_Arrows
    }

    [Header("Tipo de jugador")]
    public PlayerControlType controlType = PlayerControlType.Player1_WASD;

    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 12f;

    [Header("Salto y gravedad")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Referencias")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        MovePlayer();
        ApplyGravityAndJump();
        UpdateAnimations();
    }

    private void MovePlayer()
    {
        Vector2 input = GetMovementInput();

        Vector3 inputDirection = new Vector3(input.x, 0f, input.y).normalized;

        bool isRunning = GetRunInput();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle;

            if (cameraTransform != null)
            {
                targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            }
            else
            {
                targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            }

            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
        }
    }

    private void ApplyGravityAndJump()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        if (GetJumpInput() && isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private Vector2 GetMovementInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (controlType == PlayerControlType.Player1_WASD)
        {
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;
            if (Input.GetKey(KeyCode.W)) vertical = 1f;
            if (Input.GetKey(KeyCode.S)) vertical = -1f;
        }
        else if (controlType == PlayerControlType.Player2_Arrows)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
            if (Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) vertical = -1f;
        }

        return new Vector2(horizontal, vertical);
    }

    private bool GetRunInput()
    {
        if (controlType == PlayerControlType.Player1_WASD)
        {
            return Input.GetKey(KeyCode.LeftShift);
        }

        return Input.GetKey(KeyCode.RightShift);
    }

    private bool GetJumpInput()
    {
        if (controlType == PlayerControlType.Player1_WASD)
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        return Input.GetKeyDown(KeyCode.Return);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        if (animator.runtimeAnimatorController == null) return;

        Vector2 input = GetMovementInput();
        float speedValue = input.magnitude;

        animator.SetFloat("Speed", speedValue);
        animator.SetBool("IsGrounded", controller.isGrounded);
    }
}