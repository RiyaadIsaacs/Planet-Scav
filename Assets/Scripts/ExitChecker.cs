using UnityEngine;

public class ExitChecker : MonoBehaviour
{
    public Transform gateMove;

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("someone reached the exit");
        var player = other; //.GetComponent<PlayerController>();
        if (player.CompareTag("Player"))
        {
            Debug.Log("Player has reached the exit.");
            if (player.GetComponent<PlayerStats>().upgradeCheck)
            {
                Debug.Log("Player has reached the exit and has the upgrade. Moving gate.");
                gameObject.transform.position = new Vector3(gateMove.position.x, gateMove.position.y, gateMove.position.z);
            }
        }
    }
}
