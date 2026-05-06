
public interface IPlayerState
{
    void Enter();
    void Update();
    void LateUpdate();
    void Exit();
    void HandleJump(bool isPressed);
}