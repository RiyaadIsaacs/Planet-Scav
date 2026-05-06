using UnityEngine;

public class IdleState : IPlayerState
{
    private PlayerController player;
    public IdleState(PlayerController controller) => player = controller;

    public void Enter() { player.horizontalVelocity = Vector3.zero; }

    public void Update()
    {
        if (player.IsGrounded())
        {
            player.verticalVelocity = -0.5f;
            //player.SnapToGround(); // Snaps feet to floor
        }
        else
        {
            player.verticalVelocity = Mathf.Max(player.verticalVelocity + player.gravity * Time.deltaTime, player.maxFallSpeed);
        }

        player.transform.Translate(new Vector3(0, player.verticalVelocity * Time.deltaTime, 0), Space.World);

        if (player.moveInput.magnitude > 0.1f) player.TransitionTo(player.MoveState);
        if (player.isCrouchHeld) player.TransitionTo(player.ChargeState);
    }

    public void HandleJump(bool isPressed) { if (isPressed && player.IsGrounded()) { player.JumpState.PrepareJump(); player.TransitionTo(player.JumpState); } }
    public void LateUpdate() { }
    public void Exit() { }
}