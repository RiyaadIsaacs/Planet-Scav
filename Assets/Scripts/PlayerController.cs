using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

// Player controller for movement, camera control and other features to be decided later
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject playerRef;
    [SerializeField] private Animator animator;
    [SerializeField] public DeathHandling deathHandling;

    [Header("Animation Params (Animator)")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string isRunningParam = "IsRunning";
    [SerializeField] private string isJumpingParam = "IsJumping";
    [SerializeField] private string isGroundedParam = "IsGrounded";

    [Header("Animation (Jump)")]
    [SerializeField] private bool forceJumpAnimationOnPress = true;
    [SerializeField] private string jumpStateName = "Jump";
    [SerializeField] private float jumpCrossFadeTime = 0.05f;

    // Player WASD movement and jump settings
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float groundAcceleration = 24f;
    [SerializeField] private float groundDeceleration = 28f;
    [SerializeField] private float jumpForce = 10f; // Jump force for normal jumps
    [SerializeField] private float gravity = -18f;
    [SerializeField] private float groundCheckDistance = 1.1f; // Allowable distance from ground to player
    [SerializeField] private LayerMask groundLayer = -1;

    // Player camera control with mouse
    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    // Player pump-action jump settings
    [Header("Charge Jump")]
    [SerializeField] private float chargePerPress = 5f;   // How much charge each space bar press adds
    [SerializeField] private float maxCharge = 25f;
    [SerializeField] private float chargeMultiplier = 1.2f; // Converts charge into jump force

    [SerializeField] [Range(0f, 1f)] private float airControl = 0f; // 0 = keep jump momentum only, 1 = full movement control

    private bool ctrlHeld;
    private float charge;
    private Vector3 horizontalVelocity; // units per second

    // pickup-applied temporary multiplier (consumed per charged jump)
    private float pickupChargeMultiplier = 0f;
    private int pickupMultiplierUses = 0;

    // Input values to record from the new input system
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintHeld;
    private float verticalVelocity; // value to control vertical movement 
    private float pitch;

    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;

    private IPlayerState currentState;
    private IdleState idleState;
    private WalkState walkState;
    private RunState runState;

    private int speedHash;
    private int isRunningHash;
    private int isJumpingHash;
    private int isGroundedHash;

    private bool isJumping;

    [Header("Fall Death")]
    [SerializeField] private bool killAfterLongFall = true;
    [SerializeField] private float secondsFallingToDie = 6f;
    [SerializeField] private float fallingVelocityThreshold = -0.1f;
    private float fallingTimer;
    private bool longFallDeathTriggered;

    public float MoveMagnitude => moveInput.magnitude;
    public bool SprintHeld => sprintHeld;
    public float VerticalVelocity => verticalVelocity;
    public float HorizontalSpeed => new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z).magnitude;
    public Animator Anim => animator;
    public int SpeedHash => speedHash;
    public int IsRunningHash => isRunningHash;
    public int IsJumpingHash => isJumpingHash;
    public int IsGroundedHash => isGroundedHash;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (deathHandling == null)
            deathHandling = FindFirstObjectByType<DeathHandling>();

        speedHash = Animator.StringToHash(speedParam);
        isRunningHash = Animator.StringToHash(isRunningParam);
        isJumpingHash = Animator.StringToHash(isJumpingParam);
        isGroundedHash = Animator.StringToHash(isGroundedParam);

        idleState = new IdleState(this);
        walkState = new WalkState(this);
        runState = new RunState(this);

        TransitionTo(idleState);
    }


    // Getters for the UI.
    public void BindCameraPivot(Transform pivot)
    {
        cameraPivot = pivot;
    }

    public float GetCharge()
    {
        return charge;
    }

    public float GetMaxCharge()
    {
        return maxCharge;
    }

    public bool GetCtrlHeld()
    {
        return ctrlHeld;
    }

    private static bool IsGameplayBlocked => Time.timeScale <= 0f; // Check if the game is paused

    #region Record Inputs

    public void OnMove(InputValue value)
    {
        if (IsGameplayBlocked) { moveInput = Vector2.zero; return; }
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (IsGameplayBlocked) { lookInput = Vector2.zero; return; }
        lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        if (IsGameplayBlocked) { sprintHeld = false; return; }
        sprintHeld = value.isPressed;
    }

    // Jump input checks for both normal and charged jumps using 
    public void OnJump(InputValue value)
    {
        if (IsGameplayBlocked) return;
        if (!value.isPressed) return;

        if (ctrlHeld && IsGrounded())
        {
            charge += chargePerPress;
            charge = Mathf.Clamp(charge, 0f, maxCharge);
            Debug.Log($"Charge added: {charge}");
        }
        else if (!ctrlHeld && IsGrounded())
        {
            verticalVelocity = jumpForce; // Normal jump
            SetJumping(true);
            Debug.Log("Normal jump");
        }
    }

    // crouch for pump-action jump using callback context to detect when the key is pressed and released
    public void OnCrouch(InputValue value)
    {
        if (IsGameplayBlocked) return;
        bool wasHeld = ctrlHeld;
        ctrlHeld = value.isPressed;

        // On release player jumps 
        if (wasHeld && !ctrlHeld)
        {
            if (charge > 0f && IsGrounded())
            {
                // Use pickup multiplier or otherwise use base chargeMultiplier
                float activeMultiplier = pickupMultiplierUses > 0 ? pickupChargeMultiplier : chargeMultiplier;
                float launchForce = charge * activeMultiplier;
                verticalVelocity = Mathf.Max(verticalVelocity, launchForce);
                SetJumping(true);

                // consume one pickup use if active
                if (pickupMultiplierUses > 0)
                {
                    pickupMultiplierUses--;
                    if (pickupMultiplierUses == 0)
                        pickupChargeMultiplier = 0f;
                }

                charge = 0f;
            }
        }
    }

    public void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            EventHandler.OnPauseRequested?.Invoke();
        }
    }

    public void OnToggleDialogue(InputValue value)
    {
        if (!value.isPressed)
            return;

        var dialogueUI = FindFirstObjectByType<DialogueUIManager>();
        if (dialogueUI == null)
            return;

        // Allow closing dialogue while it has paused the game; block opening during other pauses.
        if (IsGameplayBlocked && !dialogueUI.IsDialoguePanelVisible)
            return;

        dialogueUI.ToggleDialoguePanel();
    }

    #endregion

    // Apply a temporary charge multiplier.
    public void ApplyChargeMultiplier(float multiplier, int uses)
    {
        if (uses >= 0)
        {
            pickupChargeMultiplier = multiplier;
            pickupMultiplierUses = uses;
            Debug.Log($"Applied pickup multiplier {multiplier} for {uses} uses");
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void Update()
    {
        if (IsGameplayBlocked)
            return;

        Vector3 desiredDir = transform.forward * moveInput.y + transform.right * moveInput.x;
        if (desiredDir.sqrMagnitude > 1f) desiredDir.Normalize();

        float targetSpeed = sprintHeld ? runSpeed : walkSpeed;
        Vector3 desiredVel = desiredDir * targetSpeed;

        bool grounded = IsGrounded();

        // Don't use ground movement while going up.
        bool useGroundMovement = grounded && verticalVelocity <= 0f;

        if (useGroundMovement)
        {
            bool hasMoveInput = moveInput.sqrMagnitude > 0.01f;
            float accel = hasMoveInput ? groundAcceleration : groundDeceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredVel, accel * Time.deltaTime);
        }
        else if (airControl > 0f)
        {
            // Limited mid-air steering; scaled by airControl (0 = none, skip entirely).
            float airAccel = groundAcceleration * airControl;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredVel, airAccel * Time.deltaTime);
        }
        // else: airborne with airControl 0 — keep launch momentum, ignore WASD

        verticalVelocity += gravity * Time.deltaTime;
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -0.5f;

        Vector3 move = horizontalVelocity * Time.deltaTime;
        move.y = verticalVelocity * Time.deltaTime;

        transform.Translate(move, Space.World);

        // Charge cannot be built up mid air
        if (!grounded)
            charge = 0f;

        if (grounded && isJumping && verticalVelocity <= 0f)
            SetJumping(false);

        UpdateLongFallDeath(grounded);

        UpdateAnimationStateMachine();
    }

    private void UpdateLongFallDeath(bool grounded)
    {
        if (!killAfterLongFall || secondsFallingToDie <= 0f)
            return;

        if (grounded)
        {
            // Only apply the "long fall" consequence once we land.
            if (!longFallDeathTriggered && fallingTimer >= secondsFallingToDie)
            {
                longFallDeathTriggered = true;
                if (deathHandling != null)
                    deathHandling.KillPlayer();
            }

            fallingTimer = 0f;
            longFallDeathTriggered = false;
            return;
        }

        bool isFallingDown = verticalVelocity <= fallingVelocityThreshold;
        if (!isFallingDown)
        {
            fallingTimer = 0f;
            return;
        }

        fallingTimer += Time.deltaTime;
    }

    private void UpdateAnimationStateMachine()
    {
        bool grounded = IsGrounded();

        if (animator != null)
        {
            animator.SetBool(isGroundedHash, grounded);
            animator.SetFloat(speedHash, GetAnimatorSpeed());
            animator.SetBool(isRunningHash, sprintHeld && HorizontalSpeed > 0.1f);
        }

        if (!grounded) return;

        if (MoveMagnitude < 0.1f && HorizontalSpeed < 0.1f)
            TransitionTo(idleState);
        else if (sprintHeld)
            TransitionTo(runState);
        else
            TransitionTo(walkState);
    }

    // Maps actual horizontal speed to blend tree thresholds.
    private float GetAnimatorSpeed()
    {
        float speed = HorizontalSpeed;
        if (speed < 0.05f)
            return 0f;

        if (speed <= walkSpeed)
            return Mathf.Lerp(0f, 0.5f, speed / walkSpeed);

        if (runSpeed <= walkSpeed)
            return 0.5f;

        return Mathf.Lerp(0.5f, 1f, Mathf.Clamp01((speed - walkSpeed) / (runSpeed - walkSpeed)));
    }

    public void TransitionTo(IPlayerState next)
    {
        if (next == null || ReferenceEquals(currentState, next))
            return;

        currentState?.Exit();
        currentState = next;
        currentState.Enter();
    }

    private void SetJumping(bool value)
    {
        isJumping = value;
        if (animator != null)
        {
            animator.SetBool(isJumpingHash, value);

            // force the jump state to start immediately on button press.
            if (value && forceJumpAnimationOnPress && !string.IsNullOrWhiteSpace(jumpStateName))
            {
                animator.CrossFadeInFixedTime(jumpStateName, jumpCrossFadeTime);
            }
        }
    }

    private void LateUpdate()
    {
        if (IsGameplayBlocked)
            return;

        // Camera rotation - yaw rotates the player, pitch rotates the camera pivot
        float yaw = lookInput.x * lookSensitivity;
        transform.Rotate(0f, yaw, 0f);

        if (cameraPivot != null)
        {
            pitch -= lookInput.y * lookSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        if (currentPlatform != null)
        {
            // Calculate how much the platform moved since last frame
            Vector3 platformDelta = currentPlatform.position - lastPlatformPosition;

            transform.position += platformDelta;

            // Update last position for next frame
            lastPlatformPosition = currentPlatform.position;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatform = collision.transform;
            lastPlatformPosition = currentPlatform.position;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatform = null;
        }
    }


    public bool CanSpendMoney(int amount)
    {
        if (playerRef.GetComponent<PlayerStats>().coins >= amount)
        {
            playerRef.GetComponent<PlayerStats>().coins -= amount;
            return true;
        }

        return false;
    }

    // Exposed for states
    public bool IsGroundedPublic() => IsGrounded();
}