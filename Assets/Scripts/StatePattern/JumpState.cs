using UnityEngine;

public class JumpState : IPlayerState
{
    private PlayerController player;
    private bool jumpCalled;

    public JumpState(PlayerController controller) => player = controller;

    public void PrepareJump() => jumpCalled = true;

    public void Enter()
    {
        if (jumpCalled)
        {
            player.verticalVelocity = player.jumpForce;
            jumpCalled = false;
        }
    }

    public void Update()
    {
        player.verticalVelocity = Mathf.Max(player.verticalVelocity + player.gravity * Time.deltaTime, player.maxFallSpeed);

        Vector3 moveDir = player.transform.forward * player.moveInput.y + player.transform.right * player.moveInput.x;
        Vector3 targetVel = moveDir * player.runSpeed;
        player.horizontalVelocity = Vector3.Lerp(player.horizontalVelocity, targetVel, player.airControl * Time.deltaTime);

        // Apply velocities to the Rigidbody
        player.rb.linearVelocity = new Vector3(player.horizontalVelocity.x, player.verticalVelocity, player.horizontalVelocity.z);

        if (player.IsGrounded() && player.verticalVelocity < 0)
        {
            player.TransitionTo(player.moveInput.magnitude > 0.1f ? player.MoveState : player.IdleState);
        }
    }

    public void HandleJump(bool isPressed) { }
    public void LateUpdate() { }
    public void Exit() => jumpCalled = false;
}