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

    private bool _ctrlHeld;   
    private float charge;    
    private Vector3 _horizontalVelocity; // units per second

    // pickup-applied temporary multiplier (consumed per charged jump)
    private float pickupChargeMultiplier = 0f;
    private int pickupMultiplierUses = 0;

    // Input values to record from the new input system
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _verticalVelocity; // value to control vertical movement 
    private float _pitch;

    #region Record Inputs

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>();
    }

    // Jump input checks for both normal and charged jumps using 
    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;

        if (_ctrlHeld && IsGrounded())
        {
            charge += chargePerPress;
            charge = Mathf.Clamp(charge, 0f, maxCharge);
            Debug.Log($"Charge added: {charge}");
        }
        else if (!_ctrlHeld && IsGrounded())
        {
            _verticalVelocity = jumpForce; // Normal jump
            Debug.Log("Normal jump");
        }
    }

    // crouch for pump-action jump using callback context to detect when the key is pressed and released
    public void OnCrouch(InputValue value)
    {
        bool wasHeld = _ctrlHeld;
        _ctrlHeld = value.isPressed;

        Debug.Log($"Ctrl: {_ctrlHeld}");

        // On release player jumps 
        if (wasHeld && !_ctrlHeld)
        {
            if (charge > 0f && IsGrounded())
            {
                // Use pickup multiplier or otherwise use base chargeMultiplier
                float activeMultiplier = pickupMultiplierUses > 0 ? pickupChargeMultiplier : chargeMultiplier;
                float launchForce = charge * activeMultiplier;
                _verticalVelocity = Mathf.Max(_verticalVelocity, launchForce);

                // consume one pickup use if active
                if (pickupMultiplierUses > 0)
                {
                    pickupMultiplierUses--;
                    if (pickupMultiplierUses == 0)
                        pickupChargeMultiplier = 0f;
                    Debug.Log($"Pickup multiplier uses left: {pickupMultiplierUses}");
                }

                charge = 0f;
                Debug.Log($"Charged launch: {launchForce}");
            }
        }
    }

    #endregion

    // Public API for pickups to apply a temporary charge multiplier for N charged launches
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
        
        Vector3 desiredDir = transform.forward * _moveInput.y + transform.right * _moveInput.x;
        if (desiredDir.sqrMagnitude > 1f) desiredDir.Normalize();

        if (IsGrounded())
        {
            _horizontalVelocity = desiredDir * speed;
        }
        else
        {
            // No control in air when airControl is 0
            Vector3 desiredVel = desiredDir * speed;
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, desiredVel, airControl * Time.deltaTime);
        }

        _verticalVelocity += gravity * Time.deltaTime;
        if (IsGrounded() && _verticalVelocity < 0f)
            _verticalVelocity = -0.5f;

        Vector3 move = _horizontalVelocity * Time.deltaTime;
        move.y = _verticalVelocity * Time.deltaTime;

        transform.Translate(move, Space.World);

        // Charge cannot be built up mid air
        if (!IsGrounded())
            charge = 0f;
    }

    private void LateUpdate()
    {
        // Camera rotation — yaw rotates the player, pitch rotates the camera pivot
        float yaw = _lookInput.x * lookSensitivity;
        transform.Rotate(0f, yaw, 0f);

        if (cameraPivot != null)
        {
            _pitch -= _lookInput.y * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}