using UnityEngine;

public class ChargeState : IPlayerState
{
    private PlayerController player;
    public ChargeState(PlayerController controller) => player = controller;

    public void Enter()
    {
        player.horizontalVelocity = Vector3.zero;
    }

    public void Update()
    {
        if (!player.isCrouchHeld)
        {
            if (player.charge > 0)
            {
                float activeMult = player.pickupMultiplierUses > 0 ? player.pickupChargeMultiplier : player.chargeMultiplier;
                float launchForce = player.charge * activeMult;

                // Set the velocity directly and transition
                player.verticalVelocity = launchForce;

                if (player.pickupMultiplierUses > 0)
                {
                    player.pickupMultiplierUses--;
                    if (player.pickupMultiplierUses == 0) player.pickupChargeMultiplier = 0f;
                }

                player.TransitionTo(player.JumpState);
            }
            else
            {
                player.TransitionTo(player.IdleState);
            }
        }
    }

    public void HandleJump(bool isPressed)
    {
        if (isPressed && player.IsGrounded())
        {
            player.charge = Mathf.Clamp(player.charge + player.chargePerPress, 0, player.maxCharge);
        }
    }

    public void Exit() => player.charge = 0;
    public void LateUpdate() { }
}