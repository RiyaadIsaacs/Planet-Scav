using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueSequence sequence; // Assigned dialogue sequence for the level 

    // References to UI elements for displaying dialogue from the dialogue items and json 
    [Header("UI References")]
    public Text alertNameText;
    public Text dialogueText;
    public Image iconImage;

    private DialogueQueue queue = new DialogueQueue(); // Custom queue to manage dialogue items

    void Start()
    {
        // Load the correct JSON for this level
        LocalizationManager.LoadLanguage("Beginner");

        // Fill the queue from the sequence
        foreach (DialogueItem item in sequence.dialogues)
        {
            queue.Enqueue(item);
        }

        ShowNextDialogue();
    }

    public void ShowNextDialogue()
    {
        if (queue.IsEmpty())
        {
            dialogueText.text = "End of dialogue.";
            // disable Next button here or end the dialogue 
            return;
        }

        DialogueItem item = queue.Dequeue(); // Get the next dialogue item through dequeueing

        alertNameText.text = item.alertName;
        iconImage.sprite = item.icon;

        dialogueText.text = LocalizationManager.GetText(item.textID);
    }
}