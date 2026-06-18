using System;
using UnityEngine;

[Serializable]
public class GraphWaypoint
{
    public Transform waypoint;
    public int[] neighborIndices;
}

public class PatrolWaypointPath : MonoBehaviour
{
    [Header("Linear Patrol (normal enemies)")]
    [SerializeField] private Transform[] _waypointsInPatrolOrder;

    [Header("Graph Patrol (boss)")]
    [SerializeField] private bool _useGraphPatrol;
    [SerializeField] private GraphWaypoint[] _graphWaypoints;

    private CustomLinkedList<Transform> path;
    private CustomGraph<Transform> graph;
    private int[] neighborBuffer = new int[8];

    public bool UsesGraphPatrol => _useGraphPatrol;
    public int WaypointCount => _useGraphPatrol
        ? (graph != null ? graph.NodeCount : 0)
        : (path != null ? path.Count : 0);

    private void Awake()
    {
        if (_useGraphPatrol)
            BuildGraphFromInspector();
        else
            BuildPathFromInspector();
    }

    private void BuildPathFromInspector()
    {
        path = new CustomLinkedList<Transform>();

        if (_waypointsInPatrolOrder == null)
            return;

        foreach (var wp in _waypointsInPatrolOrder)
        {
            if (wp != null)
                path.AddLast(wp);
        }
    }

    private void BuildGraphFromInspector()
    {
        graph = new CustomGraph<Transform>();

        if (_graphWaypoints == null || _graphWaypoints.Length == 0)
        {
            Debug.LogError($"PatrolWaypointPath on {name} has graph patrol enabled but no graph waypoints.", this);
            return;
        }

        for (var i = 0; i < _graphWaypoints.Length; i++)
        {
            var waypoint = _graphWaypoints[i].waypoint;
            if (waypoint == null)
            {
                Debug.LogError($"PatrolWaypointPath on {name} has a null graph waypoint at index {i}.", this);
                continue;
            }

            graph.AddNode(waypoint);
        }

        for (var i = 0; i < _graphWaypoints.Length; i++)
        {
            var neighbors = _graphWaypoints[i].neighborIndices;
            if (neighbors == null || neighbors.Length == 0)
            {
                Debug.LogWarning($"PatrolWaypointPath on {name}: graph node {i} has no neighbour indices.", this);
            }

            if (neighbors == null)
                continue;

            foreach (var neighborIndex in neighbors)
            {
                if (neighborIndex < 0 || neighborIndex >= _graphWaypoints.Length)
                {
                    Debug.LogError(
                        $"PatrolWaypointPath on {name}: waypoint {i} references invalid neighbor index {neighborIndex}.",
                        this);
                    continue;
                }

                graph.AddEdge(i, neighborIndex);
            }
        }
    }

    public Transform GetWaypoint(int index)
    {
        if (_useGraphPatrol)
        {
            if (graph == null || graph.NodeCount == 0)
                return null;

            try
            {
                return graph.GetNodeValue(index);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        if (path == null || path.Count == 0)
            return null;

        var i = Mathf.Clamp(index, 0, path.Count - 1);
        try
        {
            return path.GetAt(i);
        }
        catch
        {
            return null;
        }
    }

    public int GetNextIndex(int currentIndex)
    {
        if (_useGraphPatrol)
        {
            Debug.LogWarning($"GetNextIndex called on graph patrol path {name}. Use GetRandomNeighbor instead.", this);
            return currentIndex;
        }

        if (path == null || path.Count == 0)
            return 0;

        return (currentIndex + 1) % path.Count;
    }

    public int GetRandomNeighbor(int currentIndex, int excludeIndex = -1)
    {
        if (!_useGraphPatrol || graph == null || graph.NodeCount == 0)
            return GetNextIndex(currentIndex);

        var neighborCount = graph.GetNeighborCount(currentIndex);
        if (neighborCount == 0)
            return currentIndex;

        if (neighborBuffer.Length < neighborCount)
            neighborBuffer = new int[neighborCount];

        graph.CopyNeighbors(currentIndex, neighborBuffer, out var copiedCount);

        var validCount = 0;
        for (var i = 0; i < copiedCount; i++)
        {
            if (neighborBuffer[i] == excludeIndex)
                continue;

            neighborBuffer[validCount] = neighborBuffer[i];
            validCount++;
        }

        if (validCount == 0)
            return neighborBuffer[UnityEngine.Random.Range(0, copiedCount)];

        return neighborBuffer[UnityEngine.Random.Range(0, validCount)];
    }

    public int FindNearestNodeIndex(Vector3 worldPosition)
    {
        if (!_useGraphPatrol || graph == null || graph.NodeCount == 0)
            return 0;

        var nearestIndex = 0;
        var nearestDistance = float.MaxValue;

        for (var i = 0; i < graph.NodeCount; i++)
        {
            var waypoint = graph.GetNodeValue(i);
            if (waypoint == null)
                continue;

            var distance = Vector3.Distance(worldPosition, waypoint.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_useGraphPatrol)
            DrawGraphGizmos();
        else
            DrawLinearGizmos();
    }

    private void DrawLinearGizmos()
    {
        if (_waypointsInPatrolOrder == null)
            return;

        Gizmos.color = Color.cyan;
        Transform previous = null;

        foreach (var wp in _waypointsInPatrolOrder)
        {
            if (wp == null)
                continue;

            Gizmos.DrawSphere(wp.position, 0.25f);
            if (previous != null)
                Gizmos.DrawLine(previous.position, wp.position);
            previous = wp;
        }

        if (previous != null && _waypointsInPatrolOrder.Length > 1)
        {
            var first = _waypointsInPatrolOrder[0];
            if (first != null)
                Gizmos.DrawLine(previous.position, first.position);
        }
    }

    private void DrawGraphGizmos()
    {
        if (_graphWaypoints == null)
            return;

        for (var i = 0; i < _graphWaypoints.Length; i++)
        {
            var waypoint = _graphWaypoints[i].waypoint;
            if (waypoint == null)
                continue;

            var neighborCount = _graphWaypoints[i].neighborIndices != null
                ? _graphWaypoints[i].neighborIndices.Length
                : 0;

            Gizmos.color = neighborCount >= 3 ? Color.yellow : Color.cyan;
            Gizmos.DrawSphere(waypoint.position, 0.35f);

            var neighbors = _graphWaypoints[i].neighborIndices;
            if (neighbors == null)
                continue;

            foreach (var neighborIndex in neighbors)
            {
                if (neighborIndex < 0 || neighborIndex >= _graphWaypoints.Length)
                    continue;

                var neighbor = _graphWaypoints[neighborIndex].waypoint;
                if (neighbor == null)
                    continue;

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(waypoint.position, neighbor.position);
            }
        }
    }
#endif
}
