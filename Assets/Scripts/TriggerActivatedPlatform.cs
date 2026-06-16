using System.Collections;
using UnityEngine;

/// <summary>
/// Moves between two world-space points when activated by a player trigger.
/// Similar to DroppingPlatform but for horizontal travel.
/// </summary>
public class TriggerActivatedPlatform : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float activateDelay = 0f;
    [SerializeField] private float arrivalThreshold = 0.02f;
    [SerializeField] private bool toggleBetweenPoints = true;
    [SerializeField] private bool horizontalOnly = true;
    [SerializeField] private bool snapToStartOnAwake = true;

    private bool atEnd;
    private bool isMoving;
    private Coroutine moveRoutine;

    private void Awake()
    {
        if (snapToStartOnAwake && startPoint != null)
            transform.position = GetMoveTarget(startPoint.position);
    }

    public void ActivateFromPlayer()
    {
        if (isMoving || startPoint == null || endPoint == null)
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveAfterDelay());
    }

    private IEnumerator MoveAfterDelay()
    {
        isMoving = true;

        if (activateDelay > 0f)
            yield return new WaitForSeconds(activateDelay);

        var destination = atEnd ? startPoint.position : endPoint.position;
        var target = GetMoveTarget(destination);

        while (Vector3.Distance(transform.position, target) > arrivalThreshold)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime);

            yield return null;
        }

        transform.position = target;

        if (toggleBetweenPoints)
            atEnd = !atEnd;

        isMoving = false;
        moveRoutine = null;
    }

    private Vector3 GetMoveTarget(Vector3 destination)
    {
        if (!horizontalOnly)
            return destination;

        return new Vector3(destination.x, transform.position.y, destination.z);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (startPoint == null || endPoint == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(startPoint.position, endPoint.position);
        Gizmos.DrawWireSphere(startPoint.position, 0.35f);
        Gizmos.DrawWireSphere(endPoint.position, 0.35f);
    }
#endif
}
