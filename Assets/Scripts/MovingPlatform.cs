using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform leftPoint;   // Assign in Inspector
    public Transform rightPoint;  // Assign in Inspector
    public float speed = 3f;

    private bool movingRight = true;

    void Update()
    {
        Vector3 pos = transform.position;

        if (movingRight)
        {
            pos.x = Mathf.MoveTowards(pos.x, rightPoint.position.x, speed * Time.deltaTime);

            if (Mathf.Abs(pos.x - rightPoint.position.x) < 0.01f)
            {
                movingRight = false;
            }
        }
        else
        {
            pos.x = Mathf.MoveTowards(pos.x, leftPoint.position.x, speed * Time.deltaTime);

            if (Mathf.Abs(pos.x - leftPoint.position.x) < 0.01f)
            {
                movingRight = true;
            }
        }

        transform.position = pos; // Apply only X change
    }
}