using UnityEngine;
using UnityEngine.InputSystem;

// Player controller for movement, camera control and other features to be decided later
public class PlayerController : MonoBehaviour
{
    // Player WASD movement and jump settings
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
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

    [SerializeField] private float airControl = 0f; // Control how much the player can move in the air - 0 = no control / 1 = full control

    private bool ctrlHeld;   
    private float charge;    
    private Vector3 horizontalVelocity; // units per second

    // pickup-applied temporary multiplier (consumed per charged jump)
    private float pickupChargeMultiplier = 0f;
    private int pickupMultiplierUses = 0;

    // Input values to record from the new input system
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalVelocity; // value to control vertical movement 
    private float pitch;

    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;


    // Getters for the UI.
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

    #region Record Inputs

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    // Jump input checks for both normal and charged jumps using 
    public void OnJump(InputValue value)
    {
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
            Debug.Log("Normal jump");
        }
    }

    // crouch for pump-action jump using callback context to detect when the key is pressed and released
    public void OnCrouch(InputValue value)
    {
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
        
        Vector3 desiredDir = transform.forward * moveInput.y + transform.right * moveInput.x;
        if (desiredDir.sqrMagnitude > 1f) desiredDir.Normalize();

        if (IsGrounded())
        {
            horizontalVelocity = desiredDir * speed;
        }
        else
        {
            // No control in air when airControl is 0
            Vector3 desiredVel = desiredDir * speed;
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, desiredVel, airControl * Time.deltaTime);
        }

        verticalVelocity += gravity * Time.deltaTime;
        if (IsGrounded() && verticalVelocity < 0f)
            verticalVelocity = -0.5f;

        Vector3 move = horizontalVelocity * Time.deltaTime;
        move.y = verticalVelocity * Time.deltaTime;

        transform.Translate(move, Space.World);

        // Charge cannot be built up mid air
        if (!IsGrounded())
            charge = 0f;
    }

    private void LateUpdate()
    {
        // Camera rotation — yaw rotates the player, pitch rotates the camera pivot
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

            // Apply that delta to the player
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

}