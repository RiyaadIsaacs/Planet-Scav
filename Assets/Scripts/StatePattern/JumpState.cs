using UnityEngine;

public class JumpState : IPlayerState
{
    private PlayerController player;

    public JumpState(PlayerController controller) => player = controller;

    public void Enter() { }
    public void Exit() { }
}