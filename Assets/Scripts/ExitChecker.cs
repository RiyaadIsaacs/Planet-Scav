using UnityEngine;

public class ExitChecker : MonoBehaviour
{
    public Transform gateMove;
    [SerializeField] private float moveSpeed = 2f;

    private bool moving;
    private Vector3 targetPos;

    private void Update()
    {
        if (!moving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (transform.position == targetPos)
            moving = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        var playerStats = other.GetComponent<PlayerStats>();
        if (playerStats != null && playerStats.upgradeCheck && gateMove != null)
        {
            targetPos = gateMove.position;
            moving = true;
        }
    }
}
