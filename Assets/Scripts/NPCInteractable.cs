using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public int cost = 80;                    // How much money this upgrade costs
    public string upgradeName = "Speed Booster";

    private bool playerInRange = false;
    private DialogueUIManager uiManager;
    private PlayerController playerController;

    private void ResolveReferences(Collider otherTrigger)
    {
        // With spawn-on-demand, the player may not exist yet when Start() runs.
        if (uiManager == null)
            uiManager = FindFirstObjectByType<DialogueUIManager>();

        if (playerController == null)
        {
            // Prefer grabbing the controller from the player that entered the trigger.
            if (otherTrigger != null)
                playerController = otherTrigger.GetComponentInParent<PlayerController>();

            if (playerController == null)
                playerController = FindFirstObjectByType<PlayerController>();
        }
    }

    private void Start()
    {
        uiManager = FindFirstObjectByType<DialogueUIManager>();
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ResolveReferences(other);
            playerInRange = true;
            if (uiManager != null)
                uiManager.ShowInteractPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (uiManager != null)
                uiManager.ShowInteractPrompt(false);
        }
    }

    private void Update()
    {
        if (!playerInRange) return;

        // Check for E key press using new Input System because lazy and didnt want to set up in playerController.
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        ResolveReferences(null);
        if (playerController == null) return;

        if (playerController.CanSpendMoney(cost))
        {
            playerController.GetComponent<PlayerStats>().upgradeCheck = true; // Grant the upgrade to the player.

            playerController.GetComponent<PlayerStats>().CoinGainHandler(-cost); // Deduct cost from player's credits.

            // Hide prompt after successful purchase.
            if (uiManager != null)
                uiManager.ShowInteractPrompt(false);
        }
    }
}