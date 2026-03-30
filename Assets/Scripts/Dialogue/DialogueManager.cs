using UnityEngine;
using UnityEngine.UIElements;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueSequence sequence;
    public string localizationFile = "Beginner";   // Changes dialog per scene 

    [Header("UI Toolkit")]
    [SerializeField] private UIDocument dialogueUIDocument;

    // References to UI elements for displaying 
    private VisualElement root;
    private Image iconElement;
    private Label alertNameElement;
    private Label messageElement;
    private Button nextButton;

    private DialogueQueue queue = new DialogueQueue(); // Custom queue to manage dialogue items.

    void Start()
    {
        if (dialogueUIDocument == null)
        {
            Debug.LogError("UIDocument is not assigned. pls fix");
            return;
        }

        if (sequence == null)
        {
            Debug.LogError("DialogueSequence is not assigned. should fix");
            return;
        }

        // Setup UI references.
        root = dialogueUIDocument.rootVisualElement;

        // find the UI elements by name.
        iconElement = root.Q<Image>("icon");
        alertNameElement = root.Q<Label>("alert-name");
        messageElement = root.Q<Label>("message");
        nextButton = root.Q<Button>("next-button");

        if (nextButton != null)
            nextButton.clicked += ShowNextDialogue;

        // Load the JSON for this level
        LocalizationManager.LoadLanguage(localizationFile);

        // Fill the queue from the sequence
        foreach (DialogueItem item in sequence.dialogues)
        {
            queue.Enqueue(item);
        }

        ShowNextDialogue();
    }

    public void ShowNextDialogue()
    {
        // disable Next button here or end the dialogue.
        if (queue.IsEmpty())
        {
            if (messageElement != null)
                messageElement.text = "End of dialogue. Level ready!";
            if (nextButton != null)
                nextButton.text = "Close";
            return;
        }

        DialogueItem item = queue.Dequeue();

        // Update UI with current dialogue item.
        if (alertNameElement != null)
        {
            alertNameElement.text = item.alertName;
        }
        if (messageElement != null)
        {
            messageElement.text = LocalizationManager.GetText(item.textID);

        }
        if (iconElement != null && item.icon != null)
        {
            iconElement.sprite = item.icon;
        }
    }
}