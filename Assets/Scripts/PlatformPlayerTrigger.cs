using UnityEngine;

/// <summary>
/// Place on a trigger collider. Activates a TriggerActivatedPlatform when the player enters.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlatformPlayerTrigger : MonoBehaviour
{
    [SerializeField] private TriggerActivatedPlatform platform;

    private void Awake()
    {
        var collider = GetComponent<Collider>();
        collider.isTrigger = true;

        if (platform == null)
            platform = GetComponentInParent<TriggerActivatedPlatform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        platform?.ActivateFromPlayer();
    }
}
