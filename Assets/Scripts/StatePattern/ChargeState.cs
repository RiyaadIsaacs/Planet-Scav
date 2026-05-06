using UnityEngine;

public class ChargeState : IPlayerState
{
    private PlayerController player;
    public ChargeState(PlayerController controller) => player = controller;

    public void Enter() { }

    public void Exit() { }
}