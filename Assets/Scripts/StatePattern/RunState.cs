using UnityEngine;

public class RunState : IPlayerState
{
    private readonly PlayerController player;
    public RunState(PlayerController controller) => player = controller;

    public void Enter()
    {
        var anim = player.Anim;
        if (anim == null) return;

        anim.SetBool(player.IsRunningHash, true);
        anim.SetBool(player.IsJumpingHash, false);
        anim.SetFloat(player.SpeedHash, 1f);
    }
    public void Exit() { }
}

