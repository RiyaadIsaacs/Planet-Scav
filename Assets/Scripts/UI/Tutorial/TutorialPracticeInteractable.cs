using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple practice object for the tutorial "press E to interact" step.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TutorialPracticeInteractable : MonoBehaviour
{
    [SerializeField] private string interactMessage = "Terminal activated. Nice work!";
    [SerializeField] private GameObject activatedVisual;

    private bool playerInRange;
    private bool activated;
    private DialogueUIManager dialogueUI;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || activated)
            return;

        playerInRange = true;
        ResolveDialogueUI();
        dialogueUI?.ShowInteractPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        dialogueUI?.ShowInteractPrompt(false);
    }

    private void Update()
    {
        if (!playerInRange || activated)
            return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Activate();
    }

    private void Activate()
    {
        activated = true;
        playerInRange = false;
        dialogueUI?.ShowInteractPrompt(false);

        if (activatedVisual != null)
            activatedVisual.SetActive(true);

        SFXManager.Play(SFXManager.NotificationSoundId);
        Debug.Log(interactMessage);
    }

    private void ResolveDialogueUI()
    {
        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<DialogueUIManager>();
    }
}
