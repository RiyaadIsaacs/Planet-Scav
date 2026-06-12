using UnityEngine;

public class IdleState : IPlayerState
{
    private PlayerController player;
    public IdleState(PlayerController controller) => player = controller;

    public void Enter()
    {
        var anim = player.Anim;
        if (anim == null) return;

        anim.SetBool(player.IsRunningHash, false);
    }
    public void Exit() { }
}