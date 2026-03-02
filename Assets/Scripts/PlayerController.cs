using UnityEngine;
using UnityEngine.InputSystem;

// Player controller for movement, camera control and other features to be decided later
public class PlayerController : MonoBehaviour
{
    // Player WASD movement and jump settings
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravity = -18f;
    [SerializeField] private float groundCheckDistance = 1.1f; // Allowable distance from ground to player
    [SerializeField] private LayerMask groundLayer = -1;

    //Player camera control with mouse
    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    // Input values to record from the new input system
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _verticalVelocity;
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

    public void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded())
            _verticalVelocity = jumpForce;
    }
    #endregion


    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void Update()
    {
        // Movement and gravity remain in Update (unchanged)
        Vector3 move = transform.forward * _moveInput.y + transform.right * _moveInput.x;
        move = move.normalized * (speed * Time.deltaTime);

        // Gravity and jump control 
        _verticalVelocity += gravity * Time.deltaTime;
        if (IsGrounded() && _verticalVelocity < 0f)
            _verticalVelocity = -0.5f; 
        move.y = _verticalVelocity * Time.deltaTime;

        transform.Translate(move, Space.World);
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
