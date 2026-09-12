using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 6f;
    public float jumpForce = 4.8f;
    public float rotationSpeed = 18f;

    [Header("Air Movement")]
    public float airControl = 0.65f;
    public float airRotationSpeed = 8f;
    public bool keepAirMomentumWhenNoInput = true;

    [Header("Soul Movement While Dead")]
    public float soulMoveSpeed = 6f;

    [Header("Jump Feel")]
    public float jumpGravityScale = 3.5f;
    public float fallGravityScale = 7f;
    public float maxFallSpeed = 25f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.5f;
    public LayerMask groundLayer;

    [Header("Jump Protection")]
    public float jumpLockTime = 0.12f;

    private bool isAttacking = false;
    private bool hasJumped = false;
    private float jumpLockTimer = 0f;

    private Rigidbody rb;
    private Animator animator;
    private Transform cameraTransform;
    private bool isGrounded;
    private bool wasGrounded;
    private float defaultGravity;
    private PlayerHealth playerHealth;

    private float inputH;
    private float inputV;
    private bool hasInput;
    private bool jumpPressed;
    private bool isRunning;
    private float animatorSpeedValue = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        playerHealth = GetComponent<PlayerHealth>();

        if (Camera.main != null)
            cameraTransform = Camera.main.transform;

        defaultGravity = Physics.gravity.y;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (animator != null)
            animator.applyRootMotion = false;
    }

    void Update()
    {
        if (jumpLockTimer > 0f)
            jumpLockTimer -= Time.deltaTime;

        CheckGround();

        if (IsTypingInUI())
        {
            inputH = 0f;
            inputV = 0f;
            hasInput = false;
            isRunning = false;
            StopMove();
            animatorSpeedValue = 0f;
            UpdateAnimator();
            return;
        }

        inputH = Input.GetAxisRaw("Horizontal");
        inputV = Input.GetAxisRaw("Vertical");

        hasInput = Mathf.Abs(inputH) > 0.01f || Mathf.Abs(inputV) > 0.01f;
        isRunning = hasInput && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetMouseButton(1));

        if (!hasInput)
            animatorSpeedValue = 0f;
        else
            animatorSpeedValue = isRunning ? runSpeed : walkSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        UpdateAnimator();
        HandleLanding();
    }

    void FixedUpdate()
    {
        Move();
        Jump();
        ApplyBetterGravity();
    }

    bool IsTypingInUI()
    {
        if (EventSystem.current == null)
            return false;

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
            return false;

        return selected.GetComponent<TMP_InputField>() != null ||
               selected.GetComponentInParent<TMP_InputField>() != null;
    }

    void StopMove()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        rb.angularVelocity = Vector3.zero;
    }

    void Move()
    {
        if (cameraTransform == null)
        {
            StopMove();
            animatorSpeedValue = 0f;
            return;
        }

        bool isDead = playerHealth != null && playerHealth.IsDead();

        if (!isDead && isAttacking && isGrounded)
        {
            StopMove();
            animatorSpeedValue = 0f;
            return;
        }

        if (!hasInput)
        {
            if (isGrounded || !keepAirMomentumWhenNoInput)
                StopMove();

            animatorSpeedValue = 0f;
            return;
        }

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * inputV + right * inputH).normalized;

        float targetSpeed = isDead ? soulMoveSpeed : (isRunning ? runSpeed : walkSpeed);

        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(
                moveDir.x * targetSpeed,
                rb.linearVelocity.y,
                moveDir.z * targetSpeed
            );

            RotateToMoveDirection(moveDir, rotationSpeed);
        }
        else
        {
            Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 targetHorizontalVelocity = moveDir * targetSpeed;

            Vector3 newHorizontalVelocity = Vector3.Lerp(
                currentHorizontalVelocity,
                targetHorizontalVelocity,
                airControl * Time.fixedDeltaTime * 10f
            );

            rb.linearVelocity = new Vector3(
                newHorizontalVelocity.x,
                rb.linearVelocity.y,
                newHorizontalVelocity.z
            );

            RotateToMoveDirection(moveDir, airRotationSpeed);
        }
    }

    void RotateToMoveDirection(Vector3 moveDir, float turnSpeed)
    {
        if (moveDir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }

    void Jump()
    {
        if (!jumpPressed)
            return;

        jumpPressed = false;

        if (playerHealth != null && playerHealth.IsDead())
            return;

        if (isGrounded && !hasJumped && jumpLockTimer <= 0f && !isAttacking)
        {
            hasJumped = true;
            isGrounded = false;
            jumpLockTimer = jumpLockTime;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (animator != null)
            {
                animator.ResetTrigger("Jump");
                animator.SetTrigger("Jump");
                animator.SetBool("IsGrounded", false);
            }
        }
    }

    void ApplyBetterGravity()
    {
        if (!isGrounded)
        {
            float gravityMultiplier = rb.linearVelocity.y > 0f ? jumpGravityScale : fallGravityScale;

            rb.linearVelocity += Vector3.up *
                defaultGravity *
                (gravityMultiplier - 1f) *
                Time.fixedDeltaTime;

            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -maxFallSpeed, rb.linearVelocity.z);
            }
        }
    }

    void HandleLanding()
    {
        if (!wasGrounded && isGrounded && jumpLockTimer <= 0f)
        {
            hasJumped = false;

            if (animator != null)
                animator.ResetTrigger("Jump");
        }

        wasGrounded = isGrounded;
    }

    void CheckGround()
    {
        if (groundCheck == null)
            return;

        bool touchingGround = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        isGrounded = jumpLockTimer <= 0f && touchingGround;

        if (animator != null)
            animator.SetBool("IsGrounded", isGrounded);
    }

    void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.applyRootMotion = false;
        animator.SetFloat("Speed", animatorSpeedValue);
        animator.SetBool("IsGrounded", isGrounded);
    }
}