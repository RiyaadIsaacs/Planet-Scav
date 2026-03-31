using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public int cost = 80;                    // How much money this upgrade costs
    public string upgradeName = "Speed Booster";

    private bool playerInRange = false;
    private UIManager uiManager;
    private PlayerController playerController;

    private void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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