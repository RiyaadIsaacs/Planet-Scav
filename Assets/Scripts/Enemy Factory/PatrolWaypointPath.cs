using UnityEngine;

public sealed class PatrolWaypointPath : MonoBehaviour
{
    [SerializeField] private Transform[] _waypointsInPatrolOrder;

    private CustomLinkedList<Transform> _path;

    public int WaypointCount => _path != null ? _path.Count : 0;

    private void Awake()
    {
        BuildPathFromInspector();
    }

    private void BuildPathFromInspector()
    {
        _path = new CustomLinkedList<Transform>();

        if (_waypointsInPatrolOrder == null)
            return;

        foreach (var wp in _waypointsInPatrolOrder)
        {
            if (wp != null)
                _path.AddLast(wp);
        }
    }

    /// <summary>Returns the transform at index, or null if none.</summary>
    public Transform GetWaypoint(int index)
    {
        if (_path == null || _path.Count == 0)
            return null;

        var i = Mathf.Clamp(index, 0, _path.Count - 1);
        try
        {
            return _path.GetAt(i);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Loops: last waypoint wraps to 0.</summary>
    public int GetNextIndex(int currentIndex)
    {
        if (_path == null || _path.Count == 0)
            return 0;

        return (currentIndex + 1) % _path.Count;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
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
#endif
}