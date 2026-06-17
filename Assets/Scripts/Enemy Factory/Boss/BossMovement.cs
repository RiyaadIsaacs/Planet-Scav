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

    private NavMeshAgent _agent;
    private BossState _state = BossState.Patrol;
    private int _currentNodeIndex;
    private int _previousNodeIndex = -1;
    private int _waypointsSinceRest;
    private float _stunTimer;
    private Vector3 _rushTargetPosition;
    private bool _hasRushTarget;
    private Vector3 _activeDestination;
    private bool _hasActiveDestination;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
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
            _agent.Warp(hit.position);
        else
            Debug.LogWarning($"BossMovement could not sample NavMesh for {name}.", this);

        _currentNodeIndex = Mathf.Clamp(_startNodeIndex, 0, _patrolPath.WaypointCount - 1);
        SyncLocomotion();
        ApplyCurrentSpeed();
        GoToGraphNode(_currentNodeIndex);
    }

    private void Update()
    {
        if (_agent.pathPending)
            return;

        switch (_state)
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
        if (_agent == null)
            return;

        // High acceleration + no auto-braking keeps movement at a steady speed
        // instead of easing in/out around each waypoint.
        _agent.acceleration = _agentAcceleration;
        _agent.autoBraking = false;
        _agent.stoppingDistance = 0f;
    }

    private void ApplyCurrentSpeed()
    {
        if (_agent == null)
            return;

        _agent.speed = _state == BossState.Rush ? _rushSpeed : _patrolSpeed;
    }

    public void SetPatrolPath(PatrolWaypointPath path) => _patrolPath = path;

    private void UpdatePatrol()
    {
        if (!_hasActiveDestination)
            return;

        if (!HasReachedPosition(_activeDestination))
            return;

        _waypointsSinceRest++;

        if (_waypointsSinceRest >= _waypointsBeforeRush)
        {
            BeginRush();
            return;
        }

        _previousNodeIndex = _currentNodeIndex;
        var nextIndex = _patrolPath.GetRandomNeighbor(_currentNodeIndex, _previousNodeIndex);
        if (nextIndex == _currentNodeIndex)
        {
            Debug.LogWarning(
                $"{name}: No valid neighbour from graph node {_currentNodeIndex}. Check neighbour indices on { _patrolPath.name }.",
                this);
            return;
        }

        _currentNodeIndex = nextIndex;
        GoToGraphNode(_currentNodeIndex);
    }

    private void UpdateRush()
    {
        if (!_hasRushTarget)
        {
            ResumePatrol();
            return;
        }

        if (!HasReachedPosition(_rushTargetPosition))
            return;

        BeginStun();
    }

    private void UpdateStunned()
    {
        _stunTimer -= Time.deltaTime;
        if (_stunTimer > 0f)
            return;

        ResumePatrol();
    }

    private void BeginRush()
    {
        _state = BossState.Rush;
        _agent.isStopped = false;
        ApplyCurrentSpeed();

        _rushTargetPosition = GetCenterOfGraphPatrol();
        _hasRushTarget = true;
        _hasActiveDestination = true;
        _activeDestination = _rushTargetPosition;
        _agent.SetDestination(_rushTargetPosition);
    }

    private void BeginStun()
    {
        _hasRushTarget = false;
        _hasActiveDestination = false;

        _state = BossState.Stunned;
        _stunTimer = _stunDuration;
        _agent.ResetPath();
        _agent.isStopped = true;
        locomotion?.SetStunned(true);
    }

    private void ResumePatrol()
    {
        _waypointsSinceRest = 0;
        _state = BossState.Patrol;
        _agent.isStopped = false;
        locomotion?.SetStunned(false);
        ApplyCurrentSpeed();

        _currentNodeIndex = _patrolPath.FindNearestNodeIndex(transform.position);
        _previousNodeIndex = -1;
        GoToGraphNode(_currentNodeIndex);
    }

    private void GoToGraphNode(int nodeIndex)
    {
        var waypoint = _patrolPath.GetWaypoint(nodeIndex);
        if (waypoint == null)
            return;

        _agent.isStopped = false;
        var destination = waypoint.position;

        if (NavMesh.SamplePosition(destination, out var hit, _sampleMaxDistance, NavMesh.AllAreas))
            destination = hit.position;

        _activeDestination = destination;
        _hasActiveDestination = true;

        if (!_agent.SetDestination(destination))
            Debug.LogWarning($"{name}: SetDestination failed for graph node {nodeIndex}.", this);
    }

    private bool HasReachedPosition(Vector3 targetPosition)
    {
        if (!_agent.isOnNavMesh)
            return false;

        if (_agent.pathPending)
            return false;

        if (_agent.hasPath && !float.IsInfinity(_agent.remainingDistance))
            return _agent.remainingDistance <= _arriveThreshold;

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
