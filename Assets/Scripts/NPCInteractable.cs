using UnityEngine;
using UnityEngine.InputSystem;

public enum VendorUpgradeType
{
    SpeedBooster,
    BossKillerShot
}

public class NPCInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public int cost = 80;
    public string upgradeName = "Speed Booster";
    public VendorUpgradeType upgradeType = VendorUpgradeType.SpeedBooster;

    private bool playerInRange = false;
    private DialogueUIManager uiManager;
    private PlayerController playerController;

    private void ResolveReferences(Collider otherTrigger)
    {
        if (uiManager == null)
            uiManager = FindFirstObjectByType<DialogueUIManager>();

        if (playerController == null)
        {
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

        if (Keyboard.current.eKey.wasPressedThisFrame)
            TryInteract();
    }

    private void TryInteract()
    {
        ResolveReferences(null);
        if (playerController == null) return;

        if (!playerController.CanSpendMoney(cost))
            return;

        var stats = playerController.GetComponent<PlayerStats>();
        if (stats == null)
            return;

        switch (upgradeType)
        {
            case VendorUpgradeType.SpeedBooster:
                stats.upgradeCheck = true;
                break;
            case VendorUpgradeType.BossKillerShot:
                var shooting = playerController.GetComponent<PlayerShooting>();
                if (shooting != null)
                    shooting.GrantBossKillerShot();
                else
                    Debug.LogWarning("NPCInteractable: BossKillerShot purchased but PlayerShooting is missing.");
                break;
        }

        stats.CoinGainHandler(-cost);

        if (uiManager != null)
            uiManager.ShowInteractPrompt(false);
    }
}
