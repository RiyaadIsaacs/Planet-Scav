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
    [SerializeField] private float collisionSkinWidth = 0.05f;
    [SerializeField] private float wallBlockMinNormalY = 0.55f;
    [SerializeField] private float groundStickDistance = 0.55f;

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

    private CapsuleCollider bodyCollider;
    private Rigidbody bodyRigidbody;

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

        bodyCollider = GetComponent<CapsuleCollider>();
        bodyRigidbody = GetComponent<Rigidbody>();
        if (bodyRigidbody != null)
        {
            bodyRigidbody.isKinematic = true;
            bodyRigidbody.useGravity = false;
        }

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

    public void OnAttack(InputValue value)
    {
        if (IsGameplayBlocked || !value.isPressed)
            return;

        GetComponent<PlayerShooting>()?.TryShoot();
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

    private Vector3 WorldPosition => bodyRigidbody != null ? bodyRigidbody.position : transform.position;

    private void SetWorldPosition(Vector3 position)
    {
        transform.position = position;
        if (bodyRigidbody != null)
            bodyRigidbody.position = position;
    }

    private float GetFeetWorldY(Vector3 worldPosition)
    {
        if (bodyCollider == null)
            return worldPosition.y;

        GetCapsuleWorldPointsAt(worldPosition, out _, out Vector3 bottom, out _);
        return bottom.y;
    }

    private bool TryGroundCheck(out RaycastHit hit)
    {
        Vector3 pos = WorldPosition;
        float castDistance = groundCheckDistance + 0.1f;

        // Primary check from the player root (this worked before the collision refactor).
        if (Physics.Raycast(pos + Vector3.up * 0.1f, Vector3.down, out hit, castDistance, groundLayer, QueryTriggerInteraction.Ignore))
            return true;

        if (bodyCollider == null)
        {
            hit = default;
            return false;
        }

        // Secondary check from the capsule bottom for when the root sits above the surface.
        GetCapsuleWorldPointsAt(pos, out _, out Vector3 bottom, out float radius);
        Vector3 feetOrigin = bottom + Vector3.up * 0.1f;
        return Physics.Raycast(feetOrigin, Vector3.down, out hit, radius + 0.2f, groundLayer, QueryTriggerInteraction.Ignore);
    }

    private bool IsGrounded()
    {
        if (verticalVelocity > 0.05f)
            return false;

        if (!TryGroundCheck(out RaycastHit hit))
            return false;

        float gap = GetFeetWorldY(WorldPosition) - hit.point.y;
        return gap >= -0.05f && gap <= groundStickDistance;
    }

    private Vector3 ClampVerticalMovement(Vector3 movement, Vector3 worldPosition)
    {
        if (movement.sqrMagnitude < 0.000001f || bodyCollider == null)
            return movement;

        GetCapsuleWorldPointsAt(worldPosition, out Vector3 point1, out Vector3 point2, out float radius);
        float distance = movement.magnitude;
        Vector3 direction = movement / distance;

        if (Physics.CapsuleCast(
                point1,
                point2,
                radius,
                direction,
                out RaycastHit hit,
                distance + collisionSkinWidth,
                groundLayer,
                QueryTriggerInteraction.Ignore))
        {
            float allowedDistance = Mathf.Max(hit.distance - collisionSkinWidth, 0f);
            return direction * allowedDistance;
        }

        return movement;
    }

    private void StickToGround()
    {
        if (verticalVelocity > 0.05f)
            return;

        if (!TryGroundCheck(out RaycastHit hit))
            return;

        float gap = GetFeetWorldY(WorldPosition) - hit.point.y;
        if (gap < -0.05f || gap > groundStickDistance)
            return;

        if (Mathf.Abs(gap) > 0.001f)
            SetWorldPosition(WorldPosition + Vector3.down * gap);

        verticalVelocity = 0f;
    }

    private Vector3 ClampSprintWalls(Vector3 movement, Vector3 worldPosition)
    {
        if (movement.sqrMagnitude < 0.000001f || bodyCollider == null)
            return movement;

        GetCapsuleWorldPointsAt(worldPosition, out Vector3 point1, out Vector3 point2, out float radius);

        // Cast with the upper portion of the capsule so floor/ramp geometry does not kill speed.
        float bodyHeight = point1.y - point2.y;
        Vector3 castBottom = point2 + Vector3.up * Mathf.Min(radius + 0.15f, bodyHeight * 0.45f);
        Vector3 castTop = point1;
        float castRadius = radius * 0.9f;

        float distance = movement.magnitude;
        Vector3 direction = movement / distance;

        if (Physics.CapsuleCast(
                castTop,
                castBottom,
                castRadius,
                direction,
                out RaycastHit hit,
                distance + collisionSkinWidth,
                groundLayer,
                QueryTriggerInteraction.Ignore)
            && hit.normal.y < wallBlockMinNormalY)
        {
            float allowedDistance = Mathf.Max(hit.distance - collisionSkinWidth, 0f);
            return direction * allowedDistance;
        }

        return movement;
    }

    private void GetCapsuleWorldPointsAt(Vector3 worldPosition, out Vector3 point1, out Vector3 point2, out float radius)
    {
        Vector3 scale = transform.lossyScale;
        radius = bodyCollider.radius * Mathf.Max(scale.x, scale.z);
        float height = bodyCollider.height * scale.y;
        float halfHeight = Mathf.Max(height * 0.5f - radius, 0.01f);
        Vector3 center = (worldPosition - transform.position) + transform.TransformPoint(bodyCollider.center);
        point1 = center + Vector3.up * halfHeight;
        point2 = center - Vector3.up * halfHeight;
    }

    private void ApplyMovement(Vector3 horizontalMove, bool sprinting, bool grounded)
    {
        Vector3 position = WorldPosition;

        if (sprinting && horizontalMove.sqrMagnitude > 0.000001f)
            horizontalMove = ClampSprintWalls(horizontalMove, position);

        position += horizontalMove;

        if (verticalVelocity > 0f)
        {
            Vector3 upMove = Vector3.up * (verticalVelocity * Time.deltaTime);
            position += ClampVerticalMovement(upMove, position);
        }
        else if (!grounded)
        {
            Vector3 downMove = Vector3.up * (verticalVelocity * Time.deltaTime);
            position += ClampVerticalMovement(downMove, position);
        }

        SetWorldPosition(position);
        StickToGround();
    }

    private static Transform FindPlatformRoot(Transform hitTransform)
    {
        var current = hitTransform;
        while (current != null)
        {
            if (current.CompareTag("Platform"))
                return current;
            current = current.parent;
        }

        return null;
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

        if (!grounded)
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalMove = horizontalVelocity * Time.deltaTime;

        ApplyMovement(horizontalMove, sprintHeld, grounded);

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

        UpdatePlatformCarry();
    }

    private Transform GetPlatformUnderFeet()
    {
        if (!TryGroundCheck(out RaycastHit hit))
            return null;

        return FindPlatformRoot(hit.collider.transform);
    }

    private void UpdatePlatformCarry()
    {
        var platformUnderFeet = GetPlatformUnderFeet();
        if (platformUnderFeet == null)
        {
            currentPlatform = null;
            return;
        }

        if (currentPlatform != platformUnderFeet)
        {
            currentPlatform = platformUnderFeet;
            lastPlatformPosition = currentPlatform.position;
            return;
        }

        var platformDelta = currentPlatform.position - lastPlatformPosition;
        if (platformDelta.sqrMagnitude > 0f)
            SetWorldPosition(WorldPosition + platformDelta);

        lastPlatformPosition = currentPlatform.position;
        StickToGround();
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