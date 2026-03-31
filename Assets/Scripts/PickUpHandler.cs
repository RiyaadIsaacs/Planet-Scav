using UnityEngine;

public class PickUpHandler : MonoBehaviour
{
    [Tooltip("Multiplier to apply to the player's charge multiplier when picked up.")]
    [SerializeField] private float pickupMultiplier = 1.5f;

    [Tooltip("How many charged jumps the multiplier should apply to.")]
    [SerializeField] private int uses = 5;

    private void OnTriggerEnter(Collider other)
    {
        if (CompareTag("Player"))
        {
            var player = other; //.GetComponent<PlayerController>();

            if (CompareTag("PropaneTank"))
            {
                if (player != null)
                {
                    player.GetComponent<PlayerController>().ApplyChargeMultiplier(pickupMultiplier, uses);

                    Destroy(gameObject);
                }
            }

            if (CompareTag("ExplPickUp"))
            {
                player.GetComponent<PlayerStats>().CoinGainHandler();

                Destroy(gameObject);
            }
        }

        else
        {
            return;
        }

    }
}