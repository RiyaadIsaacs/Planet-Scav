using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class RockMonsterLocomotion : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string stunnedParam = "Stunned";
    [SerializeField] private float walkAgentSpeed = 4f;
    [SerializeField] private float runAgentSpeed = 10f;
    [SerializeField] private float speedDampTime = 0.12f;

    private NavMeshAgent _agent;
    private int _speedHash;
    private int _stunnedHash;
    private bool _stunned;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        _speedHash = Animator.StringToHash(speedParam);
        _stunnedHash = Animator.StringToHash(stunnedParam);
    }

    public void ConfigureSpeeds(float walkSpeed, float runSpeed)
    {
        walkAgentSpeed = Mathf.Max(0.01f, walkSpeed);
        runAgentSpeed = Mathf.Max(walkAgentSpeed, runSpeed);
    }

    public void SetStunned(bool stunned) => _stunned = stunned;

    private void Update()
    {
        if (animator == null)
            return;

        animator.SetBool(_stunnedHash, _stunned);

        if (_stunned)
        {
            animator.SetFloat(_speedHash, 0f, speedDampTime, Time.deltaTime);
            return;
        }

        float horizontalSpeed = GetAgentSpeed();
        animator.SetFloat(_speedHash, MapSpeedToBlend(horizontalSpeed), speedDampTime, Time.deltaTime);
    }

    private float GetAgentSpeed()
    {
        if (_agent == null)
            return 0f;

        if (_agent.velocity.sqrMagnitude > 0.01f)
            return _agent.velocity.magnitude;

        if (!_agent.isStopped && _agent.hasPath && !_agent.pathPending)
            return _agent.speed;

        return 0f;
    }

    private float MapSpeedToBlend(float speed)
    {
        if (speed < 0.05f)
            return 0f;

        if (speed <= walkAgentSpeed)
            return Mathf.Lerp(0f, 0.5f, speed / walkAgentSpeed);

        if (runAgentSpeed <= walkAgentSpeed)
            return 0.5f;

        return Mathf.Lerp(0.5f, 1f, Mathf.Clamp01((speed - walkAgentSpeed) / (runAgentSpeed - walkAgentSpeed)));
    }
}
