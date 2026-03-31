using UnityEngine;

// Calls UIManager to start a new dialogue sequence when the player enters the trigger zone.
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

        // Find the UIManager and start new dialogue.
        UIManager dm = GameObject.FindFirstObjectByType<UIManager>();
        if (dm != null)
        {
            dm.StartNewDialogue(dialogueSequence, localizationFile);
            hasTriggered = true;
        }
        else
        {
            Debug.LogError("UIManager not found in scene!");
        }
    }
}