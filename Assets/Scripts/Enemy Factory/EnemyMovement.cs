using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    //To identify which WaypointPath to use.
    [SerializeField] private PatrolWaypointPath _patrolPath;

    //Provides a distance that lets the enemy move to the point incase they are far.
    [SerializeField] private float sampleMaxDistance = 2f;

    [SerializeField] private float arriveThreshold = 1f;

    private NavMeshAgent agent;
    private int waypointIndex;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (_patrolPath == null || _patrolPath.WaypointCount == 0)
        {
            Debug.LogError("No PatrolWaypointPath assigned to EnemyMovement script on " + gameObject.name);
            return;
        }

        if (NavMesh.SamplePosition(transform.position, out var hit, sampleMaxDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        else
            Debug.LogWarning("Couldn't find the navmesh" + gameObject.name);

        GoToCurrentWaypoint();
    }

    private void Update()
    {
        if (_patrolPath == null || _patrolPath.WaypointCount == 0)
            return;

        var target = _patrolPath.GetWaypoint(waypointIndex);
        if (target == null)
            return;

        // Wait until Unity finishes computing the path; otherwise remainingDistance is unreliable.
        if (agent.pathPending)
            return;

        if (HasReachedWaypoint(target))
            AdvanceWaypoint();
    }

    public void SetPatrolPath(PatrolWaypointPath path) => _patrolPath = path;

    private bool HasReachedWaypoint(Transform target)
    {
        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > arriveThreshold)
        {
            return false;
        }

        if (agent.isOnNavMesh && agent.hasPath)
        {
            if (!float.IsInfinity(agent.remainingDistance) && agent.remainingDistance > arriveThreshold * 1.5f)
                return false;
        }

        return true;
    }

    private void AdvanceWaypoint()
    {
        waypointIndex = _patrolPath.GetNextIndex(waypointIndex);
        GoToCurrentWaypoint();
    }

    private void GoToCurrentWaypoint()
    {
        var t = _patrolPath.GetWaypoint(waypointIndex);
        if (t == null)
            return;

        bool set = agent.SetDestination(t.position);
        if (!set)
            Debug.LogWarning($"{name}: SetDestination failed. Is the enemy on the NavMesh?", this);
    }

}
