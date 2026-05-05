using UnityEngine;

public class PickUpHandler : MonoBehaviour
{
    [Tooltip("Multiplier to apply to the player's charge multiplier when picked up.")]
    [SerializeField] private float pickupMultiplier = 1.5f;

    [Tooltip("How many charged jumps the multiplier should apply to.")]
    [SerializeField] private int uses = 5;

    [Tooltip("Ammount of credits per pickup")]
    [SerializeField] private int coins = 50;

    [Tooltip("Seconds before this pickup respawns after being collected.")]
    [SerializeField] private float respawnDelaySeconds = 10f;

    private bool isRespawning;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Collider pickupCollider;
    private Renderer[] pickupRenderers;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        pickupCollider = GetComponent<Collider>();
        pickupRenderers = GetComponentsInChildren<Renderer>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other; //.GetComponent<PlayerController>();

            if (CompareTag("PropaneTankPickUp"))
            {
                if (isRespawning)
                {
                    return;
                }

                if (player != null)
                {
                    player.GetComponent<PlayerController>().ApplyChargeMultiplier(pickupMultiplier, uses);

                    StartCoroutine(RespawnRoutine());
                }
            }

            if (CompareTag("ExplPickUp"))
            {
                player.GetComponent<PlayerStats>().CoinGainHandler(coins);

                gameObject.SetActive(false);
            }
        }

        else
        {
            return;
        }

    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // Deactivate the GameObject's components.
        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        for (int i = 0; i < pickupRenderers.Length; i++)
        {
            pickupRenderers[i].enabled = false;
        }

        yield return new WaitForSeconds(respawnDelaySeconds);

        transform.SetPositionAndRotation(initialPosition, initialRotation);

        if (pickupCollider != null)
        {
            pickupCollider.enabled = true;
        }

        for (int i = 0; i < pickupRenderers.Length; i++)
        {
            pickupRenderers[i].enabled = true;
        }

        isRespawning = false;
    }
}