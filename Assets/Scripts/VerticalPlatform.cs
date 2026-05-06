using UnityEngine;

public class VerticalPlatform : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float upDuration = 2f;
    public float downDuration = 2f;

    Vector3 start;

    float timer;
    bool goingUp = true;

    void Awake() => start = transform.position;

    void Update()
    {
        transform.position += (goingUp ? Vector3.up : Vector3.down) * (moveSpeed * Time.deltaTime);
        timer += Time.deltaTime;

        if (timer >= (goingUp ? upDuration : downDuration))
        {
            timer = 0f;
            goingUp = !goingUp;
        }
    }
}
