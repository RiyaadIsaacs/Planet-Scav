using UnityEngine;

// Calls DialogueManager to start a new dialogue sequence when the player enters the trigger zone.
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

        // Find the DialogueManager and start new dialogue.
        DialogueManager dm = GameObject.FindFirstObjectByType<DialogueManager>();
        if (dm != null)
        {
            dm.StartNewDialogue(dialogueSequence, localizationFile);
            hasTriggered = true;
        }
        else
        {
            Debug.LogError("DialogueManager not found in scene!");
        }
    }
}