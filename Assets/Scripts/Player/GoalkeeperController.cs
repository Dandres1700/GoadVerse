using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GoalkeeperController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int ReceiveHash = Animator.StringToHash("Receive");

    [Header("Escena")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Balon")]
    [SerializeField] private string ballObjectName = "Ball";
    [SerializeField] private float catchRadius = 1.45f;
    [SerializeField] private float catchVerticalLimit = 2.2f;
    [SerializeField] private float catchCooldown = 0.8f;
    [SerializeField] private float holdSeconds = 0.75f;
    [SerializeField] private float releaseForce = 0.5f;
    [SerializeField] private float releaseLift = 0.2f;

    [Header("Manos")]
    [SerializeField] private float catchHeight = 1.25f;
    [SerializeField] private float catchForwardOffset = 0.45f;
    [SerializeField] private float handSpacing = 0.24f;
    [SerializeField] private bool useHandIk = true;

    [Header("Movimiento")]
    [SerializeField] private bool useCurrentPositionAsHome = true;
    [SerializeField] private Vector3 homePosition;
    [SerializeField] private Vector3 lateralAxis = Vector3.forward;
    [SerializeField] private float patrolDistance = 4f;
    [SerializeField] private float moveSpeed = 2.4f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private Vector3 fieldCenter = Vector3.zero;

    private Animator animator;
    private Transform ball;
    private Rigidbody ballRigidbody;
    private bool hasSpeedParameter;
    private bool hasIsGroundedParameter;
    private bool hasIsRunningParameter;
    private bool hasReceiveParameter;
    private bool isHoldingBall;
    private float holdTimer;
    private float cooldownTimer;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        CacheAnimatorParameters();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != canchaSceneName)
        {
            enabled = false;
            return;
        }

        if (useCurrentPositionAsHome)
        {
            homePosition = transform.position;
        }

        FindBall();
        SetGroundedAnimation();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (ball == null)
        {
            FindBall();
        }

        if (isHoldingBall)
        {
            HoldBall();
            return;
        }

        MoveOnGoalLine();
        LookAtBallOrField();
        TryCatchBall();
    }

    private void MoveOnGoalLine()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = GetGoalLineTarget();

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        float movedDistance = Vector3.Distance(startPosition, transform.position);
        SetLocomotionAnimation(movedDistance > 0.001f ? Mathf.Clamp01(movedDistance / Mathf.Max(Time.deltaTime, 0.0001f)) : 0f);
    }

    private Vector3 GetGoalLineTarget()
    {
        Vector3 axis = GetFlatLateralAxis();
        Vector3 target = homePosition;

        if (ball != null)
        {
            float offset = Vector3.Dot(ball.position - homePosition, axis);
            target += axis * Mathf.Clamp(offset, -patrolDistance, patrolDistance);
        }

        target.y = homePosition.y;
        return target;
    }

    private Vector3 GetFlatLateralAxis()
    {
        Vector3 axis = lateralAxis;
        axis.y = 0f;

        if (axis.sqrMagnitude < 0.001f)
        {
            axis = Vector3.forward;
        }

        return axis.normalized;
    }

    private void LookAtBallOrField()
    {
        Vector3 target = ball != null ? ball.position : fieldCenter;
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void TryCatchBall()
    {
        if (ball == null || cooldownTimer > 0f) return;

        Vector3 keeperFlat = transform.position;
        Vector3 ballFlat = ball.position;
        keeperFlat.y = 0f;
        ballFlat.y = 0f;

        float horizontalDistance = Vector3.Distance(keeperFlat, ballFlat);
        float verticalDistance = ball.position.y - transform.position.y;

        if (horizontalDistance > catchRadius) return;
        if (verticalDistance < -0.25f || verticalDistance > catchVerticalLimit) return;

        StartCatch();
    }

    private void StartCatch()
    {
        isHoldingBall = true;
        holdTimer = holdSeconds;
        cooldownTimer = catchCooldown;
        SetLocomotionAnimation(0f);

        if (hasReceiveParameter)
        {
            animator.SetTrigger(ReceiveHash);
        }

        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.useGravity = false;
            ballRigidbody.isKinematic = true;
        }

        SnapBallToHands();
    }

    private void HoldBall()
    {
        SetLocomotionAnimation(0f);
        SnapBallToHands();
        LookAtBallOrField();

        holdTimer -= Time.deltaTime;

        if (holdTimer <= 0f)
        {
            ReleaseBall();
        }
    }

    private void SnapBallToHands()
    {
        if (ball == null) return;

        ball.position = GetBallHoldPosition();
        ball.rotation = Quaternion.identity;
    }

    private Vector3 GetBallHoldPosition()
    {
        return transform.position
            + transform.forward * catchForwardOffset
            + Vector3.up * catchHeight;
    }

    private void ReleaseBall()
    {
        isHoldingBall = false;

        if (ballRigidbody == null) return;

        Vector3 releaseDirection = fieldCenter - transform.position;
        releaseDirection.y = 0f;

        if (releaseDirection.sqrMagnitude < 0.001f)
        {
            releaseDirection = transform.forward;
        }

        releaseDirection = (releaseDirection.normalized + Vector3.up * releaseLift).normalized;

        ballRigidbody.isKinematic = false;
        ballRigidbody.useGravity = true;
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.AddForce(releaseDirection * releaseForce, ForceMode.Impulse);
    }

    private void FindBall()
    {
        GameObject ballObject = GameObject.Find(ballObjectName);

        if (ballObject == null) return;

        ball = ballObject.transform;
        ballRigidbody = ballObject.GetComponent<Rigidbody>();
    }

    private void CacheAnimatorParameters()
    {
        if (animator == null) return;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == SpeedHash)
            {
                hasSpeedParameter = true;
            }
            else if (parameter.nameHash == IsGroundedHash)
            {
                hasIsGroundedParameter = true;
            }
            else if (parameter.nameHash == IsRunningHash)
            {
                hasIsRunningParameter = true;
            }
            else if (parameter.nameHash == ReceiveHash)
            {
                hasReceiveParameter = true;
            }
        }
    }

    private void SetGroundedAnimation()
    {
        if (animator == null) return;

        if (hasIsGroundedParameter)
        {
            animator.SetBool(IsGroundedHash, true);
        }

        if (hasIsRunningParameter)
        {
            animator.SetBool(IsRunningHash, false);
        }
    }

    private void SetLocomotionAnimation(float speed)
    {
        if (animator == null) return;

        SetGroundedAnimation();

        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, speed);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!useHandIk || animator == null || !animator.isHuman) return;
        if (ball == null || !isHoldingBall) return;

        Vector3 center = GetBallHoldPosition();

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, center - transform.right * handSpacing);
        animator.SetIKPosition(AvatarIKGoal.RightHand, center + transform.right * handSpacing);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 basePosition = useCurrentPositionAsHome ? transform.position : homePosition;
        Vector3 axis = GetFlatLateralAxis();

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(basePosition - axis * patrolDistance, basePosition + axis * patrolDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(basePosition + transform.forward * catchForwardOffset + Vector3.up * catchHeight, catchRadius);
    }

    private void OnValidate()
    {
        catchRadius = Mathf.Max(0.1f, catchRadius);
        catchVerticalLimit = Mathf.Max(0.1f, catchVerticalLimit);
        catchCooldown = Mathf.Max(0f, catchCooldown);
        holdSeconds = Mathf.Max(0.05f, holdSeconds);
        releaseForce = Mathf.Max(0f, releaseForce);
        catchHeight = Mathf.Max(0f, catchHeight);
        patrolDistance = Mathf.Max(0f, patrolDistance);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        turnSpeed = Mathf.Max(0f, turnSpeed);
    }
}
