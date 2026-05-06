using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    //To identify which WaypointPath to use.
    [SerializeField] private PatrolWaypointPath _patrolPath;

    //Provides a distance that lets the enemy move to the point incase they are far.
    [SerializeField] private float _sampleMaxDistance = 2f;

    [SerializeField] private float _arriveThreshold = 1f;

    private NavMeshAgent _agent;
    private int _waypointIndex;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (_patrolPath == null || _patrolPath.WaypointCount == 0)
        {
            Debug.LogError("No PatrolWaypointPath assigned to EnemyMovement script on " + gameObject.name);
            return;
        }

        if (NavMesh.SamplePosition(transform.position, out var hit, _sampleMaxDistance, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position);
        }

        else
            Debug.LogWarning("Couldn't find the navmesh" + gameObject.name);
    }

    private void Update()
    {
        if (_patrolPath == null || _patrolPath.WaypointCount == 0)
            return;

        var target = _patrolPath.GetWaypoint(_waypointIndex);
        if (target == null)
            return;

        // Wait until Unity finishes computing the path; otherwise remainingDistance is unreliable.
        if (_agent.pathPending)
            return;

        if (HasReachedWaypoint(target))
            AdvanceWaypoint();
    }

    public void SetPatrolPath(PatrolWaypointPath path) => _patrolPath = path;

    private bool HasReachedWaypoint(Transform target)
    {
        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > _arriveThreshold)
        {
            return false;
        }

        if (_agent.isOnNavMesh && _agent.hasPath)
        {
            if (!float.IsInfinity(_agent.remainingDistance) && _agent.remainingDistance > _arriveThreshold * 1.5f)
                return false;
        }

        return true;
    }

    private void AdvanceWaypoint()
    {
        _waypointIndex = _patrolPath.GetNextIndex(_waypointIndex);
        GoToCurrentWaypoint();
    }

    private void GoToCurrentWaypoint()
    {
        var t = _patrolPath.GetWaypoint(_waypointIndex);
        if (t == null)
            return;

        bool set = _agent.SetDestination(t.position);
        if (!set)
            Debug.LogWarning($"{name}: SetDestination failed. Is the enemy on the NavMesh?", this);
    }

}
