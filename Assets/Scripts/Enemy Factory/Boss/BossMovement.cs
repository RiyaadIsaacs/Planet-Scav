using UnityEngine;
using UnityEngine.AI;

public class BossMovement : MonoBehaviour
{
    private enum BossState
    {
        Patrol,
        Rush,
        Stunned
    }

    [SerializeField] private PatrolWaypointPath _patrolPath;
    [SerializeField] private int _startNodeIndex;

    [SerializeField] private float _patrolSpeed = 4f;
    [SerializeField] private float _rushSpeed = 10f;
    [SerializeField] private int _waypointsBeforeRush = 3;
    [SerializeField] private float _stunDuration = 7f;
    [SerializeField] private float _arriveThreshold = 1.5f;
    [SerializeField] private float _sampleMaxDistance = 40f;
    [SerializeField] private float _agentAcceleration = 1000f;
    [SerializeField] private RockMonsterLocomotion locomotion;

    private NavMeshAgent agent;
    private BossState state = BossState.Patrol;
    private int currentNodeIndex;
    private int previousNodeIndex = -1;
    private int waypointsSinceRest;
    private float stunTimer;
    private Vector3 rushTargetPosition;
    private bool hasRushTarget;
    private Vector3 activeDestination;
    private bool hasActiveDestination;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (locomotion == null)
            locomotion = GetComponent<RockMonsterLocomotion>();
        ConfigureNavMeshForConstantSpeed();
    }

    private void Start()
    {
        if (_patrolPath == null || !_patrolPath.UsesGraphPatrol)
        {
            Debug.LogError($"BossMovement on {name} needs a PatrolWaypointPath with graph patrol enabled.", this);
            enabled = false;
            return;
        }

        if (_patrolPath.WaypointCount == 0)
        {
            Debug.LogError($"BossMovement on {name} has an empty patrol path.", this);
            enabled = false;
            return;
        }

        if (NavMesh.SamplePosition(transform.position, out var hit, _sampleMaxDistance, NavMesh.AllAreas))
            agent.Warp(hit.position);
        else
            Debug.LogWarning($"BossMovement could not sample NavMesh for {name}.", this);

        currentNodeIndex = Mathf.Clamp(_startNodeIndex, 0, _patrolPath.WaypointCount - 1);
        SyncLocomotion();
        ApplyCurrentSpeed();
        GoToGraphNode(currentNodeIndex);
    }

    private void Update()
    {
        if (agent.pathPending)
            return;

        switch (state)
        {
            case BossState.Patrol:
                UpdatePatrol();
                break;
            case BossState.Rush:
                UpdateRush();
                break;
            case BossState.Stunned:
                UpdateStunned();
                break;
        }
    }

    public void ConfigureMovement(float patrolSpeed, float rushSpeed)
    {
        _patrolSpeed = patrolSpeed;
        _rushSpeed = rushSpeed;
        SyncLocomotion();
        ApplyCurrentSpeed();
    }

    public float PatrolSpeed => _patrolSpeed;
    public float RushSpeed => _rushSpeed;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _patrolSpeed = Mathf.Max(0.01f, _patrolSpeed);
        _rushSpeed = Mathf.Max(_patrolSpeed, _rushSpeed);
        SyncLocomotion();
    }
#endif

    private void SyncLocomotion()
    {
        locomotion?.ConfigureSpeeds(_patrolSpeed, _rushSpeed);
    }

    private void ConfigureNavMeshForConstantSpeed()
    {
        if (agent == null)
            return;

        // High acceleration + no auto-braking keeps movement at a steady speed
        // instead of easing in/out around each waypoint.
        agent.acceleration = _agentAcceleration;
        agent.autoBraking = false;
        agent.stoppingDistance = 0f;
    }

    private void ApplyCurrentSpeed()
    {
        if (agent == null)
            return;

        agent.speed = state == BossState.Rush ? _rushSpeed : _patrolSpeed;
    }

    public void SetPatrolPath(PatrolWaypointPath path) => _patrolPath = path;

    private void UpdatePatrol()
    {
        if (!hasActiveDestination)
            return;

        if (!HasReachedPosition(activeDestination))
            return;

        waypointsSinceRest++;

        if (waypointsSinceRest >= _waypointsBeforeRush)
        {
            BeginRush();
            return;
        }

        previousNodeIndex = currentNodeIndex;
        var nextIndex = _patrolPath.GetRandomNeighbor(currentNodeIndex, previousNodeIndex);
        if (nextIndex == currentNodeIndex)
        {
            Debug.LogWarning(
                $"{name}: No valid neighbour from graph node {currentNodeIndex}. Check neighbour indices on {_patrolPath.name}.",
                this);
            return;
        }

        currentNodeIndex = nextIndex;
        GoToGraphNode(currentNodeIndex);
    }

    private void UpdateRush()
    {
        if (!hasRushTarget)
        {
            ResumePatrol();
            return;
        }

        if (!HasReachedPosition(rushTargetPosition))
            return;

        BeginStun();
    }

    private void UpdateStunned()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer > 0f)
            return;

        ResumePatrol();
    }

    private void BeginRush()
    {
        state = BossState.Rush;
        agent.isStopped = false;
        ApplyCurrentSpeed();

        rushTargetPosition = GetCenterOfGraphPatrol();
        hasRushTarget = true;
        hasActiveDestination = true;
        activeDestination = rushTargetPosition;
        agent.SetDestination(rushTargetPosition);
    }

    private void BeginStun()
    {
        hasRushTarget = false;
        hasActiveDestination = false;

        state = BossState.Stunned;
        stunTimer = _stunDuration;
        agent.ResetPath();
        agent.isStopped = true;
        locomotion?.SetStunned(true);
    }

    private void ResumePatrol()
    {
        waypointsSinceRest = 0;
        state = BossState.Patrol;
        agent.isStopped = false;
        locomotion?.SetStunned(false);
        ApplyCurrentSpeed();

        currentNodeIndex = _patrolPath.FindNearestNodeIndex(transform.position);
        previousNodeIndex = -1;
        GoToGraphNode(currentNodeIndex);
    }

    private void GoToGraphNode(int nodeIndex)
    {
        var waypoint = _patrolPath.GetWaypoint(nodeIndex);
        if (waypoint == null)
            return;

        agent.isStopped = false;
        var destination = waypoint.position;

        if (NavMesh.SamplePosition(destination, out var hit, _sampleMaxDistance, NavMesh.AllAreas))
            destination = hit.position;

        activeDestination = destination;
        hasActiveDestination = true;

        if (!agent.SetDestination(destination))
            Debug.LogWarning($"{name}: SetDestination failed for graph node {nodeIndex}.", this);
    }

    private bool HasReachedPosition(Vector3 targetPosition)
    {
        if (!agent.isOnNavMesh)
            return false;

        if (agent.pathPending)
            return false;

        if (agent.hasPath && !float.IsInfinity(agent.remainingDistance))
            return agent.remainingDistance <= _arriveThreshold;

        return Vector3.Distance(transform.position, targetPosition) <= _arriveThreshold;
    }

    private Vector3 GetCenterOfGraphPatrol()
    {
        // "Center" = average of all graph waypoint positions.
        // This avoids requiring dedicated corner transforms.
        var count = _patrolPath.WaypointCount;
        if (count <= 0)
            return transform.position;

        var sum = Vector3.zero;
        var valid = 0;
        for (var i = 0; i < count; i++)
        {
            var wp = _patrolPath.GetWaypoint(i);
            if (wp == null)
                continue;

            sum += wp.position;
            valid++;
        }

        if (valid <= 0)
            return transform.position;

        var center = sum / valid;

        // Ensure the computed center is reachable by NavMesh.
        if (NavMesh.SamplePosition(center, out var hit, _sampleMaxDistance, NavMesh.AllAreas))
            return hit.position;

        return center;
    }
}
