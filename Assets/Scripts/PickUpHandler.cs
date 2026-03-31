using UnityEngine;

public class PickUpHandler : MonoBehaviour
{
    [Tooltip("Multiplier to apply to the player's charge multiplier when picked up.")]
    [SerializeField] private float pickupMultiplier = 1.5f;

    [Tooltip("How many charged jumps the multiplier should apply to.")]
    [SerializeField] private int uses = 5;

    [Tooltip("Ammount of credits per pickup")]
    [SerializeField] private int coins = 50;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other; //.GetComponent<PlayerController>();

            if (CompareTag("PropaneTankPickUp"))
            {
                if (player != null)
                {
                    player.GetComponent<PlayerController>().ApplyChargeMultiplier(pickupMultiplier, uses);

                    Destroy(gameObject);
                }
            }

            if (CompareTag("ExplPickUp"))
            {
                player.GetComponent<PlayerStats>().CoinGainHandler(coins);

                Destroy(gameObject);
            }
        }

        else
        {
            return;
        }

    }
}