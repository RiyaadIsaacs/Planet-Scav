using UnityEngine;

public class PropaneTank : MonoBehaviour
{
    [Tooltip("Multiplier to apply to the player's charge multiplier when picked up.")]
    [SerializeField] private float pickupMultiplier = 1.5f;

    [Tooltip("How many charged jumps the multiplier should apply to.")]
    [SerializeField] private int uses = 5;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ApplyChargeMultiplier(pickupMultiplier, uses);

            Destroy(gameObject);
        }
    }
}
