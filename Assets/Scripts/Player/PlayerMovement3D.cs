using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement3D : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

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
    [SerializeField] private float maxFallSpeed = -35f;
    [SerializeField] private float maxRiseSpeed = 12f;

    [Header("Referencias")]
    public Transform cameraTransform;
    public Animator animator;

    [Header("Camara")]
    [SerializeField] private bool keepCameraFollowingPlayer = true;
    [SerializeField] private Vector3 cameraFollowOffset = new Vector3(0f, 3f, -6f);

    [Header("Rescate")]
    [SerializeField] private bool respawnIfOutOfBounds = true;
    [SerializeField] private float minRespawnY = -10f;
    [SerializeField] private float maxHeightFromSpawn = 15f;
    [SerializeField] private float maxDistanceFromSpawn = 80f;

    [Header("Animacion")]
    [SerializeField] private float animationDampTime = 0.1f;
    [SerializeField] private bool disableAnimatorRootMotion = true;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private RuntimeAnimatorController cachedAnimatorController;
    private bool hasSpeedParameter;
    private bool hasIsGroundedParameter;
    private bool hasVerticalSpeedParameter;
    private bool hasJumpParameter;
    private bool hasIsRunningParameter;
    private bool jumpStartedThisFrame;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null && disableAnimatorRootMotion)
        {
            animator.applyRootMotion = false;
        }

        ConfigureCameraFollow();
    }

    private void Update()
    {
        jumpStartedThisFrame = false;

        Vector2 input = GetMovementInput();
        bool isRunning = GetRunInput();

        MovePlayer(input, isRunning);
        ApplyGravityAndJump();
        RespawnIfNeeded();
        UpdateAnimations(input, isRunning);
    }

    private void MovePlayer(Vector2 input, bool isRunning)
    {
        Vector3 inputDirection = new Vector3(input.x, 0f, input.y).normalized;

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
            jumpStartedThisFrame = true;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        verticalVelocity.y = Mathf.Clamp(verticalVelocity.y, maxFallSpeed, maxRiseSpeed);

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void RespawnIfNeeded()
    {
        if (!respawnIfOutOfBounds) return;

        float horizontalDistanceFromSpawn = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(spawnPosition.x, spawnPosition.z)
        );

        bool insideVerticalBounds = transform.position.y > minRespawnY
            && transform.position.y < spawnPosition.y + maxHeightFromSpawn;
        bool insideHorizontalBounds = horizontalDistanceFromSpawn <= maxDistanceFromSpawn;

        if (insideVerticalBounds && insideHorizontalBounds) return;

        controller.enabled = false;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        verticalVelocity = Vector3.zero;
        controller.enabled = true;
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

    private void ConfigureCameraFollow()
    {
        if (!keepCameraFollowingPlayer || cameraTransform == null) return;

        ThirdPersonCamera thirdPersonCamera = cameraTransform.GetComponent<ThirdPersonCamera>();

        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = cameraTransform.gameObject.AddComponent<ThirdPersonCamera>();
        }

        thirdPersonCamera.target = transform;
        thirdPersonCamera.offset = cameraFollowOffset;
    }

    private void UpdateAnimations(Vector2 input, bool isRunning)
    {
        if (animator == null) return;

        if (animator.runtimeAnimatorController == null) return;

        CacheAnimatorParameters();

        float inputMagnitude = Mathf.Clamp01(input.magnitude);
        float speedValue = inputMagnitude < 0.1f ? 0f : (isRunning ? 1f : 0.5f);

        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, speedValue, animationDampTime, Time.deltaTime);
        }

        if (hasIsRunningParameter)
        {
            animator.SetBool(IsRunningHash, isRunning && inputMagnitude >= 0.1f);
        }

        if (hasIsGroundedParameter)
        {
            animator.SetBool(IsGroundedHash, controller.isGrounded);
        }

        if (hasVerticalSpeedParameter)
        {
            animator.SetFloat(VerticalSpeedHash, verticalVelocity.y);
        }

        if (jumpStartedThisFrame && hasJumpParameter)
        {
            animator.ResetTrigger(JumpHash);
            animator.SetTrigger(JumpHash);
        }
    }

    private void CacheAnimatorParameters()
    {
        if (cachedAnimatorController == animator.runtimeAnimatorController) return;

        cachedAnimatorController = animator.runtimeAnimatorController;
        hasSpeedParameter = false;
        hasIsGroundedParameter = false;
        hasVerticalSpeedParameter = false;
        hasJumpParameter = false;
        hasIsRunningParameter = false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == SpeedHash && parameter.type == AnimatorControllerParameterType.Float)
            {
                hasSpeedParameter = true;
            }
            else if (parameter.nameHash == IsGroundedHash && parameter.type == AnimatorControllerParameterType.Bool)
            {
                hasIsGroundedParameter = true;
            }
            else if (parameter.nameHash == VerticalSpeedHash && parameter.type == AnimatorControllerParameterType.Float)
            {
                hasVerticalSpeedParameter = true;
            }
            else if (parameter.nameHash == JumpHash && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                hasJumpParameter = true;
            }
            else if (parameter.nameHash == IsRunningHash && parameter.type == AnimatorControllerParameterType.Bool)
            {
                hasIsRunningParameter = true;
            }
        }
    }
}
