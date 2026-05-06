using UnityEngine;

/// <summary>
/// Moves the platform vertically between two world-space points (bottom → top → bottom).
/// Create two empty transforms in the scene, place them at the low and high positions, assign here.
/// </summary>
public class UpAndDownPlatform : MonoBehaviour
{
    [SerializeField] private Transform bottomPoint;
    [SerializeField] private Transform topPoint;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float arrivalThreshold = 0.02f;

    private bool movingUp = true;

    private void Update()
    {
        if (bottomPoint == null || topPoint == null)
            return;

        Vector3 pos = transform.position;
        Vector3 target = movingUp ? topPoint.position : bottomPoint.position;

        pos.y = Mathf.MoveTowards(pos.y, target.y, speed * Time.deltaTime);

        if (Mathf.Abs(pos.y - target.y) < arrivalThreshold)
            movingUp = !movingUp;

        transform.position = pos;
    }
}
