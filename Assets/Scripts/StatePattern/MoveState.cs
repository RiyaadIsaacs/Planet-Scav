using UnityEngine;

public class MoveState : IPlayerState
{
    private PlayerController player;
    public MoveState(PlayerController controller) => player = controller;

    public void Enter() { }

    public void Update()
    {
        // 1. Logic Error Fix: Using walkSpeed/runSpeed and isCrouchHeld
        float currentMaxSpeed = player.isCrouchHeld ? player.walkSpeed : player.runSpeed;

        Vector3 desiredDir = player.transform.forward * player.moveInput.y + player.transform.right * player.moveInput.x;
        if (desiredDir.sqrMagnitude > 1f) desiredDir.Normalize();

        if (player.IsGrounded())
        {
            player.horizontalVelocity = desiredDir * currentMaxSpeed;
            if (player.verticalVelocity < 0f) player.verticalVelocity = -0.5f;
        }
        else
        {
            Vector3 desiredVel = desiredDir * currentMaxSpeed;
            player.horizontalVelocity = Vector3.Lerp(player.horizontalVelocity, desiredVel, player.airControl * Time.deltaTime);
            player.verticalVelocity = Mathf.Max(player.verticalVelocity + player.gravity * Time.deltaTime, player.maxFallSpeed);
        }

        // 2. Movement Logic: Restoring transform.Translate to prevent falling through floor
        Vector3 move = player.horizontalVelocity * Time.deltaTime;
        move.y = player.verticalVelocity * Time.deltaTime;
        player.transform.Translate(move, Space.World);

        // 3. State Transitions: Using isCrouchHeld to match PlayerController
        if (player.moveInput.magnitude < 0.1f && player.IsGrounded())
            player.TransitionTo(player.IdleState);

        if (player.isCrouchHeld && player.IsGrounded())
            player.TransitionTo(player.ChargeState);
    }

    public void HandleJump(bool isPressed)
    {
        // Check for isCrouchHeld instead of ctrlHeld
        if (isPressed && player.IsGrounded() && !player.isCrouchHeld)
        {
            player.verticalVelocity = player.jumpForce;
            player.TransitionTo(player.JumpState);
        }
    }

    public void LateUpdate() { }
    public void Exit() { }
}