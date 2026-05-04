using UnityEngine;

// Calls DialogueUIManager to start a new dialogue sequence when the player enters the trigger zone.
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue to Play")]
    public DialogueSequence dialogueSequence; 
    public string localizationFile = "Beginner";

    private bool hasTriggered = false; // Check if player has already triggered dialogue.

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        // Find the DialogueUIManager and start new dialogue.
        DialogueUIManager dm = GameObject.FindFirstObjectByType<DialogueUIManager>();
        if (dm != null)
        {
            dm.StartNewDialogue(dialogueSequence, localizationFile);
            hasTriggered = true;
        }
        else
        {
            Debug.LogError("DialogueUIManager not found in scene!");
        }
    }
}