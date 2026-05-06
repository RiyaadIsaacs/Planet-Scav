using UnityEngine;

public class JumpAnimState : IPlayerState
{
    private readonly PlayerController player;
    public JumpAnimState(PlayerController controller) => player = controller;

    public void Enter()
    {
        var anim = player.Anim;
        if (anim == null) return;

        anim.SetBool(player.IsJumpingHash, true);
    }
    public void Exit()
    {
        var anim = player.Anim;
        if (anim == null) return;

        anim.SetBool(player.IsJumpingHash, false);
    }
}

