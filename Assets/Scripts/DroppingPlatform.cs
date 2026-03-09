using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DroppingPlatform : MonoBehaviour
{
    public float fallDelay = 2f;
    public float returnDelay = 2f;
    public float speed = 3f;

    public Transform oldPosition;   // starting position reference
    public Transform fallLimit;     // target position reference

    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(DropAfterDelay());
    }
    private void OnTriggerEnter(Collider other) //trigger not working
    {
        Debug.Log("Collided");
        StartCoroutine(ReturnPlatform());
    }

    IEnumerator DropAfterDelay()
    {
        yield return new WaitForSeconds(fallDelay);

        // Move only along Y until reaching fallLimit
        while (Mathf.Abs(transform.position.y - fallLimit.position.y) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, fallLimit.position.y, speed * Time.deltaTime);
            transform.position = pos;

            yield return null; // wait for next frame
        }
    }


    IEnumerator ReturnPlatform()
    {
        yield return new WaitForSeconds(returnDelay);

        

        Debug.Log("Returned");

        // Snap back to the full oldPosition
        transform.position = oldPosition.position;
    }


}