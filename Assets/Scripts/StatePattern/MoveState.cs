using UnityEngine;

public class MoveState : IPlayerState
{
    private PlayerController player;
    public MoveState(PlayerController controller) => player = controller;

    public void Enter() { }
    public void Exit() { }
}