using System.Collections;
using UnityEditorInternal;
using UnityEngine;

public class DeathHandling : MonoBehaviour
{
    public Transform player; // Reference to the player transform
    public PlayerCheckpoints checkPoints;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.SetPositionAndRotation(checkPoints._initialPosition, checkPoints._initialRotation);
        }
    }


}
