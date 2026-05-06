using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    [SerializeField] public PlayerController player;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("KillPlayer: OnTriggerEnter with " + other.name);
        if (other.CompareTag("Player"))
        {
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerController>();
                if (player == null)
                {
                    Debug.LogError("KillPlayer: no PlayerController in the scene. Add one or assign 'player'.");
                    return;
                }
            }

            player.deathHandling.KillPlayer();
        }
    }
}