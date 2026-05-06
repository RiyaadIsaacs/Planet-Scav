using UnityEngine;

public class WalkState : IPlayerState
{
    private readonly PlayerController player;
    public WalkState(PlayerController controller) => player = controller;

    public void Enter()
    {
        var anim = player.Anim;
        if (anim == null) return;

        anim.SetBool(player.IsRunningHash, false);
        anim.SetFloat(player.SpeedHash, 0.5f);
    }
    public void Exit() { }
}

